using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SportskaLiga.Domain.Interfaces;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Pages.Prijava;

/// <summary>Potvrda i praćenje prijave po kodu; otvara se odmah nakon slanja i sa stranice "Status prijave".</summary>
public class PotvrdaModel : PageModel
{
    private readonly IPrijavaRepository _prijave;

    public PotvrdaModel(IPrijavaRepository prijave)
    {
        _prijave = prijave;
    }

    public PrijavaMomcadi Prijava { get; private set; } = default!;

    /// <summary>Stranica je otvorena odmah nakon slanja (prikazuje se poruka o zaprimanju).</summary>
    public bool JeNova { get; private set; }

    public async Task<IActionResult> OnGetAsync(string kod, CancellationToken ct)
    {
        var prijava = await _prijave.GetByKodAsync(kod, ct);
        if (prijava is null)
        {
            return NotFound();
        }

        Prijava = prijava;
        // TempData nosi kod samo jednom (odmah nakon slanja), pa se poruka o zaprimanju ne prikazuje pri svakom otvaranju.
        JeNova = TempData["NovaPrijava"] as string == prijava.Kod;

        return Page();
    }
}
