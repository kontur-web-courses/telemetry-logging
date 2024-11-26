using Elastic.Channels;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using Serilog;
using Serilog.Events;


string logFilePath = Path.Combine(".logs", $"start-host-log-{DateTime.Now:yyyy-MM-dd}.txt");
Log.Logger = new LoggerConfiguration()
    .WriteTo.File(logFilePath, LogEventLevel.Error)
    .WriteTo.Console()
    .CreateLogger();

Log.Information($"Starting application. Logging to: {logFilePath}");

try {  
    var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
    builder.Services.AddRazorPages();
    builder.Host.UseSerilog();

    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .Enrich.FromLogContext()
        .WriteTo.Elasticsearch(new []{ new Uri("http://localhost:9200")}, opts =>
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

    app.UseSerilogRequestLogging();

    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthorization();

    app.MapRazorPages();

    app.Run();
}
catch(Exception ex)
{
    Log.Logger.Error(ex.Message);
}
finally
{
    Log.CloseAndFlush();
}

