using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SportskaLiga.Admin.Infrastructure;
using SportskaLiga.Admin.Models;
using SportskaLiga.Domain;
using SportskaLiga.Domain.Enums;
using SportskaLiga.Domain.Interfaces;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Admin.Controllers;

/// <summary>
/// Sezone (samo Administrator): CRUD, prijava i odjava momčadi (M:N), aktivna sezona te generiranje i brisanje rasporeda.
/// </summary>
[Authorize(Roles = Roles.Administrator)]
public class SezoneController : Controller
{
    private readonly ISezonaRepository _sezone;
    private readonly IMomcadRepository _momcadi;
    private readonly IUtakmicaRepository _utakmice;

    public SezoneController(ISezonaRepository sezone, IMomcadRepository momcadi, IUtakmicaRepository utakmice)
    {
        _sezone = sezone;
        _momcadi = momcadi;
        _utakmice = utakmice;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _sezone.GetSveAsync(ct));

    // Detalji: prijavljene momčadi, obrazac za prijavu nove momčadi i obrazac za generiranje rasporeda.
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var model = await DetaljiAsync(id, null, ct);
        return model is null ? NotFound() : View(model);
    }

    // ----------------------------------------------------------------- CRUD

    // Create GET prikazuje prazan obrazac s prijedlogom datuma; Create POST sprema (zatim preusmjerava: Post-Redirect-Get).
    [HttpGet]
    public IActionResult Create()
    {
        var godina = Kalendar.Danas.Month >= 7 ? Kalendar.Danas.Year : Kalendar.Danas.Year - 1;
        return View("Obrazac", new SezonaFormViewModel
        {
            Naziv = $"{godina}./{godina + 1}.",
            DatumPocetka = new DateOnly(godina, 8, 1),
            DatumZavrsetka = new DateOnly(godina + 1, 6, 30)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(SezonaFormViewModel model, CancellationToken ct)
    {
        await ProvjeriNazivAsync(model, ct);
        if (!ModelState.IsValid)
        {
            return View("Obrazac", model);
        }

        var sezona = new Sezona
        {
            Naziv = model.Naziv.Trim(),
            DatumPocetka = model.DatumPocetka,
            DatumZavrsetka = model.DatumZavrsetka,
            BodoviZaPobjedu = model.BodoviZaPobjedu,
            BodoviZaNerijeseno = model.BodoviZaNerijeseno
        };
        await _sezone.AddAsync(sezona, ct);

        TempData["Poruka"] = $"Sezona {sezona.Naziv} je dodana. Prijavite momčadi i generirajte raspored.";
        return RedirectToAction(nameof(Details), new { id = sezona.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var sezona = await _sezone.GetByIdAsync(id, ct);
        if (sezona is null)
        {
            return NotFound();
        }

        return View("Obrazac", new SezonaFormViewModel
        {
            Id = sezona.Id,
            Naziv = sezona.Naziv,
            DatumPocetka = sezona.DatumPocetka,
            DatumZavrsetka = sezona.DatumZavrsetka,
            BodoviZaPobjedu = sezona.BodoviZaPobjedu,
            BodoviZaNerijeseno = sezona.BodoviZaNerijeseno
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, SezonaFormViewModel model, CancellationToken ct)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var sezona = await _sezone.GetByIdAsync(id, ct);
        if (sezona is null)
        {
            return NotFound();
        }

        await ProvjeriNazivAsync(model, ct);
        if (!ModelState.IsValid)
        {
            return View("Obrazac", model);
        }

        // Promjena bodovanja odmah mijenja tablicu – ona se ne sprema, nego računa iz utakmica.
        sezona.Naziv = model.Naziv.Trim();
        sezona.DatumPocetka = model.DatumPocetka;
        sezona.DatumZavrsetka = model.DatumZavrsetka;
        sezona.BodoviZaPobjedu = model.BodoviZaPobjedu;
        sezona.BodoviZaNerijeseno = model.BodoviZaNerijeseno;
        await _sezone.UpdateAsync(sezona, ct);

        TempData["Poruka"] = $"Sezona {sezona.Naziv} je spremljena.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var model = await DetaljiAsync(id, null, ct);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        // Brisanje sezone kaskadno briše i njezine prijave, utakmice i golove (vidi SportskaLigaDbContext).
        switch (await _sezone.DeleteAsync(id, ct))
        {
            case DeleteResult.Deleted:
                TempData["Poruka"] = "Sezona je obrisana.";
                return RedirectToAction(nameof(Index));
            case DeleteResult.InUse:
                TempData["Greska"] = "Sezona se ne može obrisati jer se na nju vežu drugi podaci.";
                return RedirectToAction(nameof(Details), new { id });
            default:
                return NotFound();
        }
    }

    // ----------------------------------------------------------------- radnje sezone

    [HttpPost]
    public async Task<IActionResult> Aktiviraj(int id, CancellationToken ct)
        => Odgovor(await _sezone.PostaviAktivnuAsync(id, ct), "Sezona je postavljena kao aktivna.", id);

    [HttpPost]
    public async Task<IActionResult> Prijavi(int id, int momcadId, CancellationToken ct)
        => Odgovor(await _sezone.PrijaviMomcadAsync(id, momcadId, ct), "Momčad je prijavljena u sezonu.", id);

    [HttpPost]
    public async Task<IActionResult> Odjavi(int id, int momcadId, CancellationToken ct)
        => Odgovor(await _sezone.OdjaviMomcadAsync(id, momcadId, ct), "Momčad je odjavljena iz sezone.", id);

    // Generiranje rasporeda: ako ne uspije, detalji se prikazuju ponovno s porukom uz polje obrasca (a ne preusmjeravanjem).
    [HttpPost]
    public async Task<IActionResult> GenerirajRaspored(int id, [Bind(Prefix = "Raspored")] GenerirajRasporedZahtjev zahtjev, CancellationToken ct)
    {
        zahtjev.SezonaId = id;

        if (ModelState.IsValid)
        {
            var rezultat = await _utakmice.GenerirajRasporedAsync(zahtjev, ct);
            if (rezultat.Uspjeh)
            {
                TempData["Poruka"] = $"Raspored je generiran: {rezultat.Vrijednost} utakmica.";
                return RedirectToAction("Index", "Utakmice", new { sezonaId = id });
            }

            ModelState.DodajGresku(rezultat, "Raspored");
        }

        var model = await DetaljiAsync(id, zahtjev, ct);
        return model is null ? NotFound() : View(nameof(Details), model);
    }

    [HttpPost]
    public async Task<IActionResult> ObrisiRaspored(int id, CancellationToken ct)
    {
        var rezultat = await _utakmice.ObrisiRasporedAsync(id, ct);
        return Odgovor(rezultat, $"Raspored je obrisan ({rezultat.Vrijednost} utakmica).", id);
    }

    // ----------------------------------------------------------------- pomoćno

    // Ishod radnje se prenosi porukom (TempData) i preusmjeravanjem na detalje sezone.
    private IActionResult Odgovor(RezultatOperacije rezultat, string porukaUspjeha, int id)
    {
        TempData[rezultat.Uspjeh ? "Poruka" : "Greska"] = rezultat.Uspjeh ? porukaUspjeha : rezultat.Poruka;
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<SezonaDetailsViewModel?> DetaljiAsync(int id, GenerirajRasporedZahtjev? raspored, CancellationToken ct)
    {
        var sezona = await _sezone.GetDetailsAsync(id, ct);
        if (sezona is null)
        {
            return null;
        }

        var prijavljene = sezona.Prijave.Select(p => p.Momcad!).OrderBy(m => m.Naziv).ToList();
        var prijavljeniId = prijavljene.Select(m => m.Id).ToHashSet();
        var poStatusu = await _utakmice.BrojPoStatusuAsync(id, ct);

        // Prijedlog za prvo kolo: prva subota od početka sezone u 16 h.
        var prvaSubota = sezona.DatumPocetka;
        while (prvaSubota.DayOfWeek != DayOfWeek.Saturday)
        {
            prvaSubota = prvaSubota.AddDays(1);
        }

        return new SezonaDetailsViewModel
        {
            Sezona = sezona,
            Momcadi = prijavljene,
            SlobodneMomcadi = (await _momcadi.GetSveAsync(ct))
                .Where(m => !prijavljeniId.Contains(m.Id))
                .Select(m => new SelectListItem(m.Naziv, m.Id.ToString()))
                .ToList(),
            BrojUtakmica = poStatusu.Values.Sum(),
            BrojOdigranih = poStatusu.GetValueOrDefault(StatusUtakmice.Odigrana),
            Raspored = raspored ?? new GenerirajRasporedZahtjev
            {
                SezonaId = id,
                PrviTermin = prvaSubota.ToDateTime(new TimeOnly(16, 0)),
                RazmakDana = 7,
                DvostrukiKrug = true
            }
        };
    }

    // Naziv sezone mora biti jedinstven (sezona koja se uređuje izuzima se iz provjere).
    private async Task ProvjeriNazivAsync(SezonaFormViewModel model, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(model.Naziv)
            && await _sezone.NazivPostojiAsync(model.Naziv, model.Id == 0 ? null : model.Id, ct))
        {
            ModelState.AddModelError(nameof(SezonaFormViewModel.Naziv), "Sezona s tim nazivom već postoji.");
        }
    }
}
