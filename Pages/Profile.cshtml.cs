using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gawo.Pages;

[Authorize]
public class ProfileModel : PageModel
{
    private readonly ILogger<ProfileModel> _logger;

    public ProfileModel(ILogger<ProfileModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {        
    }
}