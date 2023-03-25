using System.Data.SQLite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gawo.Pages;

[Authorize]
public class AddEventModel : PageModel
{
    private readonly ILogger<AddEventModel> _logger;

    public AddEventModel(ILogger<AddEventModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {        

    }

    public IActionResult AddEvent()
    {
        Console.WriteLine("B");
        Console.WriteLine($"{Request.Form["type"]}");
        return Page();
    }
}