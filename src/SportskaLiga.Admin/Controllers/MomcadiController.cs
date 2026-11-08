using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportskaLiga.Admin.Models;
using SportskaLiga.Domain;
using SportskaLiga.Domain.Enums;
using SportskaLiga.Domain.Interfaces;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Admin.Controllers;

/// <summary>Registracija momčadi: CRUD. Pregled smiju Administrator i Delegat, izmjene samo Administrator.</summary>
[Authorize(Roles = Roles.Osoblje)]
public class MomcadiController : Controller
{
    private const int VelicinaStranice = 15;

    private readonly IMomcadRepository _momcadi;

    public MomcadiController(IMomcadRepository momcadi)
    {
        _momcadi = momcadi;
    }

    // Popis s pretragom (naziv, grad) i straničenjem.
    public async Task<IActionResult> Index(string? tekst, int stranica = 1, CancellationToken ct = default)
    {
        var rezultat = await _momcadi.SearchAsync(tekst, stranica, VelicinaStranice, ct);

        // Stranica izvan raspona (npr. nakon brisanja): prikaži zadnju umjesto praznog popisa.
        if (rezultat.Items.Count == 0 && rezultat.UkupnoStavki > 0)
        {
            rezultat = await _momcadi.SearchAsync(tekst, rezultat.UkupnoStranica, VelicinaStranice, ct);
        }

        return View(new MomcadiIndexViewModel { Tekst = tekst, Rezultat = rezultat });
    }

    // Detalji: podaci, igrači po broju dresa i sezone u kojima momčad sudjeluje.
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var momcad = await _momcadi.GetDetailsAsync(id, ct);
        return momcad is null ? NotFound() : View(momcad);
    }

    [HttpGet]
    [Authorize(Roles = Roles.Administrator)]
    public IActionResult Create() => View("Obrazac", new MomcadFormViewModel());

    [HttpPost]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> Create(MomcadFormViewModel model, CancellationToken ct)
    {
        await ProvjeriNazivAsync(model, ct);
        if (!ModelState.IsValid)
        {
            return View("Obrazac", model);
        }

        var momcad = new Momcad();
        Prepisi(model, momcad);
        await _momcadi.AddAsync(momcad, ct);

        TempData["Poruka"] = $"Momčad \"{momcad.Naziv}\" je registrirana. Sada joj dodajte igrače.";
        return RedirectToAction(nameof(Details), new { id = momcad.Id });
    }

    [HttpGet]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var momcad = await _momcadi.GetByIdAsync(id, ct);
        if (momcad is null)
        {
            return NotFound();
        }

        return View("Obrazac", new MomcadFormViewModel
        {
            Id = momcad.Id,
            Naziv = momcad.Naziv,
            Grad = momcad.Grad,
            Igraliste = momcad.Igraliste,
            GodinaOsnutka = momcad.GodinaOsnutka,
            KontaktEmail = momcad.KontaktEmail
        });
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> Edit(int id, MomcadFormViewModel model, CancellationToken ct)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var momcad = await _momcadi.GetByIdAsync(id, ct);
        if (momcad is null)
        {
            return NotFound();
        }

        await ProvjeriNazivAsync(model, ct);
        if (!ModelState.IsValid)
        {
            return View("Obrazac", model);
        }

        Prepisi(model, momcad);
        await _momcadi.UpdateAsync(momcad, ct);

        TempData["Poruka"] = $"Momčad \"{momcad.Naziv}\" je spremljena.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var momcad = await _momcadi.GetDetailsAsync(id, ct);
        return momcad is null ? NotFound() : View(momcad);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        switch (await _momcadi.DeleteAsync(id, ct))
        {
            case DeleteResult.Deleted:
                TempData["Poruka"] = "Momčad je obrisana.";
                return RedirectToAction(nameof(Index));
            case DeleteResult.InUse:
                // Restrict u bazi: momčad s igračima, prijavama u sezonu ili utakmicama ne može se obrisati.
                TempData["Greska"] = "Momčad se ne može obrisati jer ima igrače, prijave u sezonu ili utakmice.";
                return RedirectToAction(nameof(Details), new { id });
            default:
                return NotFound();
        }
    }

    // Kopira dopuštena polja iz obrasca u entitet (prazan tekst sprema se kao NULL).
    private static void Prepisi(MomcadFormViewModel model, Momcad momcad)
    {
        momcad.Naziv = model.Naziv.Trim();
        momcad.Grad = model.Grad.Trim();
        momcad.Igraliste = Ocisti(model.Igraliste);
        momcad.GodinaOsnutka = model.GodinaOsnutka;
        momcad.KontaktEmail = Ocisti(model.KontaktEmail);
    }

    private async Task ProvjeriNazivAsync(MomcadFormViewModel model, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(model.Naziv)
            && await _momcadi.NazivPostojiAsync(model.Naziv, model.Id == 0 ? null : model.Id, ct))
        {
            ModelState.AddModelError(nameof(MomcadFormViewModel.Naziv), "Momčad s tim nazivom već postoji.");
        }
    }

    private static string? Ocisti(string? tekst) => string.IsNullOrWhiteSpace(tekst) ? null : tekst.Trim();
}
