using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gawo.Pages;

public class LoginModel : PageModel
{
    private readonly ILogger<LoginModel> _logger;

    [BindProperty]
    public string? Username { get; set; }

    [BindProperty]
    public string? Password { get; set; }

    public LoginModel(ILogger<LoginModel> logger)
    {
        _logger = logger;
    }

    public void OnPost()
    {
        Console.WriteLine($"{Username}");
        Console.WriteLine($"{Password}");
        if (Username == null || Password == null || Username.Length <= 3 || Password.Length <= 8)
            return;
    }

    public void OnGet()
    {        

    }
}