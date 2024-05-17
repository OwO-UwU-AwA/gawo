using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GaWo.Controllers;

[Authorize(Policy = "AdminOnly")]
public class AdminModel : PageModel
{
}