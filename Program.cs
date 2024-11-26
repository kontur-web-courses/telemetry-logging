using Serilog;



try
{
    Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.


    builder.Host.UseSerilog();
    builder.Services.AddSerilog(loggerConfiguration =>
                loggerConfiguration.ReadFrom.Configuration(builder.Configuration));

    builder.Services.AddRazorPages();


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
catch(Exception e)
{
    Log.Error(e.Message);
    throw;
}
