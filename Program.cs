using OpenTelemetry;
using OpenTelemetry.Resources;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(".logs/start-host-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    var serviceName = builder.Configuration["OTEL_SERVICE_NAME"] ?? "komarovww";
    // builder.Host.UseSerilog();
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(r => r.AddService(serviceName))
        .WithLogging()
        .UseOtlpExporter();
    // builder.Services.AddSerilog((_, lc) => lc.Enrich.FromLogContext()
    //     .WriteTo.Elasticsearch([new Uri("http://localhost:9200")], opts =>
    //     {
    //         opts.DataStream = new DataStreamName("logs", "telemetry-loggin", "demo");
    //         opts.BootstrapMethod = BootstrapMethod.Failure;
    //         opts.ConfigureChannel = channelOpts =>
    //         {
    //             channelOpts.BufferOptions = new BufferOptions
    //             {
    //                 ExportMaxConcurrency = 10
    //             };
    //         };
    //     }, transport =>
    //     {
    //         transport.Authentication(new BasicAuthentication("elastic", "changeme")); // Basic Auth
    //         // transport.Authentication(new ApiKey(base64EncodedApiKey)); // ApiKey
    //         transport.OnRequestCompleted(d => Console.WriteLine($"es-req: {d.DebugInformation}"));
    //     })
    //     .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    //     .ReadFrom.Configuration(builder.Configuration));
    // Add services to the container.
    builder.Services.AddRazorPages();

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    // app.UseSerilogRequestLogging();

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
