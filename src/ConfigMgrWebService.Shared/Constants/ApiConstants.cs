namespace ConfigMgrWebService.Shared.Constants;

public static class ApiConstants
{
    public const string ApiVersion = "v1";
    public const string ApiTitle = "ConfigMgr Web Service API";
    public const string ApiDescription = "REST API for Microsoft ConfigMgr, Intune and Entra ID Operations";

    public static class Headers
    {
        public const string CorrelationId = "X-Correlation-ID";
        public const string ApiKey = "X-API-Key";
    }

    public static class AuthenticationSchemes
    {
        public const string Windows = "Windows";
        public const string ApiKey = "ApiKey";
    }

    public static class Policies
    {
        public const string WindowsAuthPolicy = "WindowsAuthPolicy";
        public const string ApiKeyPolicy = "ApiKeyPolicy";
        public const string CombinedPolicy = "WindowsOrApiKey";
    }

    public static class ResponseMessages
    {
        // Success
        public const string OperationSuccess = "Operation completed successfully";
        public const string ComputerAdded = "Computer added successfully";
        public const string ComputerDeleted = "Computer deleted successfully";
        public const string UserAdded = "Primary user added successfully";
        public const string UserRemoved = "Primary user removed successfully";
        public const string CollectionUpdated = "Collection membership updated successfully";
        public const string PxeFlagCleared = "PXE flag cleared successfully";
        public const string AssociationCreated = "USMT association created successfully";
        public const string AssociationDeleted = "USMT association deleted successfully";
        public const string VariableAdded = "Variable added successfully";
        public const string VariableDeleted = "Variable deleted successfully";

        // Errors
        public const string OperationFailed = "Operation failed";
        public const string ComputerAlreadyExists = "A computer with this identifier already exists";
        public const string ComputerNotFound = "Computer not found";
        public const string UserNotFound = "User not found";
        public const string CollectionNotFound = "Collection not found";
        public const string InvalidRequest = "Invalid request parameters";
        public const string UnauthorizedAccess = "Unauthorized access";
        public const string InternalServerError = "An internal server error occurred";
        public const string ConfigMgrConnectionFailed = "Failed to connect to ConfigMgr server";
        public const string GraphApiConnectionFailed = "Failed to connect to Microsoft Graph API";
    }

    public static class LoggingEvents
    {
        public const int ApiRequest = 1000;
        public const int ApiResponse = 1001;
        public const int ConfigMgrOperation = 2000;
        public const int GraphApiOperation = 3000;
        public const int EntraOperation = 4000;
        public const int AuthenticationEvent = 5000;
        public const int ErrorEvent = 9000;
    }
}
