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
        const string myName = "Вот и всё… Неплохая получилась история. Интересная, весёлая, порой немного грустная, а главное – поучительная…Она научила нас быть смелыми и не бояться вызовов, которые готовит нам жизнь. Помогала нам добиваться поставленных целей несмотря ни на что. И самое важное – она научила нас по-настоящему любить и не сходить с пути, следуя за своей мечтой";
        _logger.LogInformation("Sample log. My name is {MyName}", myName);
    }
}
