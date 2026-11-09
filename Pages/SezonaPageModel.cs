using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SportskaLiga.Domain.Interfaces;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Pages;

/// <summary>
/// Zajednička osnova stranica koje prikazuju podatke jedne sezone (raspored, rezultati, tablica, strijelci).
/// Sezona se bira parametrom <c>?sezona=</c>; bez njega se prikazuje aktivna sezona.
/// </summary>
public abstract class SezonaPageModel : PageModel
{
    protected SezonaPageModel(ISezonaRepository sezone)
    {
        SezoneRepo = sezone;
    }

    protected ISezonaRepository SezoneRepo { get; }

    /// <summary>Id sezone iz adrese (npr. /Tablica?sezona=1); SupportsGet = puni se i pri GET zahtjevu.</summary>
    [BindProperty(Name = "sezona", SupportsGet = true)]
    public int? SezonaId { get; set; }

    public Sezona? Sezona { get; private set; }

    public IEnumerable<SelectListItem> Sezone { get; private set; } = [];

    /// <summary>Učitava popis sezona i odabranu (ili aktivnu) sezonu; vraća false ako tražena sezona ne postoji.</summary>
    protected async Task<bool> UcitajSezonuAsync(CancellationToken ct)
    {
        var sve = await SezoneRepo.GetSveAsync(ct);
        Sezona = SezonaId is { } id
            ? sve.FirstOrDefault(s => s.Id == id)
            : await SezoneRepo.GetAktivnaAsync(ct);

        Sezone = sve.Select(s => new SelectListItem(s.Naziv, s.Id.ToString(), s.Id == Sezona?.Id)).ToList();
        return Sezona is not null || SezonaId is null;
    }
}
