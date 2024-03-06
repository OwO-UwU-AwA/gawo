using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gawo.Pages;

public class SitemapModel : PageModel
{
    private readonly ILogger<SitemapModel> _logger;

    public SitemapModel(ILogger<SitemapModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {        
    }
}
