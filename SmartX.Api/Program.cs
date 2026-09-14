using SmartX.Api.Models;
using SmartX.Api.Services;

// OpenAPI document generation in .NET 10 (Microsoft Docs, 2026).  
using Microsoft.AspNetCore.OpenApi;  

var builder = WebApplication.CreateBuilder(args);

//  OPENAPI / SWAGGER SERVICE REGISTRATION   
// Registers the OpenAPI document generator (Microsoft Docs, 2026).  
builder.Services.AddOpenApi();  

// CORS so the MAUI frontend can call the API (Microsoft Docs, 2026).
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMaui", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Singleton services keep in-memory state (Microsoft Docs, 2026).
builder.Services.AddSingleton<SensorStore>();
// TelemetryBatchStore has no parameterless constructor, so it is registered
// with an explicit factory that supplies the jagged-array dimensions
// (Microsoft Docs, 2026).
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    int numberOfBatches = config.GetValue<int?>("Telemetry:NumberOfBatches") ?? 10;
    int batchSize = config.GetValue<int?>("Telemetry:BatchSize") ?? 100;
    return new TelemetryBatchStore(numberOfBatches, batchSize);
});
builder.Services.AddSingleton<DeploymentValidator>();

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024;
});

var app = builder.Build();

//  OPENAPI + SWAGGER UI MIDDLEWARE   
// Only expose Swagger in Development.  
// OWASP, 2021 recommends disabling API documentation in production.  
if (app.Environment.IsDevelopment())  
{  
    // Serves the OpenAPI JSON at /openapi/v1.json (Microsoft Docs, 2026).  
    app.MapOpenApi();  

    // Serves Swagger UI at /swagger (Microsoft Docs, 2026).  
    app.UseSwaggerUI(options =>  
    {  
        options.SwaggerEndpoint("/openapi/v1.json", "Smart-X API v1");  
        options.RoutePrefix = "swagger";  
    });  
}  

app.UseCors("AllowMaui");

 
// SENSOR REGISTRATION (IIE, 2026)
 

app.MapPost("/api/sensors/register", (SensorRegistration reg, SensorStore store) =>
{
    if (string.IsNullOrWhiteSpace(reg.MacAddress))
        return Results.BadRequest(new { error = "MAC address is required." });

    if (string.IsNullOrWhiteSpace(reg.Location))
        return Results.BadRequest(new { error = "Deployment location is required." });

    if (string.IsNullOrWhiteSpace(reg.Category))
        return Results.BadRequest(new { error = "Sensor category is required." });

    store.Register(reg);
    return Results.Ok(new { message = $"Sensor {reg.MacAddress} registered.", sensor = reg });
})
.WithName("RegisterSensor")           
.WithTags("Sensors")                  
.WithSummary("Register a new sensor")  
.WithDescription("Registers an ESP32-class sensor with MAC, location, and category.");  

app.MapGet("/api/sensors", (SensorStore store) => Results.Ok(store.GetAll()))
    .WithName("ListSensors")          
    .WithTags("Sensors")              
    .WithSummary("List all sensors");  

 
// TELEMETRY INGESTION (IIE, 2026)
 

app.MapPost("/api/telemetry/moisture", (TelemetryPacket<float> packet, SensorStore store) =>
{
    store.RecordTelemetry(packet);
    return Results.Ok(new { received = packet.Value, timestamp = packet.Timestamp });
})
.WithName("MoistureTelemetry")        
.WithTags("Telemetry")                
.WithSummary("Receive float telemetry (soil moisture)");  

app.MapPost("/api/telemetry/power", (TelemetryPacket<int> packet, SensorStore store) =>
{
    store.RecordTelemetry(packet);
    return Results.Ok(new { received = packet.Value, timestamp = packet.Timestamp });
})
.WithName("PowerTelemetry")           
.WithTags("Telemetry")                
.WithSummary("Receive int telemetry (wattage)");  

app.MapPost("/api/telemetry/valve", (TelemetryPacket<bool> packet, SensorStore store) =>
{
    store.RecordTelemetry(packet);
    return Results.Ok(new { received = packet.Value, timestamp = packet.Timestamp });
})
.WithName("ValveTelemetry")           
.WithTags("Telemetry")                
.WithSummary("Receive bool telemetry (valve state)");  

// HISTORICAL BATCH STORAGE - JAGGED ARRAYS (IIE, 2026)


// Stores one sequential batch of raw telemetry into the jagged array store,
// then exposes it transferred into List<T> collections (IIE, 2026).
app.MapPost("/api/telemetry/batch", (TelemetryBatch batch, TelemetryBatchStore store) =>
{
    if (batch.MoistureValues is { Length: > 0 })
        store.StoreMoistureBatch(batch.BatchIndex, batch.MoistureValues);

    if (batch.WattageValues is { Length: > 0 })
        store.StoreWattageBatch(batch.BatchIndex, batch.WattageValues);

    return Results.Ok(new
    {
        message = $"Batch {batch.BatchIndex} stored in jagged array.",
        moistureCount = batch.MoistureValues?.Length ?? 0,
        wattageCount = batch.WattageValues?.Length ?? 0
    });
})
.WithName("StoreTelemetryBatch")
.WithTags("Telemetry")
.WithSummary("Store a raw telemetry batch in a jagged array")
.WithDescription("Writes a sequential batch of raw readings into float[][] / int[][] storage.");

// Transfers the jagged array contents into optimised List<T> collections
// (Microsoft Docs, 2026).
app.MapGet("/api/telemetry/batches/summary", (TelemetryBatchStore store) =>
{
    List<float> moisture = store.GetAllMoistureReadings();
    List<int> wattage = store.GetAllWattageReadings();

    return Results.Ok(new
    {
        moistureSlots = moisture.Count,
        wattageSlots = wattage.Count,
        moistureAverage = moisture.Count > 0 ? moisture.Average() : 0,
        wattagePeak = wattage.Count > 0 ? wattage.Max() : 0,
        firstTenMoisture = moisture.Take(10).ToList()
    });
})
.WithName("TelemetryBatchSummary")
.WithTags("Telemetry")
.WithSummary("Transfer jagged array batches into List<T> and summarise");


 
// DEPLOYMENT VALIDATION (IIE, 2026)
 

app.MapPost("/api/deployment/validate", (DeploymentNode root, DeploymentValidator validator) =>
{
    var errors = validator.Validate(root);
    return errors.Count == 0
        ? Results.Ok(new { valid = true, message = "Deployment tree is valid." })
        : Results.BadRequest(new { valid = false, errors });
})
.WithName("ValidateDeployment")       
.WithTags("Deployment")               
.WithSummary("Recursively validate a deployment tree");  

 
// FILE UPLOAD (IIE, 2026)
 

app.MapPost("/api/sensors/{mac}/attachment",
    async (string mac, IFormFile file, SensorStore store, IWebHostEnvironment env) =>
    {
        if (file.Length == 0)
            return Results.BadRequest("File is empty.");

        var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".pdf", ".log", ".txt", ".json", ".csv" };
        var ext = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(ext))
            return Results.BadRequest($"File type {ext} is not allowed.");

        var safeMac = mac.Replace(":", "_").Replace("/", "_");
        var uploadsFolder = Path.Combine(env.ContentRootPath, "Uploads", safeMac);
        Directory.CreateDirectory(uploadsFolder);

        var safeFileName = Path.GetFileName(file.FileName);
        var filePath = Path.Combine(uploadsFolder, safeFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        store.AddAttachment(mac, safeFileName);
        return Results.Ok(new { message = $"File '{safeFileName}' uploaded for sensor {mac}." });
    })
.DisableAntiforgery()
.WithName("UploadAttachment")         
.WithTags("Sensors")                  
.WithSummary("Upload a configuration file or photo");  

 
// DASHBOARD SUMMARY (IIE, 2026)
 

app.MapGet("/api/dashboard/summary", (SensorStore store) => Results.Ok(store.GetDashboardSummary()))
    .WithName("DashboardSummary")     
    .WithTags("Dashboard")            
    .WithSummary("Get dashboard summary data");  

 
// OPERATOR OVERLOADING DEMO (IIE, 2026)
 

app.MapGet("/api/demo/operator-overloading", () =>
{
    var meter1 = new SensorReading(120.5f, "METER_01", "W");
    var meter2 = new SensorReading(85.3f, "METER_02", "W");

    var totalLoad = meter1 + meter2;
    var delta = meter1 - meter2;
    bool isHigh = meter1 > 100f;

    return Results.Ok(new
    {
        Meter1 = meter1.ToString(),
        Meter2 = meter2.ToString(),
        AggregateLoad = totalLoad.ToString(),
        Delta = delta.Value,
        IsMeter1High = isHigh
    });
})
.WithName("OperatorOverloadingDemo")  
.WithTags("Demos")                    
.WithSummary("Demonstrate operator overloading");  

app.Run();

// Request payload for a raw historical telemetry batch (IIE, 2026).
public record TelemetryBatch(int BatchIndex, float[]? MoistureValues, int[]? WattageValues);

/* Reference List
IIE, 2026. PROG7312 POE. The Independent Institute of Education (Pty) Ltd.

Microsoft Docs, 2026. ASP.NET Core web API documentation with OpenAPI and Swagger. [online] Available at: <https://learn.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger> [Accessed 13 September 2026].

Microsoft Docs, 2026. Microsoft.AspNetCore.OpenApi package. [online] Available at: <https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi> [Accessed 13 September 2026].

OWASP, 2021. Improper inventory management. [online] Available at: <https://owasp.org/API-Security/editions/2023/en/0xa9-improper-inventory-management/> [Accessed 13 September 2026].
*/