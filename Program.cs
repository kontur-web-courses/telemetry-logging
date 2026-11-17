using Serilog;
using OpenTelemetry;
using OpenTelemetry.Resources;

// Log.Logger = new LoggerConfiguration()
//     .WriteTo.Console()
//     .WriteTo.File(
//         ".logs/start-host-log-.txt",
//         rollingInterval: RollingInterval.Day)
//     .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // OpenTelemetry
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(r => r.AddService("telemetry-logging"))
        .WithLogging()
        .UseOtlpExporter();

    // Serilog
    // Log.Logger = new LoggerConfiguration()
    //     .Enrich.FromLogContext()
    //     .WriteTo.Elasticsearch([new Uri("http://localhost:9200")], opts =>
    //     {
    //         opts.DataStream = new DataStreamName("logs", "telemetry-logging", "demo");
    //         opts.BootstrapMethod = BootstrapMethod.Failure;
    //
    //         opts.ConfigureChannel = channelOpts =>
    //         {
    //             channelOpts.BufferOptions = new BufferOptions
    //             {
    //                 ExportMaxConcurrency = 10
    //             };
    //         };
    //     }, transport =>
    //     {
    //         transport.Authentication(
    //             new BasicAuthentication("elastic", "changeme"));
    //
    //         transport.OnRequestCompleted(d =>
    //             Console.WriteLine($"es-req: {d.DebugInformation}"));
    //     })
    //     .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    //     .ReadFrom.Configuration(builder.Configuration)
    //     .CreateLogger();

    // builder.Services.AddSerilog(Log.Logger);

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
    Console.Error.WriteLine(ex);
}