using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GaWo.Controllers;

[Authorize]
public class MyEventsModel(IAuthorizationService authorizationService) : PageModel
{
    public readonly IAuthorizationService AuthorizationService = authorizationService;
}