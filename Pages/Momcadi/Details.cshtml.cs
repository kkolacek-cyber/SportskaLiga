using Microsoft.AspNetCore.Mvc;
using SportskaLiga.Domain.Interfaces;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Pages.Momcadi;

/// <summary>Stranica momčadi: igrači, pozicija u tablici, utakmice i strijelci u odabranoj (ili aktivnoj) sezoni.</summary>
public class DetailsModel : SezonaPageModel
{
    private readonly IMomcadRepository _momcadi;
    private readonly IUtakmicaRepository _utakmice;

    public DetailsModel(ISezonaRepository sezone, IMomcadRepository momcadi, IUtakmicaRepository utakmice) : base(sezone)
    {
        _momcadi = momcadi;
        _utakmice = utakmice;
    }

    public Momcad Momcad { get; private set; } = default!;

    /// <summary>Red momčadi u tablici odabrane sezone; null ako momčad u njoj ne igra.</summary>
    public TablicaRed? RedUTablici { get; private set; }

    public IReadOnlyList<Utakmica> Utakmice { get; private set; } = [];

    public IReadOnlyList<Strijelac> Strijelci { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var momcad = await _momcadi.GetDetailsAsync(id, ct);
        if (momcad is null || !await UcitajSezonuAsync(ct))
        {
            return NotFound();
        }

        Momcad = momcad;

        if (Sezona is not null)
        {
            RedUTablici = (await _utakmice.GetTablicaAsync(Sezona.Id, ct)).FirstOrDefault(r => r.MomcadId == id);

            Utakmice = (await _utakmice.SearchAsync(new UtakmicaFilter
            {
                SezonaId = Sezona.Id,
                MomcadId = id,
                VelicinaStranice = 100
            }, ct)).Items;

            Strijelci = (await _utakmice.GetStrijelciAsync(Sezona.Id, null, ct))
                .Where(s => s.MomcadId == id)
                .ToList();
        }

        return Page();
    }
}
