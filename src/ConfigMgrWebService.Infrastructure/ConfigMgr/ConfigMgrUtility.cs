using Microsoft.ConfigurationManagement.ManagementProvider;
using Microsoft.ConfigurationManagement.ManagementProvider.WqlQueryEngine;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Management.Instrumentation;

namespace ConfigMgrWebService.Infrastructure.ConfigMgr;

/// <summary>
/// ConfigMgr utility for automating ConfigMgr operations via WMI
/// Modernized for .NET 8 with async/await and dependency injection
/// </summary>
public class ConfigMgrUtility : IDisposable
{
    #region Constants

    private const string WmiSelectQuery = "SELECT * FROM {0}";
    private const string WmiSelectQueryWhereString = "SELECT * FROM {0} WHERE {1} = '{2}'";
    private const string WmiSelectQueryWhereStringAndString = "SELECT * FROM {0} WHERE {1} = '{2}' AND {3} = '{4}'";
    private const string WmiSelectQueryWhereStringAndCustom = "SELECT * FROM {0} WHERE {1} = '{2}' AND {3}";
    private const string WmiSelectQueryWhereInteger = "SELECT * FROM {0} WHERE {1} = {2}";
    private const string WmiDirectReferenceString = "{0}.{1}='{2}'";
    private const string WmiDirectReferenceInt = "{0}.{1}={2}";

    #endregion

    #region Fields

    private readonly WqlConnectionManager _managementServer;
    private readonly ILogger<ConfigMgrUtility> _logger;
    private readonly string _siteCode;
    private bool _isDisposed;

    #endregion

    #region Properties

    public string SccmServer { get; }
    public bool IsConnected { get; }
    public WqlConnectionManager SccmConnection => _managementServer;

    #endregion

    #region Enums

    public enum AddComputerTypes
    {
        MacAddress,
        SmBiosGuid
    }

    public enum CollectionAction
    {
        AddMembershipRule,
        DeleteMembershipRule
    }

    public enum DeviceAffinityTypes
    {
        SoftwareCatalog = 1,
        Administrator = 2,
        User = 3,
        UsageAgent = 4,
        DeviceManagement = 5,
        Osd = 6,
        FastInstall = 7,
        ExchangeConnector = 8
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of ConfigMgrUtility
    /// </summary>
    /// <param name="sccmServer">ConfigMgr Site Server hostname</param>
    /// <param name="userName">Username for authentication (optional, uses current credentials if empty)</param>
    /// <param name="password">Password for authentication (optional)</param>
    /// <param name="logger">Logger instance</param>
    public ConfigMgrUtility(string sccmServer, string userName, string password, ILogger<ConfigMgrUtility> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        SccmServer = sccmServer ?? throw new ArgumentNullException(nameof(sccmServer));

        var namedValues = new SmsNamedValuesDictionary();
        _managementServer = new WqlConnectionManager(namedValues);

        try
        {
            IsConnected = string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password)
                ? _managementServer.Connect(SccmServer)
                : _managementServer.Connect(SccmServer, userName, password);

            if (!IsConnected)
            {
                _logger.LogError("Unable to connect to ConfigMgr server {SccmServer}", SccmServer);
                throw new InvalidOperationException($"Unable to connect to {SccmServer}");
            }

            // Get site code
            using var smsSiteResult = _managementServer.QueryProcessor.ExecuteQuery(
                string.Format(WmiSelectQuery, "SMS_Site"));

            foreach (IResultObject site in smsSiteResult)
            {
                _siteCode = site["SiteCode"].StringValue;
                break;
            }

            _logger.LogInformation("Successfully connected to ConfigMgr server {SccmServer}, Site Code: {SiteCode}",
                SccmServer, _siteCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to ConfigMgr server {SccmServer}", SccmServer);
            throw;
        }
    }

    /// <summary>
    /// Initializes a new instance using local server with current credentials
    /// </summary>
    public ConfigMgrUtility(ILogger<ConfigMgrUtility> logger)
        : this(System.Net.Dns.GetHostName(), string.Empty, string.Empty, logger)
    {
    }

    #endregion

    #region Computer Management

    /// <summary>
    /// Adds a new computer to ConfigMgr by SMBIOS GUID
    /// </summary>
    public async Task<int> AddNewComputerBySmBiosGuidAsync(string computerName, string smBiosGuid)
    {
        return await Task.Run(() => AddNewComputer(computerName, smBiosGuid, AddComputerTypes.SmBiosGuid));
    }

    /// <summary>
    /// Adds a new computer to ConfigMgr by MAC Address
    /// </summary>
    public async Task<int> AddNewComputerByMacAddressAsync(string computerName, string macAddress)
    {
        return await Task.Run(() => AddNewComputer(computerName, macAddress, AddComputerTypes.MacAddress));
    }

    /// <summary>
    /// Adds a new computer to ConfigMgr
    /// </summary>
    private int AddNewComputer(string computerName, string macAddressOrBiosGuid, AddComputerTypes addComputerType)
    {
        if (string.IsNullOrEmpty(macAddressOrBiosGuid))
        {
            throw new ArgumentNullException(nameof(macAddressOrBiosGuid),
                "MAC address or BIOS GUID must have a value");
        }

        _logger.LogInformation("Adding new computer {ComputerName} to ConfigMgr", computerName);

        var computerParameter = new Dictionary<string, object>
        {
            { "NetbiosName", computerName }
        };

        switch (addComputerType)
        {
            case AddComputerTypes.MacAddress:
                var macAddress = macAddressOrBiosGuid.Replace("-", ":");
                computerParameter.Add("MACAddress", macAddress);
                _logger.LogDebug("Adding computer by MAC address: {MacAddress}", macAddress);
                break;

            case AddComputerTypes.SmBiosGuid:
                computerParameter.Add("SMBIOSGUID", macAddressOrBiosGuid);
                _logger.LogDebug("Adding computer by SMBIOS GUID: {SmBiosGuid}", macAddressOrBiosGuid);
                break;
        }

        computerParameter.Add("OverwriteExistingRecord", false);

        using var outParameter = _managementServer.ExecuteMethod("SMS_Site", "ImportMachineEntry", computerParameter);
        var resourceId = outParameter["ResourceID"].IntegerValue;

        _logger.LogInformation("Successfully added computer {ComputerName} with Resource ID {ResourceId}",
            computerName, resourceId);

        return resourceId;
    }

    /// <summary>
    /// Deletes a computer from ConfigMgr by name
    /// </summary>
    public async Task<int> DeleteComputerByNameAsync(string computerName)
    {
        return await Task.Run(() => DeleteComputer(computerName, false));
    }

    /// <summary>
    /// Deletes a computer from ConfigMgr by SMBIOS GUID
    /// </summary>
    public async Task<int> DeleteComputerBySmBiosGuidAsync(string smBiosGuid)
    {
        return await Task.Run(() => DeleteComputer(smBiosGuid, true));
    }

    /// <summary>
    /// Deletes a computer from ConfigMgr
    /// </summary>
    private int DeleteComputer(string identifier, bool isGuid)
    {
        _logger.LogInformation("Deleting computer {Identifier} from ConfigMgr", identifier);

        var deletedCount = 0;
        var fieldName = isGuid ? "SMBIOSGUID" : "Name";

        using var computers = _managementServer.QueryProcessor.ExecuteQuery(
            string.Format(WmiSelectQueryWhereString, "SMS_R_SYSTEM", fieldName, identifier));

        foreach (IResultObject computer in computers)
        {
            using (computer)
            {
                computer.Delete();
                deletedCount++;
            }
        }

        _logger.LogInformation("Deleted {Count} computer(s) with {FieldName} = {Identifier}",
            deletedCount, fieldName, identifier);

        return deletedCount;
    }

    /// <summary>
    /// Checks if a computer exists in ConfigMgr by name
    /// </summary>
    public async Task<bool> ClientExistsByNameAsync(string clientName)
    {
        return await Task.Run(() =>
        {
            try
            {
                var client = GetClient(clientName);
                return client != null;
            }
            catch
            {
                return false;
            }
        });
    }

    /// <summary>
    /// Checks if a computer exists in ConfigMgr by Resource ID
    /// </summary>
    public async Task<bool> ClientExistsByResourceIdAsync(string resourceId)
    {
        return await Task.Run(() =>
        {
            try
            {
                var client = GetClientByResourceId(resourceId);
                return client != null;
            }
            catch
            {
                return false;
            }
        });
    }

    /// <summary>
    /// Checks if a computer exists in ConfigMgr by SMBIOS GUID
    /// </summary>
    public async Task<bool> ClientExistsByUuidAsync(string smBiosGuid)
    {
        return await Task.Run(() =>
        {
            try
            {
                var client = GetClientByUuid(smBiosGuid);
                return client != null;
            }
            catch
            {
                return false;
            }
        });
    }

    /// <summary>
    /// Clears the last PXE advertisement for a computer
    /// </summary>
    public async Task ClearLastPxeAdvertisementAsync(string computerName)
    {
        await Task.Run(() =>
        {
            _logger.LogInformation("Clearing last PXE advertisement for {ComputerName}", computerName);

            var computer = GetClient(computerName);
            var inParams = new Dictionary<string, object>
            {
                { "ResourceIDs", new[] { computer.ResourceId } }
            };

            using var outParams = _managementServer.ExecuteMethod(
                "SMS_Collection", "ClearLastNBSAdvForMachines", inParams);

            if (outParams == null || outParams["StatusCode"].IntegerValue != 0)
            {
                _logger.LogError("Failed to clear PXE advertisement for {ComputerName}", computerName);
                throw new InvalidOperationException(
                    $"Failed to clear last PXE advertisement for {computerName}");
            }

            _logger.LogInformation("Successfully cleared PXE advertisement for {ComputerName}", computerName);
        });
    }

    /// <summary>
    /// Gets the ConfigMgr Resource ID by computer name
    /// </summary>
    public async Task<int> GetComputerResourceIdByNameAsync(string computerName)
    {
        return await Task.Run(() =>
        {
            var computer = GetClient(computerName);
            return computer.ResourceId;
        });
    }

    /// <summary>
    /// Gets the SMSM GUID for a computer
    /// </summary>
    public async Task<string> GetSmsGuidAsync(string computerName)
    {
        return await Task.Run(() =>
        {
            var computer = GetClient(computerName);
            return computer.SmsUniqueIdentifier;
        });
    }

    #endregion

    #region Collection Management

    /// <summary>
    /// Creates a device collection
    /// </summary>
    public async Task<string> CreateDeviceCollectionAsync(
        string collectionName,
        string collectionDescription,
        string limitingCollection)
    {
        return await Task.Run(() =>
        {
            _logger.LogInformation("Creating device collection {CollectionName}", collectionName);

            var limitingCollectionId = GetCollectionIdByName(limitingCollection);

            using var newCollection = _managementServer.CreateInstance("SMS_Collection");
            newCollection["Name"].StringValue = collectionName;
            newCollection["Comment"].StringValue = collectionDescription;
            newCollection["CollectionType"].IntegerValue = 2; // Device collection
            newCollection["LimitToCollectionID"].StringValue = limitingCollectionId;

            newCollection.Put();

            var collectionId = newCollection["CollectionID"].StringValue;

            _logger.LogInformation("Created device collection {CollectionName} with ID {CollectionId}",
                collectionName, collectionId);

            return collectionId;
        });
    }

    /// <summary>
    /// Gets collection ID by name
    /// </summary>
    public async Task<string> GetCollectionIdByNameAsync(string collectionName)
    {
        return await Task.Run(() => GetCollectionIdByName(collectionName));
    }

    /// <summary>
    /// Gets collection ID by name (sync version)
    /// </summary>
    private string GetCollectionIdByName(string collectionName)
    {
        using var collection = _managementServer.QueryProcessor.ExecuteQuery(
            string.Format(WmiSelectQueryWhereString, "SMS_Collection", "Name", collectionName));

        foreach (IResultObject currentCollection in collection)
        {
            using (currentCollection)
            {
                return currentCollection["CollectionID"].StringValue;
            }
        }

        throw new InstanceNotFoundException(
            $"Unable to find a collection with name {collectionName}");
    }

    /// <summary>
    /// Gets all members of a collection
    /// </summary>
    public async Task<List<string>> GetMembersOfCollectionAsync(string collectionId)
    {
        return await Task.Run(() =>
        {
            var result = new List<string>();

            using var collection = _managementServer.QueryProcessor.ExecuteQuery(
                string.Format(WmiSelectQueryWhereString, "SMS_FullCollectionMembership", "CollectionID", collectionId));

            foreach (IResultObject member in collection)
            {
                using (member)
                {
                    result.Add(member["Name"].StringValue);
                }
            }

            return result;
        });
    }

    /// <summary>
    /// Adds a member to a collection directly
    /// </summary>
    public async Task AddMemberToCollectionDirectByNameAsync(string collectionId, string clientName)
    {
        await Task.Run(() =>
        {
            _logger.LogInformation("Adding {ClientName} to collection {CollectionId}", clientName, collectionId);

            using var collection = GetCollection(collectionId);
            var client = GetClient(clientName);

            ChangeMemberInCollectionDirect(collection, client, CollectionAction.AddMembershipRule);

            _logger.LogInformation("Successfully added {ClientName} to collection {CollectionId}",
                clientName, collectionId);
        });
    }

    /// <summary>
    /// Removes a member from a collection directly
    /// </summary>
    public async Task RemoveMemberFromCollectionDirectByNameAsync(string collectionId, string clientName)
    {
        await Task.Run(() =>
        {
            _logger.LogInformation("Removing {ClientName} from collection {CollectionId}",
                clientName, collectionId);

            using var collection = GetCollection(collectionId);
            var client = GetClient(clientName);

            ChangeMemberInCollectionDirect(collection, client, CollectionAction.DeleteMembershipRule);

            _logger.LogInformation("Successfully removed {ClientName} from collection {CollectionId}",
                clientName, collectionId);
        });
    }

    /// <summary>
    /// Updates collection membership
    /// </summary>
    public async Task UpdateCollectionMembershipAsync(string collectionId)
    {
        await Task.Run(() =>
        {
            _logger.LogInformation("Updating membership for collection {CollectionId}", collectionId);

            using var collection = _managementServer.GetInstance(
                string.Format(WmiDirectReferenceString, "SMS_Collection", "CollectionID", collectionId));

            collection.ExecuteMethod("RequestRefresh", null);

            _logger.LogInformation("Successfully requested refresh for collection {CollectionId}", collectionId);
        });
    }

    #endregion

    #region User Management

    /// <summary>
    /// Gets primary users of a computer
    /// </summary>
    public async Task<string[]> GetPrimaryUsersAsync(string computerName)
    {
        return await Task.Run(() =>
        {
            var relatedUser = new List<string>();

            using var machineRelationship = _managementServer.QueryProcessor.ExecuteQuery(
                string.Format(WmiSelectQueryWhereStringAndString,
                    "SMS_UserMachineRelationship", "Sources", "2", "ResourceName", computerName));

            foreach (IResultObject machine in machineRelationship)
            {
                using (machine)
                {
                    relatedUser.Add(machine["UniqueUserName"].StringValue);
                }
            }

            return relatedUser.ToArray();
        });
    }

    /// <summary>
    /// Sets the primary user for a computer
    /// </summary>
    public async Task SetPrimaryUserAsync(
        string computerName,
        string userName,
        DeviceAffinityTypes deviceAffinityType)
    {
        await Task.Run(() =>
        {
            _logger.LogInformation("Setting primary user {UserName} for computer {ComputerName}",
                userName, computerName);

            var clientId = GetComputerResourceIdByName(computerName);
            var user = GetUser(userName);

            var inParameters = new Dictionary<string, object>
            {
                { "MachineResourceID", clientId },
                { "UserAccountName", user.UniqueUserName },
                { "SourceId", deviceAffinityType },
                { "TypeId", 1 }
            };

            using var retVal = _managementServer.ExecuteMethod(
                "SMS_UserMachineRelationShip", "CreateRelationShip", inParameters);

            if (retVal["ReturnValue"].IntegerValue != 0)
            {
                throw new InvalidOperationException(
                    $"Failed to create user device affinity for {computerName} with user {user.UniqueUserName}");
            }

            _logger.LogInformation("Successfully set primary user {UserName} for computer {ComputerName}",
                userName, computerName);
        });
    }

    /// <summary>
    /// Deletes the primary user for a computer
    /// </summary>
    public async Task DeletePrimaryUserAsync(string computerName, string userName)
    {
        await Task.Run(() =>
        {
            _logger.LogInformation("Deleting primary user {UserName} for computer {ComputerName}",
                userName, computerName);

            var clientId = GetComputerResourceIdByName(computerName);

            using var relationships = _managementServer.QueryProcessor.ExecuteQuery(
                string.Format(WmiSelectQueryWhereInteger, "SMS_UserMachineRelationShip", "ResourceID", clientId));

            var userToRemove = userName.Split('\\');

            foreach (IResultObject relationship in relationships)
            {
                using (relationship)
                {
                    var existingName = relationship.Properties["UniqueUserName"].StringValue;
                    if (existingName.Contains(userToRemove[^1], StringComparison.OrdinalIgnoreCase))
                    {
                        relationship.Delete();
                        break;
                    }
                }
            }

            _logger.LogInformation("Successfully deleted primary user {UserName} for computer {ComputerName}",
                userName, computerName);
        });
    }

    #endregion

    #region USMT Operations

    /// <summary>
    /// Adds a USMT computer association
    /// </summary>
    public async Task<bool> AddUsmtComputerAssociationAsync(
        string sourceComputerName,
        string destinationComputerName)
    {
        return await Task.Run(() =>
        {
            _logger.LogInformation("Creating USMT association from {SourceComputer} to {DestinationComputer}",
                sourceComputerName, destinationComputerName);

            var sourceResourceId = GetResourceIdByComputerName(sourceComputerName);
            var destinationResourceId = GetResourceIdByComputerName(destinationComputerName);

            var associationParameters = new Dictionary<string, object>
            {
                { "SourceClientResourceID", sourceResourceId },
                { "RestoreClientResourceID", destinationResourceId }
            };

            using var outParameter = _managementServer.ExecuteMethod(
                "SMS_StateMigration", "AddAssociation", associationParameters);

            var returnValue = outParameter["ReturnValue"].IntegerValue;

            if (returnValue != 0)
            {
                throw new InvalidOperationException(
                    $"SCCM returned error code {returnValue} when creating association");
            }

            _logger.LogInformation("Successfully created USMT association from {SourceComputer} to {DestinationComputer}",
                sourceComputerName, destinationComputerName);

            return true;
        });
    }

    /// <summary>
    /// Removes a USMT computer association
    /// </summary>
    public async Task<bool> RemoveUsmtComputerAssociationAsync(
        string sourceComputerName,
        string destinationComputerName)
    {
        return await Task.Run(() =>
        {
            _logger.LogInformation("Deleting USMT association from {SourceComputer} to {DestinationComputer}",
                sourceComputerName, destinationComputerName);

            var sourceResourceId = GetResourceIdByComputerName(sourceComputerName);
            var destinationResourceId = GetResourceIdByComputerName(destinationComputerName);

            var associationParameters = new Dictionary<string, object>
            {
                { "SourceClientResourceID", sourceResourceId },
                { "RestoreClientResourceID", destinationResourceId }
            };

            using var outParameter = _managementServer.ExecuteMethod(
                "SMS_StateMigration", "DeleteAssociation", associationParameters);

            var returnValue = outParameter["ReturnValue"].IntegerValue;

            if (returnValue != 0)
            {
                throw new InvalidOperationException(
                    $"SCCM returned error code {returnValue} when deleting association");
            }

            _logger.LogInformation("Successfully deleted USMT association from {SourceComputer} to {DestinationComputer}",
                sourceComputerName, destinationComputerName);

            return true;
        });
    }

    /// <summary>
    /// Gets USMT migration status
    /// </summary>
    public async Task<string> GetUsmtMigrationStatusAsync(
        string sourceComputerName,
        string destinationComputerName)
    {
        return await Task.Run(() =>
        {
            try
            {
                var query = $"SELECT MigrationStatus FROM SMS_StateMigration " +
                           $"WHERE SourceName = '{sourceComputerName}' AND RestoreName = '{destinationComputerName}'";

                using var queryResults = _managementServer.QueryProcessor.ExecuteQuery(query);

                foreach (IResultObject result in queryResults)
                {
                    using (result)
                    {
                        var status = result["MigrationStatus"].IntegerValue;

                        return status switch
                        {
                            0 => "NOTSTARTED",
                            1 => "INPROGRESS",
                            2 => "COMPLETED",
                            _ => "Unknown status value"
                        };
                    }
                }

                return "No matching records found";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting USMT migration status");
                return $"Error: {ex.Message}";
            }
        });
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Gets a client by name
    /// </summary>
    private SmsClient GetClient(string clientName)
    {
        var wmiQuery = string.Format(WmiSelectQueryWhereStringAndCustom,
            "SMS_R_System", "Name", clientName, "(Obsolete = 0 OR Obsolete IS NULL)");

        using var resultObject = _managementServer.QueryProcessor.ExecuteQuery(wmiQuery);

        foreach (IResultObject wqlResultObject in resultObject)
        {
            using (wqlResultObject)
            {
                return new SmsClient(wqlResultObject);
            }
        }

        throw new InstanceNotFoundException($"Unable to find client with name {clientName}");
    }

    /// <summary>
    /// Gets a client by Resource ID
    /// </summary>
    private SmsClient GetClientByResourceId(string resourceId)
    {
        using var resultObject = _managementServer.QueryProcessor.ExecuteQuery(
            string.Format(WmiSelectQueryWhereString, "SMS_R_System", "ResourceID", resourceId));

        foreach (IResultObject wqlResultObject in resultObject)
        {
            using (wqlResultObject)
            {
                return new SmsClient(wqlResultObject);
            }
        }

        throw new InstanceNotFoundException($"Unable to find client with ResourceID {resourceId}");
    }

    /// <summary>
    /// Gets a client by UUID/SMBIOS GUID
    /// </summary>
    private SmsClient GetClientByUuid(string smBiosGuid)
    {
        using var resultObject = _managementServer.QueryProcessor.ExecuteQuery(
            string.Format(WmiSelectQueryWhereString, "SMS_R_System", "SMBIOSGUID", smBiosGuid));

        foreach (IResultObject wqlResultObject in resultObject)
        {
            using (wqlResultObject)
            {
                return new SmsClient(wqlResultObject);
            }
        }

        throw new InstanceNotFoundException($"Unable to find client with UUID {smBiosGuid}");
    }

    /// <summary>
    /// Gets Resource ID by computer name
    /// </summary>
    private int GetResourceIdByComputerName(string computerName)
    {
        if (string.IsNullOrEmpty(computerName))
        {
            throw new ArgumentNullException(nameof(computerName), "Computer Name cannot be null or empty");
        }

        using var resultObject = _managementServer.QueryProcessor.ExecuteQuery(
            $"SELECT ResourceID FROM SMS_R_System WHERE Name = '{computerName}'");

        foreach (IResultObject wqlResultObject in resultObject)
        {
            using (wqlResultObject)
            {
                return wqlResultObject["ResourceID"].IntegerValue;
            }
        }

        throw new InstanceNotFoundException($"No computer found with the name {computerName}");
    }

    /// <summary>
    /// Gets Resource ID by computer name
    /// </summary>
    private int GetComputerResourceIdByName(string computerName)
    {
        var computer = GetClient(computerName);
        return computer.ResourceId;
    }

    /// <summary>
    /// Gets a collection
    /// </summary>
    private IResultObject GetCollection(string collectionId)
    {
        using var collections = _managementServer.QueryProcessor.ExecuteQuery(
            string.Format(WmiSelectQueryWhereString, "SMS_Collection", "CollectionID", collectionId));

        foreach (IResultObject collection in collections)
        {
            collection.Get();
            return collection;
        }

        throw new InstanceNotFoundException($"Unable to find collection with ID {collectionId}");
    }

    /// <summary>
    /// Gets a user
    /// </summary>
    private SmsUser GetUser(string userName)
    {
        using var users = _managementServer.QueryProcessor.ExecuteQuery(
            string.Format(WmiSelectQueryWhereString, "SMS_R_User", "UniqueUserName", userName));

        foreach (IResultObject user in users)
        {
            using (user)
            {
                return new SmsUser(user);
            }
        }

        throw new InstanceNotFoundException($"Unable to find user with UserName {userName}");
    }

    /// <summary>
    /// Changes member in collection (direct membership rule)
    /// </summary>
    private void ChangeMemberInCollectionDirect(
        IResultObject collection,
        SmsClient client,
        CollectionAction action)
    {
        var collectionId = collection.Properties["CollectionID"].StringValue;
        var found = IsMemberOfCollection(client.ResourceId, collectionId);

        if (action == CollectionAction.AddMembershipRule && found)
        {
            return; // Already member
        }

        if (action == CollectionAction.DeleteMembershipRule && !found)
        {
            return; // Not member
        }

        using var rule = _managementServer.CreateInstance("SMS_CollectionRuleDirect");
        rule["ResourceID"].StringValue = client.ResourceId.ToString(CultureInfo.InvariantCulture);
        rule["ResourceClassName"].StringValue = "SMS_R_System";
        rule["RuleName"].StringValue = client.Name;

        var parameters = new Dictionary<string, object>
        {
            { "collectionRule", rule }
        };

        using var id = collection.ExecuteMethod(Enum.GetName(typeof(CollectionAction), action)!, parameters);

        if (id == null)
        {
            throw new InvalidOperationException(
                $"Failed to {action} member {client.Name} in collection {collection["Name"].StringValue}");
        }
    }

    /// <summary>
    /// Checks if member is in collection
    /// </summary>
    private bool IsMemberOfCollection(int clientId, string collectionId)
    {
        using var collectionMembers = _managementServer.QueryProcessor.ExecuteQuery(
            $"SELECT * FROM SMS_CM_RES_COLL_{collectionId}");

        foreach (IResultObject collectionMember in collectionMembers)
        {
            using (collectionMember)
            {
                if (collectionMember.Properties["ResourceID"].IntegerValue == clientId)
                {
                    return true;
                }
            }
        }

        return false;
    }

    #endregion

    #region Helper Classes

    /// <summary>
    /// Represents a ConfigMgr client
    /// </summary>
    public class SmsClient
    {
        public int ResourceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SmsUniqueIdentifier { get; set; } = string.Empty;

        public SmsClient(IResultObject resultObject)
        {
            ResourceId = resultObject["ResourceID"].IntegerValue;
            Name = resultObject["Name"].StringValue;
            SmsUniqueIdentifier = resultObject["SMSUniqueIdentifier"].StringValue;
        }
    }

    /// <summary>
    /// Represents a ConfigMgr user
    /// </summary>
    public class SmsUser
    {
        public int ResourceId { get; set; }
        public string UniqueUserName { get; set; } = string.Empty;

        public SmsUser(IResultObject resultObject)
        {
            ResourceId = resultObject["ResourceID"].IntegerValue;
            UniqueUserName = resultObject["UniqueUserName"].StringValue;
        }
    }

    #endregion

    #region IDisposable Implementation

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _managementServer?.Dispose();
            }
            _isDisposed = true;
        }
    }

    ~ConfigMgrUtility()
    {
        Dispose(false);
    }

    #endregion
}
