using Elastic.Channels;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using OpenTelemetry;
using OpenTelemetry.Resources;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddRazorPages();

    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty(
            "Environment",
            builder.Environment.EnvironmentName)
        .WriteTo.Console()
        .WriteTo.Elasticsearch(
            [new Uri("http://localhost:9200")],
            opts =>
            {
                opts.DataStream = new DataStreamName(
                    "logs",
                    "telemetry-logging",
                    "demo");

                opts.BootstrapMethod = BootstrapMethod.Failure;

                opts.ConfigureChannel = channelOpts =>
                {
                    channelOpts.BufferOptions = new BufferOptions
                    {
                        ExportMaxConcurrency = 10
                    };
                };
            },
            transport =>
            {
                transport.Authentication(
                    new BasicAuthentication("elastic", "changeme"));

                transport.OnRequestCompleted(d =>
                    Console.WriteLine($"es-req: {d.DebugInformation}"));
            }));
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(r => r.AddService("abobus"))
        .WithLogging()
        .UseOtlpExporter();

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseSerilogRequestLogging();

    app.UseRouting();
    app.UseAuthorization();

    app.MapRazorPages();

    Log.Information("Application configured successfully");

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