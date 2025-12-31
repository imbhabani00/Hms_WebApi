using Amazon.Runtime;
using Amazon.S3;
using Asp.Versioning.ApiExplorer;
using EasyCaching.InMemory;
using FluentValidation;
using Hms.Helpers;
using Hms.Infrastructure.AWS;
using Hms.Infrastructure.Cache;
using Hms.WebApi.Extensions;
using Hms.WebApi.Models;
using Hms.WebApi.Models.Options;
using Hms.WebApi.Validators.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ========== CONFIGURATION ==========
// Load all configuration sources
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("Configuration/aws.json", optional: true, reloadOnChange: true)
    .AddJsonFile("Configuration/caching.json", optional: true, reloadOnChange: true)
    .AddJsonFile("Configuration/serilog.json", optional: true, reloadOnChange: true)
    .AddJsonFile("Configuration/jwt.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();  // ← CRITICAL: This enables User Secrets in Development!

var connectionString = builder.Configuration.GetConnectionString("HmsConnectionString");

// ========== SERILOG CONFIGURATION ==========
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateLogger();

if (!string.IsNullOrEmpty(connectionString))
{
    try
    {
        using var testConn = new System.Data.SqlClient.SqlConnection(connectionString);
        testConn.Open();
        Log.Information("SQL Connection successful: {DbServer}", testConn.DataSource);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .WriteTo.MSSqlServer(
                connectionString: connectionString,
                schemaName: "dbo",
                tableName: "tbl_Logs",
                autoCreateSqlTable: true,
                restrictedToMinimumLevel: LogEventLevel.Information,
                formatProvider: null
            )
            .WriteTo.Console()
            .Enrich.FromLogContext()
            .CreateLogger();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "❌ SQL Connection failed. Using console only. Conn: {Conn}",
            connectionString?.Substring(0, Math.Min(50, connectionString.Length)) + "...");

        // Fallback to console
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .WriteTo.Console()
            .Enrich.FromLogContext()
            .CreateLogger();
    }
}

builder.Host.UseSerilog();

// ========== ADD SERVICES ==========
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddVersioning();
builder.Services.ConfigureSwagger();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddCustomCors("AllowWebApp");

// ========== JWT AUTHENTICATION ==========
var jwtSettings = builder.Configuration.GetSection("JWTSettings");
var secretKey = builder.Configuration["JWTSettings:JWTSecretKey"];

if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("JWT secret key not found. Check User Secrets or Configuration/jwt.json file.");
}

Log.Information("JWT Secret Key loaded successfully");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.GetValue<string>("Issuer"),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// ========== AWS S3 CONFIGURATION ==========
// Read AWS credentials from User Secrets (Development) or Environment Variables (Production)
try
{
    var awsAccessKey = builder.Configuration["AWS:Credentials:AccessKey"];
    var awsSecretKey = builder.Configuration["AWS:Credentials:SecretKey"];
    var awsRegion = builder.Configuration["AWS:Region"];

    if (!string.IsNullOrEmpty(awsAccessKey) && !string.IsNullOrEmpty(awsSecretKey))
    {
        // Create AWS credentials explicitly
        var awsCredentials = new BasicAWSCredentials(awsAccessKey, awsSecretKey);

        // Configure AWS options with credentials
        var awsOptions = new Amazon.Extensions.NETCore.Setup.AWSOptions
        {
            Credentials = awsCredentials,
            Region = Amazon.RegionEndpoint.GetBySystemName(awsRegion ?? "ap-southeast-1")
        };

        builder.Services.AddDefaultAWSOptions(awsOptions);
        builder.Services.AddAWSService<IAmazonS3>();
        builder.Services.AddSingleton<IAWSS3Service, AWSS3Service>();

        // Configure S3 bucket options
        builder.Services.Configure<S3Options>(builder.Configuration.GetSection("AWS:S3:Private"));
        builder.Services.Configure<S3PublicOptions>(builder.Configuration.GetSection("AWS:S3:Public"));

        Log.Information("AWS S3 services configured successfully for region: {Region}", awsRegion);
    }
    else
    {
        Log.Warning("AWS credentials not found in User Secrets. S3 services will not be available.");
        Log.Information("To add AWS credentials, run:");
        Log.Information("  dotnet user-secrets set \"AWS:Credentials:AccessKey\" \"your-key\"");
        Log.Information("  dotnet user-secrets set \"AWS:Credentials:SecretKey\" \"your-secret\"");
    }
}
catch (Exception ex)
{
    Log.Error(ex, "Failed to configure AWS services. Application will continue without S3 support.");
}

// ========== CACHING ==========
builder.Services.AddEasyCaching(options =>
{
    options.UseInMemory(config =>
    {
        config.DBConfig = new InMemoryCachingOptions
        {
            SizeLimit = 100,
            ExpirationScanFrequency = 60
        };
    }, "in-memory-cache");
});

// ========== APPLICATION SERVICES ==========
builder.Services.AddSingleton<AccessCodeHelper>();
builder.Services.AddScoped<ICachedConfigurationService, CachedConfigurationService>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddHMSApplicationServices();
builder.Services.AddSingleton<IValidator<AuthModel>, UserAuthValidator>();

// ========== BUILD APPLICATION ==========
var app = builder.Build();

// ========== CONFIGURE MIDDLEWARE ==========
if (app.Environment.IsDevelopment())
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"HMS Web API {description.GroupName.ToUpperInvariant()}");
        }
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowWebApp");
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandling(app.Services.GetRequiredService<ILoggerFactory>(), app.Environment);
app.MapControllers();

// ========== START APPLICATION ==========
try
{
    Log.Information("Starting HMS Web API in {Environment} environment", app.Environment.EnvironmentName);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}