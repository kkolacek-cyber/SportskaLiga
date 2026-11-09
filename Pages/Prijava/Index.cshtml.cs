using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SportskaLiga.Domain.Interfaces;
using SportskaLiga.Domain.Models;
using SportskaLiga.ViewModels;

namespace SportskaLiga.Pages.Prijava;

/// <summary>
/// Obrazac za prijavu momčadi u ligu (bez korisničkog računa). Nakon uspjeha podnositelj dobiva kod
/// i preusmjerava se na potvrdu (Post-Redirect-Get).
/// </summary>
public class IndexModel : PageModel
{
    private readonly ISezonaRepository _sezone;
    private readonly IPrijavaRepository _prijave;

    public IndexModel(ISezonaRepository sezone, IPrijavaRepository prijave)
    {
        _sezone = sezone;
        _prijave = prijave;
    }

    // Podaci s obrasca; atributi na PrijavaInput određuju validaciju (na klijentu i poslužitelju).
    [BindProperty]
    public PrijavaInput Input { get; set; } = new();

    public IEnumerable<SelectListItem> Sezone { get; private set; } = [];

    public bool PrijaveOtvorene { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        await UcitajSezoneAsync(ct);
    }

    // POST: validacija modela → pravila i spremanje kroz repozitorij (PodnesiAsync) → preusmjeravanje na potvrdu.
    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!Input.PrihvacamPravila)
        {
            ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.PrihvacamPravila)}", "Morate prihvatiti pravila lige.");
        }

        if (!ModelState.IsValid)
        {
            await UcitajSezoneAsync(ct);
            return Page();
        }

        // Ulazni model → entitet. Status, kod i datum postavlja repozitorij, ne obrazac.
        var prijava = new PrijavaMomcadi
        {
            SezonaId = Input.SezonaId,
            NazivMomcadi = Input.NazivMomcadi,
            Grad = Input.Grad.Trim(),
            Igraliste = Ocisti(Input.Igraliste),
            KontaktOsoba = Input.KontaktOsoba.Trim(),
            KontaktEmail = Input.KontaktEmail.Trim(),
            KontaktTelefon = Ocisti(Input.KontaktTelefon),
            BrojIgraca = Input.BrojIgraca,
            Napomena = Ocisti(Input.Napomena)
        };

        var rezultat = await _prijave.PodnesiAsync(prijava, ct);
        if (!rezultat.Uspjeh)
        {
            // Polje iz repozitorija (npr. "NazivMomcadi") odgovara polju ulaznog modela, pa greška ide uz to polje.
            var kljuc = rezultat.Polje is null ? string.Empty : $"{nameof(Input)}.{rezultat.Polje}";
            ModelState.AddModelError(kljuc, rezultat.Poruka ?? "Prijavu nije moguće podnijeti.");
            await UcitajSezoneAsync(ct);
            return Page();
        }

        // PRG: nakon uspjeha preusmjeravamo na potvrdu, pa osvježavanje stranice ne šalje prijavu ponovno.
        TempData["NovaPrijava"] = prijava.Kod;
        return RedirectToPage("Potvrda", new { kod = prijava.Kod });
    }

    // Nude se samo sezone u koje se još može prijaviti; prva je unaprijed odabrana.
    private async Task UcitajSezoneAsync(CancellationToken ct)
    {
        var otvorene = await _sezone.GetOtvoreneZaPrijaveAsync(ct);
        PrijaveOtvorene = otvorene.Count > 0;

        if (Input.SezonaId == 0 && PrijaveOtvorene)
        {
            Input.SezonaId = otvorene[0].Id;
        }

        Sezone = otvorene.Select(s => new SelectListItem(
            $"{s.Naziv} (od {s.DatumPocetka:dd.MM.yyyy.})", s.Id.ToString(), s.Id == Input.SezonaId));
    }

    private static string? Ocisti(string? tekst) => string.IsNullOrWhiteSpace(tekst) ? null : tekst.Trim();
}
