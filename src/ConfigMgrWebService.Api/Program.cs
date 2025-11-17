using ConfigMgrWebService.Api.Authentication;
using ConfigMgrWebService.Api.Middleware;
using ConfigMgrWebService.Shared.Constants;
using ConfigMgrWebService.Shared.Models;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// SERILOG CONFIGURATION
// ========================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithThreadId()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .CreateLogger();

builder.Host.UseSerilog();

// ========================================
// CONFIGURATION
// ========================================
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection(AppSettings.SectionName));

// ========================================
// AUTHENTICATION
// ========================================
var appSettings = builder.Configuration.GetSection(AppSettings.SectionName).Get<AppSettings>();

// Windows Authentication
if (appSettings?.Authentication.EnableWindowsAuthentication == true)
{
    builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
        .AddNegotiate();
}

// API Key Authentication
if (appSettings?.Authentication.EnableApiKeyAuthentication == true)
{
    builder.Services.AddAuthentication()
        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
            ApiConstants.AuthenticationSchemes.ApiKey, null);
}

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    if (appSettings?.Authentication.EnableWindowsAuthentication == true)
    {
        options.AddPolicy(ApiConstants.Policies.WindowsAuthPolicy, policy =>
            policy.RequireAuthenticatedUser()
                  .AddAuthenticationSchemes(NegotiateDefaults.AuthenticationScheme));
    }

    if (appSettings?.Authentication.EnableApiKeyAuthentication == true)
    {
        options.AddPolicy(ApiConstants.Policies.ApiKeyPolicy, policy =>
            policy.RequireAuthenticatedUser()
                  .AddAuthenticationSchemes(ApiConstants.AuthenticationSchemes.ApiKey));
    }

    // Combined policy: Allow either Windows Auth OR API Key
    if (appSettings?.Authentication.EnableWindowsAuthentication == true ||
        appSettings?.Authentication.EnableApiKeyAuthentication == true)
    {
        var schemes = new List<string>();
        if (appSettings.Authentication.EnableWindowsAuthentication)
            schemes.Add(NegotiateDefaults.AuthenticationScheme);
        if (appSettings.Authentication.EnableApiKeyAuthentication)
            schemes.Add(ApiConstants.AuthenticationSchemes.ApiKey);

        options.AddPolicy(ApiConstants.Policies.CombinedPolicy, policy =>
            policy.RequireAuthenticatedUser()
                  .AddAuthenticationSchemes(schemes.ToArray()));
    }
});

// ========================================
// CONTROLLERS & API VERSIONING
// ========================================
builder.Services.AddControllers();

// ========================================
// SWAGGER / OpenAPI
// ========================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = ApiConstants.ApiTitle,
        Version = ApiConstants.ApiVersion,
        Description = ApiConstants.ApiDescription,
        Contact = new OpenApiContact
        {
            Name = "Swisscom ConfigMgr Team",
            Email = "support@swisscom.com"
        }
    });

    // Windows Authentication
    c.AddSecurityDefinition("windows", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "negotiate",
        Description = "Windows Authentication"
    });

    // API Key Authentication
    c.AddSecurityDefinition("apikey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = ApiConstants.Headers.ApiKey,
        Description = "API Key for system-to-system authentication"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "windows"
                }
            },
            Array.Empty<string>()
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "apikey"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ========================================
// HEALTH CHECKS
// ========================================
builder.Services.AddHealthChecks();

// ========================================
// CORS (Optional - for development)
// ========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ========================================
// DEPENDENCY INJECTION
// ========================================
// TODO: Register services here
// builder.Services.AddScoped<IComputerService, ComputerService>();
// builder.Services.AddScoped<IGraphUtility, GraphUtility>();
// etc.

var app = builder.Build();

// ========================================
// MIDDLEWARE PIPELINE
// ========================================

// Request logging (first)
app.UseRequestLogging();

// Exception handling
app.UseExceptionHandling();

// Swagger (development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{ApiConstants.ApiTitle} {ApiConstants.ApiVersion}");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}

// HTTPS Redirection
app.UseHttpsRedirection();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Health checks
app.MapHealthChecks("/health");

// Controllers
app.MapControllers();

Log.Information("ConfigMgr Web Service API starting up...");
Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
Log.Information("Windows Authentication: {WindowsAuth}", appSettings?.Authentication.EnableWindowsAuthentication);
Log.Information("API Key Authentication: {ApiKeyAuth}", appSettings?.Authentication.EnableApiKeyAuthentication);

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
