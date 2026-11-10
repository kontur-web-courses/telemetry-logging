using Elastic.Channels;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(".logs/start-host-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddRazorPages();

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
        .ReadFrom.Configuration(builder.Configuration));*/


    builder.Services.AddOpenTelemetry()
        .ConfigureResource(r => r.AddService("ABOBA_3_SPIDOMETER_2.0"))
        .WithLogging()
        .UseOtlpExporter();


    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    //app.UseSerilogRequestLogging();

    app.UseRouting();

    app.UseAuthorization();

    app.MapRazorPages();

    app.Run();
}
catch (Exception ex)
{
    Log.Logger.Error(ex.Message, ex);
}