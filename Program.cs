using Serilog;
using Elastic.Serilog.Sinks;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Transport;
using Elastic.Channels;
using OpenTelemetry.Resources;
using OpenTelemetry;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/start-host-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // builder.Host.UseSerilog();

    builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("<aboba_project>"))
    .WithLogging()
    .UseOtlpExporter();


    // builder.Services.AddSerilog((_, lc) => lc
    // .Enrich.FromLogContext()
    // .WriteTo.Elasticsearch(new[] { new Uri("http://localhost:9200") }, opts =>
    // {
    //     opts.DataStream = new DataStreamName("logs", "telemetry-loggin", "demo");
    //     opts.BootstrapMethod = BootstrapMethod.Failure;
    //     opts.ConfigureChannel = channelOpts =>
    //     {
    //         channelOpts.BufferOptions = new BufferOptions
    //         {
    //             ExportMaxConcurrency = 10
    //         };
    //     };
    // }, transport =>
    // {
    //     transport.Authentication(new BasicAuthentication("elastic", "changeme"));
    //     transport.OnRequestCompleted(d => Console.WriteLine($"es-req: {d.DebugInformation}"));
    // })
    // .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    // .ReadFrom.Configuration(builder.Configuration));



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