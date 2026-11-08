using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SportskaLiga.Admin.Models;
using SportskaLiga.Domain;
using SportskaLiga.Domain.Enums;
using SportskaLiga.Domain.Interfaces;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Admin.Controllers;

/// <summary>Registracija igrača: CRUD. Pregled smiju Administrator i Delegat, izmjene samo Administrator.</summary>
[Authorize(Roles = Roles.Osoblje)]
public class IgraciController : Controller
{
    private const int VelicinaStranice = 20;

    private readonly IIgracRepository _igraci;
    private readonly IMomcadRepository _momcadi;

    public IgraciController(IIgracRepository igraci, IMomcadRepository momcadi)
    {
        _igraci = igraci;
        _momcadi = momcadi;
    }

    // Popis s filtrima (tekst, momčad, pozicija) i straničenjem.
    public async Task<IActionResult> Index(string? tekst, int? momcadId, Pozicija? pozicija, int stranica = 1, CancellationToken ct = default)
    {
        var filter = new IgracFilter
        {
            Tekst = tekst,
            MomcadId = momcadId,
            Pozicija = pozicija,
            Stranica = stranica,
            VelicinaStranice = VelicinaStranice
        };

        var rezultat = await _igraci.SearchAsync(filter, ct);
        if (rezultat.Items.Count == 0 && rezultat.UkupnoStavki > 0)
        {
            filter.Stranica = rezultat.UkupnoStranica;
            rezultat = await _igraci.SearchAsync(filter, ct);
        }

        return View(new IgraciIndexViewModel
        {
            Tekst = tekst,
            MomcadId = momcadId,
            Pozicija = pozicija,
            Rezultat = rezultat,
            Momcadi = await MomcadiSelectAsync(momcadId, ct)
        });
    }

    // momcadId: kad se igrač dodaje s detalja momčadi, momčad je unaprijed odabrana.
    [HttpGet]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> Create(int? momcadId, CancellationToken ct)
    {
        var model = new IgracFormViewModel { MomcadId = momcadId ?? 0 };
        model.Momcadi = await MomcadiSelectAsync(model.MomcadId, ct);
        return View("Obrazac", model);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> Create(IgracFormViewModel model, CancellationToken ct)
    {
        await ProvjeriBrojDresaAsync(model, ct);
        if (!ModelState.IsValid)
        {
            model.Momcadi = await MomcadiSelectAsync(model.MomcadId, ct);
            return View("Obrazac", model);
        }

        var igrac = new Igrac();
        Prepisi(model, igrac);
        await _igraci.AddAsync(igrac, ct);

        TempData["Poruka"] = $"Igrač {igrac.PunoIme} je registriran.";
        return RedirectToAction("Details", "Momcadi", new { id = igrac.MomcadId });
    }

    [HttpGet]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var igrac = await _igraci.GetByIdAsync(id, ct);
        if (igrac is null)
        {
            return NotFound();
        }

        return View("Obrazac", new IgracFormViewModel
        {
            Id = igrac.Id,
            Ime = igrac.Ime,
            Prezime = igrac.Prezime,
            DatumRodenja = igrac.DatumRodenja,
            BrojDresa = igrac.BrojDresa,
            Pozicija = igrac.Pozicija,
            MomcadId = igrac.MomcadId,
            Momcadi = await MomcadiSelectAsync(igrac.MomcadId, ct)
        });
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> Edit(int id, IgracFormViewModel model, CancellationToken ct)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var igrac = await _igraci.GetByIdAsync(id, ct);
        if (igrac is null)
        {
            return NotFound();
        }

        await ProvjeriBrojDresaAsync(model, ct);
        if (!ModelState.IsValid)
        {
            model.Momcadi = await MomcadiSelectAsync(model.MomcadId, ct);
            return View("Obrazac", model);
        }

        // Prelazak u drugi klub ne mijenja povijest: golovi pamte momčad za koju su postignuti (Gol.MomcadId).
        Prepisi(model, igrac);
        await _igraci.UpdateAsync(igrac, ct);

        TempData["Poruka"] = $"Igrač {igrac.PunoIme} je spremljen.";
        return RedirectToAction("Details", "Momcadi", new { id = igrac.MomcadId });
    }

    [HttpGet]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var igrac = await _igraci.GetDetailsAsync(id, ct);
        return igrac is null ? NotFound() : View(igrac);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var momcadId = (await _igraci.GetByIdAsync(id, ct))?.MomcadId;

        switch (await _igraci.DeleteAsync(id, ct))
        {
            case DeleteResult.Deleted:
                TempData["Poruka"] = "Igrač je obrisan.";
                return RedirectToAction("Details", "Momcadi", new { id = momcadId });
            case DeleteResult.InUse:
                // Restrict u bazi: igrač koji je zabio gol ostaje, da lista strijelaca i zapisnici ostanu točni.
                TempData["Greska"] = "Igrač se ne može obrisati jer je zabio golove na odigranim utakmicama.";
                return RedirectToAction("Details", "Momcadi", new { id = momcadId });
            default:
                return NotFound();
        }
    }

    private static void Prepisi(IgracFormViewModel model, Igrac igrac)
    {
        igrac.Ime = model.Ime.Trim();
        igrac.Prezime = model.Prezime.Trim();
        igrac.DatumRodenja = model.DatumRodenja;
        igrac.BrojDresa = model.BrojDresa;
        igrac.Pozicija = model.Pozicija;
        igrac.MomcadId = model.MomcadId;
    }

    // Broj dresa je jedinstven unutar momčadi (isto pravilo čuva i unique indeks u bazi).
    private async Task ProvjeriBrojDresaAsync(IgracFormViewModel model, CancellationToken ct)
    {
        if (model.MomcadId > 0
            && await _igraci.BrojDresaZauzetAsync(model.MomcadId, model.BrojDresa, model.Id == 0 ? null : model.Id, ct))
        {
            ModelState.AddModelError(nameof(IgracFormViewModel.BrojDresa), $"Broj {model.BrojDresa} u toj momčadi već nosi drugi igrač.");
        }
    }

    private async Task<IEnumerable<SelectListItem>> MomcadiSelectAsync(int? odabrana, CancellationToken ct)
        => (await _momcadi.GetSveAsync(ct))
            .Select(m => new SelectListItem(m.Naziv, m.Id.ToString(), m.Id == odabrana))
            .ToList();
}
