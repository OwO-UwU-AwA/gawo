using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gawo.Pages;

[Authorize]
public class MyEventsModel : PageModel
{
    private readonly ILogger<MyEventsModel> _logger;

    public MyEventsModel(ILogger<MyEventsModel> logger)
    {
        _logger = logger;
    }
}