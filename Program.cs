using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
             .WriteTo.Console()
             .WriteTo.File(".logs/start-host-log-.txt",
                           LogEventLevel.Error,
                           rollingInterval: RollingInterval.Day,
                           rollOnFileSizeLimit: true)
             .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddRazorPages();

    builder.Host.UseSerilog();

    builder.Services.AddSerilog((_, loggerConfiguration) =>
                                    loggerConfiguration.ReadFrom.Configuration(builder.Configuration));

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
catch (Exception e)
{
    Log.Fatal(e, "Exception occured during startup");
}