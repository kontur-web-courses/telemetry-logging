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
        var myName = "Idris Khalikov and Vladimir Obraztsov";
        _logger.LogInformation("Sample log. My name is {MyName}", myName);
    }
}
