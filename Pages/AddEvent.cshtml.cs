using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gawo.Pages;

[Authorize]
public class AddEventModel : PageModel
{
    private readonly ILogger<AddEventModel> _logger;

    [BindProperty]
    public string Name { get; set; } = string.Empty;
    [BindProperty]
    public string Description { get; set; } = string.Empty;
    [BindProperty]
    public string Date { get; set; } = string.Empty;

    public enum Rooms {
        NULL,
        SPORT,
        UG13,
        UG14,
        UG15,
        UG16,
        UG17,
        UG18,
        UG19,
        UG20,
        UG21,
        UG22,
        UG23,
        UG24,
        UG25,
        UG26,
        UG27,
        UG28,
        UG29,
        UG30,
        UG31,
        UG32,
        UG33,
        UG34,
        UG35,
    };
    [BindProperty]
    public Rooms Room { get; set; } = Rooms.NULL;
    [BindProperty]
    public byte[]? Picture { get; set; }
    [BindProperty]
    public int Capacity { get; set; } = -1;
    [BindProperty]
    public int Duration { get; set; } = -1;
    [BindProperty]
    public string Grade7 { get; set; } = string.Empty;
    [BindProperty]
    public string Grade8 { get; set; } = string.Empty;
    [BindProperty]
    public string Grade9 { get; set; } = string.Empty;
    [BindProperty]
    public string Grade10 { get; set; } = string.Empty;
    [BindProperty]
    public string Grade11 { get; set; } = string.Empty;
    [BindProperty]
    public string Grade12 { get; set; } = string.Empty;
    [BindProperty]
    public string Notes { get; set; } = string.Empty;
    [BindProperty]
    public string? Organiser { get; set; }
    [BindProperty]
    public string CoOrganisers { get; set; } = string.Empty;

    public AddEventModel(ILogger<AddEventModel> logger)
    
    {
        _logger = logger;
    }

    public void OnGet()
    {        

    }

    public IActionResult OnPostAddEvent()
    {
        Console.WriteLine($"{Name} : {Description} : {Organiser} : {CoOrganisers} : {Date} : {Room} : {Picture} : {Capacity} : {Duration} : {Grade10} : {Notes}");
        return Page();
    }
}