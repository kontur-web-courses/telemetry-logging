using System.Diagnostics;
using Elastic.Channels;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using Serilog;


var builder = WebApplication.CreateBuilder(args);
Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));
Log.Logger = new LoggerConfiguration()
    // .WriteTo.Console()
    // .WriteTo.File(".logs/start-host-log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Elasticsearch(new List<Uri>{new Uri("http://localhost:9200")}, opts =>
    {
        opts.DataStream = new DataStreamName("logs", "telemetry-logging", "demo");
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
        transport.OnRequestCompleted(d => Console.WriteLine($"es-req: {d.DebugInformation}"));
    })
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

try
{
// Add services to the container.
    builder.Services.AddRazorPages();

    builder.Host.UseSerilog();

    builder.Services.AddSerilog(Log.Logger);

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

    app.Run();
}
catch (Exception)
{
    Log.Logger.Fatal("ASDASDASDASDASDASDASD PIZDA");
}
