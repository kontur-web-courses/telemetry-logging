using Elastic.Channels;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(".logs/start-host-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting web application...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddRazorPages();

    builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
        loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));
    
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
    
    
    
    // builder.Services.AddSerilog(Log.Logger);

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