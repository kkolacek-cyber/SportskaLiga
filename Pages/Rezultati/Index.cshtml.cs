using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SportskaLiga.Domain.Enums;
using SportskaLiga.Domain.Interfaces;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Pages.Rezultati;

/// <summary>Rezultati odigranih utakmica sezone, najnovije prve; filtri kolo i momčad, straničenje.</summary>
public class IndexModel : SezonaPageModel
{
    private const int VelicinaStranice = 16;

    private readonly IUtakmicaRepository _utakmice;

    public IndexModel(ISezonaRepository sezone, IUtakmicaRepository utakmice) : base(sezone)
    {
        _utakmice = utakmice;
    }

    [BindProperty(SupportsGet = true)]
    public int? Kolo { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Momcad { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Stranica { get; set; } = 1;

    public PagedResult<Utakmica> Rezultat { get; private set; } = new();

    public IEnumerable<SelectListItem> Kola { get; private set; } = [];

    public IEnumerable<SelectListItem> Momcadi { get; private set; } = [];

    public Dictionary<string, string?> Parametri { get; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (!await UcitajSezonuAsync(ct))
        {
            return NotFound();
        }

        if (Sezona is null)
        {
            return Page();
        }

        Kola = (await _utakmice.GetKolaAsync(Sezona.Id, ct))
            .Select(k => new SelectListItem($"{k}. kolo", k.ToString(), k == Kolo));
        Momcadi = (await SezoneRepo.GetMomcadiAsync(Sezona.Id, ct))
            .Select(m => new SelectListItem(m.Naziv, m.Id.ToString(), m.Id == Momcad));

        Rezultat = await _utakmice.SearchAsync(new UtakmicaFilter
        {
            SezonaId = Sezona.Id,
            Kolo = Kolo,
            MomcadId = Momcad,
            Status = StatusUtakmice.Odigrana,
            NajnovijePrvo = true,
            Stranica = Stranica,
            VelicinaStranice = VelicinaStranice
        }, ct);

        Parametri["sezona"] = Sezona.Id.ToString();
        if (Kolo is not null) Parametri["kolo"] = Kolo.ToString();
        if (Momcad is not null) Parametri["momcad"] = Momcad.ToString();

        return Page();
    }
}
