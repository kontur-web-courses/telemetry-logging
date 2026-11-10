   using Elastic.Channels;
   using Elastic.Ingest.Elasticsearch;
   using Elastic.Ingest.Elasticsearch.DataStreams;
   using Elastic.Serilog.Sinks;
   using Elastic.Transport;
   using OpenTelemetry;
   using OpenTelemetry.Resources;
   DotNetEnv.Env.Load();
   //using Serilog;

   // Log.Logger = new LoggerConfiguration()
   //     .Enrich.FromLogContext()
   //     .WriteTo.Console()
   //     .WriteTo.File(
   //         ".logs/start-host-log-.txt",
   //         rollingInterval: RollingInterval.Day)
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
   //         transport.Authentication(new BasicAuthentication("elastic", "changeme"));
   //         transport.OnRequestCompleted(d => Console.WriteLine($"es-req: {d.DebugInformation}"));
   //     })
   //     .CreateLogger();
try
{
   // Log.Information("Starting the application");

    var builder = WebApplication.CreateBuilder(args);

    //builder.Services.AddSerilog(Log.Logger);
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(r => r.AddService("WetherApp10.11"))
        .WithLogging()
        .UseOtlpExporter();

    builder.Services.AddRazorPages();
    var endpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
    Console.WriteLine($"[DEBUG] OTEL Endpoint is: {endpoint}");
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
   // Log.Fatal(ex, "Application terminated unexpectedly");
}

finally
{
  //  Log.CloseAndFlush();
}