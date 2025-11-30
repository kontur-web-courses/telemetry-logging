using Serilog;
using Serilog.Sinks.Elasticsearch;


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(".logs/start-host-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddRazorPages();

    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
        {
            IndexFormat = "logs-{0:yyyy.MM.dd}",

            AutoRegisterTemplate = true,
            AutoRegisterTemplateVersion = Serilog.Sinks.Elasticsearch.AutoRegisterTemplateVersion.ESv8,

            ModifyConnectionSettings = conn => conn
                .BasicAuthentication("elastic", "changeme")
                .ServerCertificateValidationCallback((_, _, _, _) => true),

            BufferBaseFilename = "logs/buffer",
            BufferLogShippingInterval = TimeSpan.FromSeconds(5),
            EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog | EmitEventFailureHandling.WriteToFailureSink,
            FailureCallback = (logEvent, exception) =>
            {
                Console.WriteLine($"Failed to log to Elasticsearch: {logEvent.MessageTemplate.Text}, Exception: {exception.Message}");
            }
        })
        .CreateLogger();

    builder.Host.UseSerilog(Log.Logger);
        

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
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}