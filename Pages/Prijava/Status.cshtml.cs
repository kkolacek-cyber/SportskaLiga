using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SportskaLiga.Domain.Interfaces;
using SportskaLiga.ViewModels;

namespace SportskaLiga.Pages.Prijava;

/// <summary>Provjera statusa: prijava se pronalazi kodom, a e-mail potvrđuje da je prijava podnositeljeva.</summary>
public class StatusModel : PageModel
{
    private readonly IPrijavaRepository _prijave;

    public StatusModel(IPrijavaRepository prijave)
    {
        _prijave = prijave;
    }

    [BindProperty]
    public StatusInput Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var prijava = await _prijave.GetByKodAsync(Input.Kod, ct);

        // Isti odgovor za nepoznat kod i krivi e-mail, da se kodovi ne mogu "pogađati".
        if (prijava is null || !string.Equals(prijava.KontaktEmail, Input.Email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, "Nije pronađena prijava s tim kodom i e-mail adresom.");
            return Page();
        }

        return RedirectToPage("Potvrda", new { kod = prijava.Kod });
    }
}
