using Elastic.Channels;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using Serilog;
using Serilog.Formatting.Json;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/start-host-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting up");
    
    var builder = WebApplication.CreateBuilder(args);

    var logDir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
    Directory.CreateDirectory(logDir);

    var logPath = Path.Combine(logDir, "log-.json");
    
// Add services to the container.
    builder.Host.UseSerilog((context, config) =>
    {
        config
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Elasticsearch(
                new[] { new Uri("http://localhost:9200") },
                opts =>
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
                },
                transport =>
                {
                    transport.Authentication(new BasicAuthentication("elastic", "changeme")); // Basic Auth
                    // transport.Authentication(new ApiKey(base64EncodedApiKey)); // ApiKey
                    transport.OnRequestCompleted(d => Console.WriteLine($"es-req: {d.DebugInformation}"));
                })
            // ------------------------------------------------------------------------------

            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
            
            .WriteTo.Console()
            .WriteTo.File(
                path: logPath,
                rollingInterval: RollingInterval.Day,
                formatter: new JsonFormatter(),
                shared: true,
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information);
    });

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

    app.UseSerilogRequestLogging();
    app.UseRouting();

    app.UseAuthorization();

    app.MapRazorPages();

    Log.Information("Started");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.CloseAndFlush();
}
