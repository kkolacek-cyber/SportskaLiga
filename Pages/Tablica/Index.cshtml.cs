using Microsoft.AspNetCore.Mvc;
using SportskaLiga.Domain.Interfaces;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Pages.Tablica;

/// <summary>Tablica lige odabrane sezone (računa se iz odigranih utakmica pri svakom otvaranju).</summary>
public class IndexModel : SezonaPageModel
{
    private readonly IUtakmicaRepository _utakmice;

    public IndexModel(ISezonaRepository sezone, IUtakmicaRepository utakmice) : base(sezone)
    {
        _utakmice = utakmice;
    }

    public IReadOnlyList<TablicaRed> Redovi { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (!await UcitajSezonuAsync(ct))
        {
            return NotFound();
        }

        if (Sezona is not null)
        {
            Redovi = await _utakmice.GetTablicaAsync(Sezona.Id, ct);
        }

        return Page();
    }
}
