using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace telemetry.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        var myNames = "Топорков Артём, Волков Андрей, Адылов Мурад";
        _logger.LogInformation("Sample log. My names are {MyNames}", myNames);
    }
}
