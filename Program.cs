using Elastic.Channels;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Elasticsearch(new []{ new Uri("http://localhost:9200") }, opts =>
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
        transport.Authentication(new BasicAuthentication("elastic", "changeme"));
    })
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

try
{
    builder.Services.AddRazorPages();

    builder.Host.UseSerilog();

    builder.Services.AddSerilog((hostingContext, loggerConfiguration) =>
        loggerConfiguration.ReadFrom.Configuration(hostingContext.GetService<HostBuilderContext>().Configuration));

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
catch (Exception e)
{
    Log.Logger.Fatal($"Fatal: {e.Message}");
}

