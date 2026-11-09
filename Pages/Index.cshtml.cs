using SportskaLiga.Domain.Enums;
using SportskaLiga.Domain.Interfaces;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Pages;

/// <summary>Početna: aktivna sezona ukratko – tablica, zadnji rezultati, sljedeće utakmice i najbolji strijelci.</summary>
public class IndexModel : SezonaPageModel
{
    private readonly IUtakmicaRepository _utakmice;

    public IndexModel(ISezonaRepository sezone, IUtakmicaRepository utakmice) : base(sezone)
    {
        _utakmice = utakmice;
    }

    public IReadOnlyList<TablicaRed> Tablica { get; private set; } = [];

    public IReadOnlyList<Utakmica> ZadnjiRezultati { get; private set; } = [];

    public IReadOnlyList<Utakmica> Sljedece { get; private set; } = [];

    public IReadOnlyList<Strijelac> Strijelci { get; private set; } = [];

    public bool ImaOtvorenihPrijava { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        // Početna uvijek prikazuje aktivnu sezonu (parametar ?sezona se ovdje ne koristi).
        SezonaId = null;
        await UcitajSezonuAsync(ct);
        ImaOtvorenihPrijava = (await SezoneRepo.GetOtvoreneZaPrijaveAsync(ct)).Count > 0;

        if (Sezona is null)
        {
            return;
        }

        Tablica = await _utakmice.GetTablicaAsync(Sezona.Id, ct);
        Strijelci = await _utakmice.GetStrijelciAsync(Sezona.Id, 5, ct);

        ZadnjiRezultati = (await _utakmice.SearchAsync(new UtakmicaFilter
        {
            SezonaId = Sezona.Id,
            Status = StatusUtakmice.Odigrana,
            NajnovijePrvo = true,
            VelicinaStranice = 6
        }, ct)).Items;

        // Zakazane kronološki; one kojima je termin prošao (čekaju rezultat) nisu "sljedeće".
        var zakazane = await _utakmice.SearchAsync(new UtakmicaFilter
        {
            SezonaId = Sezona.Id,
            Status = StatusUtakmice.Zakazana,
            VelicinaStranice = 50
        }, ct);
        Sljedece = zakazane.Items.Where(u => u.Pocetak >= DateTime.UtcNow).Take(6).ToList();
    }
}
