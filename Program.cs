using Elastic.Channels;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry;
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

    // Keep the bootstrap logger above active until configuration has been read.
    // Setting Telemetry:UseSerilog to false switches the app to OpenTelemetry-only
    // logging (the final exercise), while leaving the early-startup logger intact.
    var useSerilog = builder.Configuration.GetValue("Telemetry:UseSerilog", true);
    var useElastic = builder.Configuration.GetValue("Telemetry:UseElastic", true);

    if (useSerilog)
    {
        var loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName);

        if (useElastic)
        {
            loggerConfiguration.WriteTo.Elasticsearch(
                [new Uri(builder.Configuration["Telemetry:Elasticsearch:Uri"] ?? "http://localhost:9200")],
                options =>
                {
                    options.DataStream = new DataStreamName("logs", "telemetry-loggin", "demo");
                    options.BootstrapMethod = BootstrapMethod.Failure;
                    options.ConfigureChannel = channelOptions =>
                    {
                        channelOptions.BufferOptions = new BufferOptions
                        {
                            ExportMaxConcurrency = 10
                        };
                    };
                },
                transport =>
                {
                    transport.Authentication(new BasicAuthentication(
                        builder.Configuration["Telemetry:Elasticsearch:Username"] ?? "elastic",
                        builder.Configuration["Telemetry:Elasticsearch:Password"] ?? "changeme"));
                    transport.OnRequestCompleted(d =>
                        Console.WriteLine($"es-req: {d.DebugInformation}"));
                });
        }

        // Register the already-configured logger so ASP.NET Core uses the same
        // sinks as the bootstrap logger and does not create a second one.
        Log.Logger = loggerConfiguration.CreateLogger();
        builder.Services.AddSerilog(Log.Logger);
    }

    var serviceName = builder.Configuration["Telemetry:ServiceName"] ?? "telemetry-logging";
    var useConsoleExporter = builder.Configuration.GetValue("Telemetry:UseConsoleExporter", false);
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService(serviceName))
        .WithLogging(logging =>
        {
            if (useConsoleExporter)
            {
                logging.AddConsoleExporter();
            }
        })
        .UseOtlpExporter();

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
    if (useSerilog)
    {
        app.UseSerilogRequestLogging();
    }

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