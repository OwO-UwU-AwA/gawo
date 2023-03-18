using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gawo.Pages;

public class ProfileModel : PageModel
{
    private readonly ILogger<ProfileModel> _logger;

    public ProfileModel(ILogger<ProfileModel> logger)
    {
        _logger = logger;
    }

    public IActionResult OnGet()
    {        
        if (HttpContext.User.Identity != null && !HttpContext.User.Identity.IsAuthenticated)
        {
            return RedirectToPage("/Login");
        }
        else
            return RedirectToPage("Error");
    }
}