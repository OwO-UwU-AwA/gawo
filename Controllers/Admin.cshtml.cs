using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GaWo.Controllers;

public class AdminModel : PageModel
{
    public bool IsLucky { get; set; }
    private readonly IAuthorizationService _authorization;

    public AdminModel(IAuthorizationService authorization)
    {
        _authorization = authorization;
    }

    public async void OnGet()
    {
        var res = await _authorization.AuthorizeAsync(User, "AdminOnly");
        IsLucky = res.Succeeded;
    }
}