   using Elastic.Channels;
   using Elastic.Ingest.Elasticsearch;
   using Elastic.Ingest.Elasticsearch.DataStreams;
   using Elastic.Serilog.Sinks;
   using Elastic.Transport;
   using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

try
{
   // Log.Information("Starting the application");

    var builder = WebApplication.CreateBuilder(args);

    //builder.Services.AddSerilog(Log.Logger);
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(r => r.AddService("VeraMarty"))
        .WithLogging(l => {l.AddConsoleExporter(); l.AddOtlpExporter();})
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