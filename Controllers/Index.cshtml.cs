using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GaWo.Controllers;

public class IndexModel(IAuthorizationService authorizationService) : PageModel
{
    public readonly IAuthorizationService AuthorizationService = authorizationService;
}