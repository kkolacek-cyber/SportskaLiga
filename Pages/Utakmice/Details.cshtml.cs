using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SportskaLiga.Domain.Interfaces;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Pages.Utakmice;

/// <summary>Detalji utakmice: rezultat, strijelci po minutama, igralište i sudac.</summary>
public class DetailsModel : PageModel
{
    private readonly IUtakmicaRepository _utakmice;

    public DetailsModel(IUtakmicaRepository utakmice)
    {
        _utakmice = utakmice;
    }

    public Utakmica Utakmica { get; private set; } = default!;

    // id dolazi iz rute: @page "{id:int}" → /Utakmice/Details/15
    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var utakmica = await _utakmice.GetDetailsAsync(id, ct);
        if (utakmica is null)
        {
            return NotFound();
        }

        Utakmica = utakmica;
        return Page();
    }
}
