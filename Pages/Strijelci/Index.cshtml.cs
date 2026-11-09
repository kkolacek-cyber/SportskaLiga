using Microsoft.AspNetCore.Mvc;
using SportskaLiga.Domain.Interfaces;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Pages.Strijelci;

/// <summary>Lista strijelaca odabrane sezone (autogolovi se ne broje).</summary>
public class IndexModel : SezonaPageModel
{
    private readonly IUtakmicaRepository _utakmice;

    public IndexModel(ISezonaRepository sezone, IUtakmicaRepository utakmice) : base(sezone)
    {
        _utakmice = utakmice;
    }

    public IReadOnlyList<Strijelac> Lista { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (!await UcitajSezonuAsync(ct))
        {
            return NotFound();
        }

        if (Sezona is not null)
        {
            Lista = await _utakmice.GetStrijelciAsync(Sezona.Id, null, ct);
        }

        return Page();
    }
}
