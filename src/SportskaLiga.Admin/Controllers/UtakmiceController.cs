using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SportskaLiga.Admin.Infrastructure;
using SportskaLiga.Admin.Models;
using SportskaLiga.Data;
using SportskaLiga.Domain;
using SportskaLiga.Domain.Enums;
using SportskaLiga.Domain.Interfaces;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Admin.Controllers;

/// <summary>
/// Utakmice: popis (asinkrono, Fetch API), detalji, upis rezultata i strijelaca (delegat svoje utakmice, administrator sve)
/// te ručno dodavanje, uređivanje i brisanje (samo Administrator).
/// </summary>
[Authorize(Roles = Roles.Osoblje)]
public class UtakmiceController : Controller
{
    private const int VelicinaStranice = 20;

    private readonly IUtakmicaRepository _utakmice;
    private readonly ISezonaRepository _sezone;
    private readonly IIgracRepository _igraci;
    private readonly IRepository<Sudac> _suci;
    private readonly UserManager<ApplicationUser> _korisnici;

    public UtakmiceController(
        IUtakmicaRepository utakmice,
        ISezonaRepository sezone,
        IIgracRepository igraci,
        IRepository<Sudac> suci,
        UserManager<ApplicationUser> korisnici)
    {
        _utakmice = utakmice;
        _sezone = sezone;
        _igraci = igraci;
        _suci = suci;
        _korisnici = korisnici;
    }

    private string? KorisnikId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private bool JeAdministrator => User.IsInRole(Roles.Administrator);

    // ----------------------------------------------------------------- popis (asinkroni prikaz)

    /// <summary>Samo okvir stranice s filtrima; tablicu učitava utakmice.js pozivom na akciju <see cref="Lista"/>.</summary>
    public async Task<IActionResult> Index(int? sezonaId, int? kolo, StatusUtakmice? status, bool? samoMoje, bool samoCekajuRezultat, CancellationToken ct)
    {
        var sezone = await _sezone.GetSveAsync(ct);
        var odabrana = sezonaId ?? (await _sezone.GetAktivnaAsync(ct))?.Id;

        return View(new UtakmiceIndexViewModel
        {
            SezonaId = odabrana,
            Kolo = kolo,
            Status = status,
            // Delegat po zadanom vidi samo svoje utakmice.
            SamoMoje = samoMoje ?? !JeAdministrator,
            SamoCekajuRezultat = samoCekajuRezultat,
            Sezone = sezone.Select(s => new SelectListItem(s.Naziv, s.Id.ToString(), s.Id == odabrana)),
            Kola = odabrana is null ? [] : (await _utakmice.GetKolaAsync(odabrana.Value, ct))
                .Select(k => new SelectListItem($"{k}. kolo", k.ToString(), k == kolo))
        });
    }

    /// <summary>HTML fragment s tablicom utakmica i straničenjem; poziva ga utakmice.js pomoću fetch().</summary>
    public async Task<IActionResult> Lista(int? sezonaId, int? kolo, StatusUtakmice? status, bool samoMoje, bool samoCekajuRezultat, int stranica = 1, CancellationToken ct = default)
    {
        var filter = new UtakmicaFilter
        {
            SezonaId = sezonaId,
            Kolo = kolo,
            Status = status,
            DelegatId = samoMoje ? KorisnikId : null,
            SamoCekajuRezultat = samoCekajuRezultat,
            Stranica = stranica,
            VelicinaStranice = VelicinaStranice
        };

        var rezultat = await _utakmice.SearchAsync(filter, ct);
        if (rezultat.Items.Count == 0 && rezultat.UkupnoStavki > 0)
        {
            filter.Stranica = rezultat.UkupnoStranica;
            rezultat = await _utakmice.SearchAsync(filter, ct);
        }

        return PartialView("_Lista", new UtakmiceListaViewModel
        {
            Rezultat = rezultat,
            Delegati = await DelegatiPoIduAsync(),
            KorisnikId = KorisnikId,
            JeAdministrator = JeAdministrator
        });
    }

    /// <summary>JSON s kolima sezone; utakmice.js njime puni izbornik kola kad se promijeni sezona.</summary>
    [HttpGet]
    public async Task<IActionResult> Kola(int sezonaId, CancellationToken ct)
        => Json(await _utakmice.GetKolaAsync(sezonaId, ct));

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var utakmica = await _utakmice.GetDetailsAsync(id, ct);
        if (utakmica is null)
        {
            return NotFound();
        }

        var delegat = utakmica.DelegatId is null ? null : await _korisnici.FindByIdAsync(utakmica.DelegatId);

        return View(new UtakmicaDetailsViewModel
        {
            Utakmica = utakmica,
            Delegat = delegat is null ? null : delegat.ImePrezime ?? delegat.Email,
            SmijeUpisati = JeAdministrator || (utakmica.DelegatId is not null && utakmica.DelegatId == KorisnikId)
        });
    }

    // ----------------------------------------------------------------- upis rezultata

    [HttpGet]
    public async Task<IActionResult> Rezultat(int id, CancellationToken ct)
    {
        var utakmica = await _utakmice.GetDetailsAsync(id, ct);
        if (utakmica is null)
        {
            return NotFound();
        }

        // Ista provjera kao u repozitoriju, ovdje samo da delegat odmah dobije "Pristup odbijen" umjesto obrasca.
        if (!JeAdministrator && utakmica.DelegatId != KorisnikId)
        {
            return Forbid();
        }

        // Postojeći rezultat se učitava u obrazac (ispravak pogrešno upisanog rezultata).
        var unos = new UnosRezultata
        {
            GoloviDomacin = utakmica.GoloviDomacin ?? 0,
            GoloviGost = utakmica.GoloviGost ?? 0,
            Golovi = utakmica.Golovi
                .OrderBy(g => g.Minuta)
                .Select(g => new UnosGola { IgracId = g.IgracId, Minuta = g.Minuta, Autogol = g.Autogol })
                .ToList()
        };

        return View(await RezultatModelAsync(utakmica, unos, ct));
    }

    [HttpPost]
    public async Task<IActionResult> Rezultat(int id, [Bind(Prefix = "Unos")] UnosRezultata unos, CancellationToken ct)
    {
        if (ModelState.IsValid)
        {
            // Sva pravila (ovlast, vrijeme, broj strijelaca, momčadi strijelaca) provjerava repozitorij.
            var rezultat = await _utakmice.UnesiRezultatAsync(id, unos, KorisnikId, JeAdministrator, ct);
            if (rezultat.Uspjeh)
            {
                TempData["Poruka"] = $"Rezultat {unos.GoloviDomacin}:{unos.GoloviGost} je upisan.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (rezultat.Pogreska == PogreskaOperacije.NemaOvlasti)
            {
                return Forbid();
            }

            if (rezultat.Pogreska == PogreskaOperacije.NijePronadeno)
            {
                return NotFound();
            }

            ModelState.DodajGresku(rezultat, "Unos");
        }

        var utakmica = await _utakmice.GetDetailsAsync(id, ct);
        return utakmica is null ? NotFound() : View(await RezultatModelAsync(utakmica, unos, ct));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> PonistiRezultat(int id, CancellationToken ct)
    {
        var rezultat = await _utakmice.PonistiRezultatAsync(id, ct);
        TempData[rezultat.Uspjeh ? "Poruka" : "Greska"] = rezultat.Uspjeh ? "Rezultat je poništen." : rezultat.Poruka;
        return RedirectToAction(nameof(Details), new { id });
    }

    // ----------------------------------------------------------------- ručni CRUD (Administrator)

    [HttpGet]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> Create(int? sezonaId, CancellationToken ct)
    {
        var model = new UtakmicaFormViewModel
        {
            SezonaId = sezonaId ?? (await _sezone.GetAktivnaAsync(ct))?.Id ?? 0,
            Pocetak = SljedecaSubota()
        };

        return View("Obrazac", await UcitajPopiseAsync(model, ct));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> Create(UtakmicaFormViewModel model, CancellationToken ct)
    {
        var utakmica = new Utakmica();
        Prepisi(model, utakmica);

        if (ModelState.IsValid)
        {
            ModelState.DodajGresku(await _utakmice.ProvjeriAsync(utakmica, ct));
        }

        if (!ModelState.IsValid)
        {
            return View("Obrazac", await UcitajPopiseAsync(model, ct));
        }

        await _utakmice.AddAsync(utakmica, ct);

        TempData["Poruka"] = "Utakmica je dodana.";
        return RedirectToAction(nameof(Details), new { id = utakmica.Id });
    }

    [HttpGet]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var utakmica = await _utakmice.GetByIdAsync(id, ct);
        if (utakmica is null)
        {
            return NotFound();
        }

        var model = new UtakmicaFormViewModel
        {
            Id = utakmica.Id,
            SezonaId = utakmica.SezonaId,
            Kolo = utakmica.Kolo,
            Pocetak = Kalendar.ULokalno(utakmica.Pocetak),
            Lokacija = utakmica.Lokacija,
            DomacinId = utakmica.DomacinId,
            GostId = utakmica.GostId,
            SudacId = utakmica.SudacId,
            DelegatId = utakmica.DelegatId,
            Status = utakmica.Status,
            Napomena = utakmica.Napomena,
            Odigrana = utakmica.Status == StatusUtakmice.Odigrana
        };

        return View("Obrazac", await UcitajPopiseAsync(model, ct));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> Edit(int id, UtakmicaFormViewModel model, CancellationToken ct)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var utakmica = await _utakmice.GetByIdAsync(id, ct);
        if (utakmica is null)
        {
            return NotFound();
        }

        // Odigranoj utakmici ne mijenjamo momčadi ni status (golovi su vezani uz te momčadi) – najprije poništiti rezultat.
        model.Odigrana = utakmica.Status == StatusUtakmice.Odigrana;
        if (model.Odigrana)
        {
            model.SezonaId = utakmica.SezonaId;
            model.DomacinId = utakmica.DomacinId;
            model.GostId = utakmica.GostId;
            model.Status = StatusUtakmice.Odigrana;
        }

        Prepisi(model, utakmica);

        if (ModelState.IsValid)
        {
            ModelState.DodajGresku(await _utakmice.ProvjeriAsync(utakmica, ct));
        }

        if (!ModelState.IsValid)
        {
            return View("Obrazac", await UcitajPopiseAsync(model, ct));
        }

        await _utakmice.UpdateAsync(utakmica, ct);

        TempData["Poruka"] = "Utakmica je spremljena.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var utakmica = await _utakmice.GetDetailsAsync(id, ct);
        return utakmica is null ? NotFound() : View(utakmica);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var sezonaId = (await _utakmice.GetByIdAsync(id, ct))?.SezonaId;

        // Cascade u bazi: brisanjem utakmice brišu se i njezini golovi.
        switch (await _utakmice.DeleteAsync(id, ct))
        {
            case DeleteResult.Deleted:
                TempData["Poruka"] = "Utakmica je obrisana.";
                return RedirectToAction(nameof(Index), new { sezonaId });
            case DeleteResult.InUse:
                TempData["Greska"] = "Utakmica se ne može obrisati.";
                return RedirectToAction(nameof(Details), new { id });
            default:
                return NotFound();
        }
    }

    // ----------------------------------------------------------------- pomoćno

    // Kopira dopuštena polja obrasca u entitet; vrijeme iz obrasca je lokalno, u bazu ide UTC.
    private static void Prepisi(UtakmicaFormViewModel model, Utakmica utakmica)
    {
        utakmica.SezonaId = model.SezonaId;
        utakmica.Kolo = model.Kolo;
        utakmica.Pocetak = Kalendar.UUtc(model.Pocetak);
        utakmica.Lokacija = string.IsNullOrWhiteSpace(model.Lokacija) ? null : model.Lokacija.Trim();
        utakmica.DomacinId = model.DomacinId;
        utakmica.GostId = model.GostId;
        utakmica.SudacId = model.SudacId;
        utakmica.DelegatId = string.IsNullOrEmpty(model.DelegatId) ? null : model.DelegatId;
        utakmica.Napomena = string.IsNullOrWhiteSpace(model.Napomena) ? null : model.Napomena.Trim();

        // "Odigrana" se ne bira ručno; ručno su dopušteni samo Zakazana i Odgođena.
        if (!model.Odigrana)
        {
            utakmica.Status = model.Status == StatusUtakmice.Odgodena ? StatusUtakmice.Odgodena : StatusUtakmice.Zakazana;
        }
    }

    private async Task<UtakmicaFormViewModel> UcitajPopiseAsync(UtakmicaFormViewModel model, CancellationToken ct)
    {
        model.Sezone = (await _sezone.GetSveAsync(ct))
            .Select(s => new SelectListItem(s.Naziv, s.Id.ToString(), s.Id == model.SezonaId));

        // Za odabir momčadi nude se samo momčadi prijavljene u sezonu.
        IReadOnlyList<Momcad> momcadi = model.SezonaId > 0 ? await _sezone.GetMomcadiAsync(model.SezonaId, ct) : [];
        model.Momcadi = momcadi.Select(m => new SelectListItem(m.Naziv, m.Id.ToString())).ToList();

        model.Suci = (await _suci.GetAllAsync(ct))
            .OrderBy(s => s.Prezime)
            .Select(s => new SelectListItem(s.PunoIme, s.Id.ToString(), s.Id == model.SudacId));

        model.Delegati = (await DelegatiPoIduAsync())
            .OrderBy(d => d.Value)
            .Select(d => new SelectListItem(d.Value, d.Key, d.Key == model.DelegatId));

        model.Statusi = (model.Odigrana ? [StatusUtakmice.Odigrana] : new[] { StatusUtakmice.Zakazana, StatusUtakmice.Odgodena })
            .Select(s => new SelectListItem(s.NazivZaPrikaz(), s.ToString(), s == model.Status));

        return model;
    }

    // Obrazac rezultata: igrači obje momčadi u padajućem izborniku, grupirani po momčadi (<optgroup>).
    private async Task<RezultatViewModel> RezultatModelAsync(Utakmica utakmica, UnosRezultata unos, CancellationToken ct)
    {
        var igraci = await _igraci.GetByMomcadimaAsync([utakmica.DomacinId, utakmica.GostId], ct);
        var domacin = new SelectListGroup { Name = $"{utakmica.Domacin?.Naziv} (domaćin)" };
        var gost = new SelectListGroup { Name = $"{utakmica.Gost?.Naziv} (gost)" };

        return new RezultatViewModel
        {
            Utakmica = utakmica,
            Unos = unos,
            Igraci = igraci
                .OrderBy(i => i.MomcadId == utakmica.DomacinId ? 0 : 1)
                .ThenBy(i => i.BrojDresa)
                .Select(i => new SelectListItem($"{i.BrojDresa}. {i.PunoIme}", i.Id.ToString())
                {
                    Group = i.MomcadId == utakmica.DomacinId ? domacin : gost
                })
                .ToList()
        };
    }

    // Identity korisnici u ulozi Delegat (Id → prikazno ime); domena ne zna za Identity, pa ih dohvaća kontroler.
    private async Task<IReadOnlyDictionary<string, string>> DelegatiPoIduAsync()
        => (await _korisnici.GetUsersInRoleAsync(Roles.Delegat))
            .ToDictionary(u => u.Id, u => u.ImePrezime ?? u.Email ?? u.Id);

    // Prijedlog termina za novu utakmicu: sljedeća subota u 16 h.
    private static DateTime SljedecaSubota()
    {
        var dan = Kalendar.Danas.AddDays(1);
        while (dan.DayOfWeek != DayOfWeek.Saturday)
        {
            dan = dan.AddDays(1);
        }

        return dan.ToDateTime(new TimeOnly(16, 0));
    }
}
