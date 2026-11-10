/*using Serilog;
using Elastic.Serilog.Sinks;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Channels;
using Elastic.Transport;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(".logs/start-host-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
*/
using OpenTelemetry.Resources;
using OpenTelemetry.Logs;
using OpenTelemetry;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
.ConfigureResource(r => r.AddService("TelemetryLogging"))
.WithLogging()
.UseOtlpExporter();

    /*builder.Host.UseSerilog();

    builder.Services.AddSerilog((_, lc) => lc.Enrich.FromLogContext()
    .WriteTo.Elasticsearch([new Uri("http://localhost:9200")], opts =>
    {
        opts.DataStream = new DataStreamName("logs", "telemetry-loggin", "demo");
        opts.BootstrapMethod = BootstrapMethod.Failure;
        opts.ConfigureChannel = channelOpts =>
        {
            channelOpts.BufferOptions = new BufferOptions
            {
                ExportMaxConcurrency = 10
            };
        };
    }, transport =>
    {
        transport.Authentication(new BasicAuthentication("elastic", "changeme")); // Basic Auth
        // transport.Authentication(new ApiKey(base64EncodedApiKey)); // ApiKey
        transport.OnRequestCompleted(d => Console.WriteLine($"es-req: {d.DebugInformation}"));
    })
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .ReadFrom.Configuration(builder.Configuration));
    app.UseSerilogRequestLogging();
    */


builder.Services.AddRazorPages();

var app = builder.Build();

    // Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

/*
catch (Exception ex)
{
    Log.Fatal(ex, "Хост аварийно завершил работу при старте");
}
finally
{
    Log.CloseAndFlush();
}*/