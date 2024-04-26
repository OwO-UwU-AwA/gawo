using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GaWo.Controllers;

public class AdminModel(IAuthorizationService authorization) : PageModel
{
    public bool IsLucky { get; set; }

    public async void OnGet()
    {
        var res = await authorization.AuthorizeAsync(User, "AdminOnly");
        IsLucky = res.Succeeded;
    }
}