using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gawo.Pages;

public class AdminModel : PageModel
{
    public bool IsLucky { get; set; }
    private readonly ILogger<AdminModel> _logger;
    private readonly IAuthorizationService authorization;

    public AdminModel(ILogger<AdminModel> logger, IAuthorizationService authorization)
    {
        _logger = logger;
        this.authorization = authorization;
    }

    public async void OnGet()
    {        
        var res = await authorization.AuthorizeAsync(User, "AdminOnly");
        IsLucky = res.Succeeded;
    }
}
