namespace ConfigMgrWebService.Shared.Models;

/// <summary>
/// Application configuration settings
/// </summary>
public class AppSettings
{
    public const string SectionName = "AppSettings";

    /// <summary>
    /// ConfigMgr/SCCM settings
    /// </summary>
    public ConfigMgrSettings ConfigMgr { get; set; } = new();

    /// <summary>
    /// Microsoft Graph API settings
    /// </summary>
    public GraphApiSettings GraphApi { get; set; } = new();

    /// <summary>
    /// Active Directory settings
    /// </summary>
    public ActiveDirectorySettings ActiveDirectory { get; set; } = new();

    /// <summary>
    /// Logging settings
    /// </summary>
    public LoggingSettings Logging { get; set; } = new();

    /// <summary>
    /// Authentication settings
    /// </summary>
    public AuthenticationSettings Authentication { get; set; } = new();
}

public class ConfigMgrSettings
{
    /// <summary>
    /// ConfigMgr Site Server FQDN
    /// </summary>
    public string SiteServer { get; set; } = string.Empty;

    /// <summary>
    /// Optional: Username for ConfigMgr connection
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Optional: Password for ConfigMgr connection
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Domain short name (e.g., "CONTOSO")
    /// </summary>
    public string DomainShortName { get; set; } = string.Empty;

    /// <summary>
    /// Primary user variable name
    /// </summary>
    public string PrimaryUserVariable { get; set; } = "SMSTSUDAUsers";

    /// <summary>
    /// Temporary directory for user data
    /// </summary>
    public string TemporaryUserDirectory { get; set; } = @"C:\Temp\ConfigMgrWebService\Users";

    /// <summary>
    /// OSD Collection mappings (key-value pairs)
    /// </summary>
    public Dictionary<string, string> OsDeploymentCollections { get; set; } = new();
}

public class GraphApiSettings
{
    /// <summary>
    /// Azure AD Application Display Name
    /// </summary>
    public string AppDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Azure AD Application (Client) ID
    /// </summary>
    public string AppId { get; set; } = string.Empty;

    /// <summary>
    /// Azure AD Tenant ID
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Azure AD Client Secret
    /// </summary>
    public string SecretString { get; set; } = string.Empty;

    /// <summary>
    /// Microsoft Graph API URL
    /// </summary>
    public string GraphUrl { get; set; } = "https://graph.microsoft.com/beta/";
}

public class ActiveDirectorySettings
{
    /// <summary>
    /// Active Directory Domain
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Optional: Specific DC to connect to
    /// </summary>
    public string? DomainController { get; set; }
}

public class LoggingSettings
{
    /// <summary>
    /// Log file path
    /// </summary>
    public string LogFile { get; set; } = @"C:\Logs\ConfigMgrWebService\service.log";

    /// <summary>
    /// Maximum log file size in bytes (default: 10MB)
    /// </summary>
    public long MaxLogSize { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// Maximum number of log files to retain
    /// </summary>
    public int MaxLogFiles { get; set; } = 10;

    /// <summary>
    /// Minimum log level (Verbose, Debug, Information, Warning, Error, Fatal)
    /// </summary>
    public string MinimumLevel { get; set; } = "Information";
}

public class AuthenticationSettings
{
    /// <summary>
    /// Enable Windows Authentication
    /// </summary>
    public bool EnableWindowsAuthentication { get; set; } = true;

    /// <summary>
    /// Enable API Key Authentication for system-to-system calls
    /// </summary>
    public bool EnableApiKeyAuthentication { get; set; } = true;

    /// <summary>
    /// API Keys for system-to-system authentication (key = API Key, value = Client Name)
    /// </summary>
    public Dictionary<string, string> ApiKeys { get; set; } = new();
}
