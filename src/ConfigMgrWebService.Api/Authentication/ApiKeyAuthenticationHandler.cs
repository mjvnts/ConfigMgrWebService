using ConfigMgrWebService.Shared.Constants;
using ConfigMgrWebService.Shared.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace ConfigMgrWebService.Api.Authentication;

/// <summary>
/// API Key Authentication Handler for system-to-system authentication
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly AppSettings _appSettings;
    private readonly ILogger<ApiKeyAuthenticationHandler> _logger;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        IOptions<AppSettings> appSettings)
        : base(options, loggerFactory, encoder)
    {
        _appSettings = appSettings.Value;
        _logger = loggerFactory.CreateLogger<ApiKeyAuthenticationHandler>();
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if API Key authentication is enabled
        if (!_appSettings.Authentication.EnableApiKeyAuthentication)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // Try to get API key from header
        if (!Request.Headers.TryGetValue(ApiConstants.Headers.ApiKey, out var apiKeyHeaderValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var providedApiKey = apiKeyHeaderValues.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(providedApiKey))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // Validate API key
        if (!_appSettings.Authentication.ApiKeys.TryGetValue(providedApiKey, out var clientName))
        {
            _logger.LogWarning("Invalid API key attempted from IP: {IpAddress}",
                Context.Connection.RemoteIpAddress);
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
        }

        // Create claims
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, clientName),
            new Claim("AuthenticationType", "ApiKey"),
            new Claim(ClaimTypes.Role, "ApiClient")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        _logger.LogInformation("API Key authentication successful for client: {ClientName}", clientName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
