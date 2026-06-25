using Asp.Versioning;

using FluentValidation;

using Rps.Api.Http.Domain;
using Rps.Api.Http.Shared.Extensions;
using Rps.Api.Http.Shared.Utilities;

using Scalar.AspNetCore;

using Serilog;
using Serilog.Events;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentUserName()
        .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);
});

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        // Add error handling for problematic types
        options.JsonSerializerOptions.IgnoreReadOnlyProperties = false;
        options.JsonSerializerOptions.IncludeFields = false;
    });

// API Versioning Setup
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddMvc();

builder.Services.AddOpenApi("v1", options =>  // Note: "v1" to bypass interceptor
{
    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        // Remove parameters with null schema
        foreach (var param in operation.Parameters?.ToList() ?? [])
        {
            if (param.Schema == null)
                operation.Parameters?.Remove(param);
        }

        // Remove parameter descriptions with null metadata
        for (int i = context.Description.ParameterDescriptions.Count - 1; i >= 0; i--)
        {
            if (context.Description.ParameterDescriptions[i].ModelMetadata == null)
                context.Description.ParameterDescriptions.RemoveAt(i);
        }

        return Task.CompletedTask;
    });

    options.AddDocumentTransformer((document, context, _) =>
    {
        document.Info = new()
        {
            Title = "Rock|Paper|Scissors",
            Version = "v1.0",
            Description = """

                    Rock|Paper|Scissors API
                    
                    This API allow to play rock paper scissors against the itself. 
                    It provides endpoints to play a single game or multiple games at once. 
                    The API is versioned and supports different versions of the game logic.
                    
                    """,
            Contact = new()
            {
                Name = "Rock Paper Scissors Support",
                Email = "support@rps.org",
            }
        };
        return Task.CompletedTask;
    });

    options.AddSchemaTransformer<SafeSchemaTransformer>();
});

// Register application services
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi()
        .CacheOutput()
        .AllowAnonymous();

    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Rock Paper Scissors API")
               .WithTheme(ScalarTheme.Kepler);
    });

    app.MapGet("/", () => Results.Redirect("/scalar/v1"))
       .ExcludeFromDescription()
       .AllowAnonymous();
}

app.MapControllers();

await app.RunAsync().ConfigureAwait(false);

public partial class Program { }
