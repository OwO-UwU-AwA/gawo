using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GaWo.Controllers;

public class PointsModel(IAuthorizationService authorizationService) : PageModel
{
    public readonly IAuthorizationService AuthorizationService = authorizationService;

}