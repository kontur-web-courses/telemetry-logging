using Serilog;
using Elastic.Ingest.Elasticsearch;
using Elastic.Transport;
using Elastic.Serilog.Sinks;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Channels;

try
{
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File(".logs/start-host-log-.txt", rollingInterval: RollingInterval.Day)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithMachineName()      
        .CreateLogger();     

    var builder = WebApplication.CreateBuilder(args);
    
    builder.Services.AddSerilog(Log.Logger);

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
            .WriteTo.Console()
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
                transport.Authentication(new BasicAuthentication("elastic", "changeme"));
                // transport.Authentication(new ApiKey(base64EncodedApiKey));
                transport.OnRequestCompleted(d => Console.WriteLine($"es-req: {d.DebugInformation}"));
            });
    });

    builder.Services.AddRazorPages();

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
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}