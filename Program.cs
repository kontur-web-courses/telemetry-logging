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


    
    builder.Services.AddSerilog((hostingContext, loggerConfiguration) =>
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

    app.UseSerilogRequestLogging();

    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthorization();

    app.MapRazorPages();

    app.Run();
}
catch(Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

