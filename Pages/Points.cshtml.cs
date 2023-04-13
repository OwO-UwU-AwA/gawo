using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gawo.Pages;

public class PointsModel : PageModel
{
    private readonly ILogger<PointsModel> _logger;

    public PointsModel(ILogger<PointsModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
      
    }
}
