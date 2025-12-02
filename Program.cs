using Serilog;



try
{
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File(".logs/start-host-log-.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();
    // Add services to the container.


    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddRazorPages();
    builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
        loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));
    var app = builder.Build();

    app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthorization();

    app.MapRazorPages();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Super crush");
}