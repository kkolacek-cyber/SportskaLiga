using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportskaLiga.Admin.Infrastructure;
using SportskaLiga.Admin.Models;
using SportskaLiga.Domain;
using SportskaLiga.Domain.Enums;
using SportskaLiga.Domain.Interfaces;

namespace SportskaLiga.Admin.Controllers;

/// <summary>Prijave momčadi s javnog portala (samo Administrator): pregled, odobravanje i odbijanje.</summary>
[Authorize(Roles = Roles.Administrator)]
public class PrijaveController : Controller
{
    private const int VelicinaStranice = 20;

    private readonly IPrijavaRepository _prijave;

    public PrijaveController(IPrijavaRepository prijave)
    {
        _prijave = prijave;
    }

    // Zadano se prikazuju prijave koje čekaju obradu; "svi" = bez filtra statusa.
    public async Task<IActionResult> Index(StatusPrijave? status, bool svi = false, int stranica = 1, CancellationToken ct = default)
    {
        StatusPrijave? filter = svi ? null : status ?? StatusPrijave.NaCekanju;

        return View(new PrijaveIndexViewModel
        {
            Status = filter,
            Rezultat = await _prijave.SearchAsync(filter, stranica, VelicinaStranice, ct)
        });
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var prijava = await _prijave.GetDetailsAsync(id, ct);
        return prijava is null ? NotFound() : View(new PrijavaObradaViewModel { Prijava = prijava });
    }

    [HttpPost]
    public async Task<IActionResult> Odobri(int id, string? odgovor, CancellationToken ct)
    {
        var rezultat = await _prijave.OdobriAsync(id, odgovor, ct);
        if (rezultat.Uspjeh)
        {
            TempData["Poruka"] = "Prijava je odobrena, a momčad upisana u sezonu.";
            return RedirectToAction(nameof(Details), new { id });
        }

        return await PonovoAsync(id, odgovor, rezultat, ct);
    }

    [HttpPost]
    public async Task<IActionResult> Odbij(int id, string? odgovor, CancellationToken ct)
    {
        var rezultat = await _prijave.OdbijAsync(id, odgovor ?? string.Empty, ct);
        if (rezultat.Uspjeh)
        {
            TempData["Poruka"] = "Prijava je odbijena.";
            return RedirectToAction(nameof(Details), new { id });
        }

        return await PonovoAsync(id, odgovor, rezultat, ct);
    }

    // Neuspjela obrada: ista stranica s porukom (i upisanim odgovorom), bez preusmjeravanja.
    private async Task<IActionResult> PonovoAsync(int id, string? odgovor, Domain.Models.RezultatOperacije rezultat, CancellationToken ct)
    {
        var prijava = await _prijave.GetDetailsAsync(id, ct);
        if (prijava is null)
        {
            return NotFound();
        }

        ModelState.DodajGresku(rezultat);
        return View(nameof(Details), new PrijavaObradaViewModel { Prijava = prijava, Odgovor = odgovor });
    }
}
