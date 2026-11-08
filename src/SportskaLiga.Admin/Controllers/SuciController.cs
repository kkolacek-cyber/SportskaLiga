using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportskaLiga.Admin.Models;
using SportskaLiga.Domain;
using SportskaLiga.Domain.Enums;
using SportskaLiga.Domain.Interfaces;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Admin.Controllers;

/// <summary>
/// Suci: CRUD (samo Administrator). Sudac nema posebnu logiku, pa koristi opći repozitorij <see cref="IRepository{T}"/>.
/// </summary>
[Authorize(Roles = Roles.Administrator)]
public class SuciController : Controller
{
    private readonly IRepository<Sudac> _suci;

    public SuciController(IRepository<Sudac> suci)
    {
        _suci = suci;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var suci = (await _suci.GetAllAsync(ct))
            .OrderBy(s => s.Prezime)
            .ThenBy(s => s.Ime)
            .ToList();

        return View(suci);
    }

    [HttpGet]
    public IActionResult Create() => View("Obrazac", new SudacFormViewModel());

    [HttpPost]
    public async Task<IActionResult> Create(SudacFormViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View("Obrazac", model);
        }

        var sudac = new Sudac();
        Prepisi(model, sudac);
        await _suci.AddAsync(sudac, ct);

        TempData["Poruka"] = $"Sudac {sudac.PunoIme} je dodan.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var sudac = await _suci.GetByIdAsync(id, ct);
        if (sudac is null)
        {
            return NotFound();
        }

        return View("Obrazac", new SudacFormViewModel
        {
            Id = sudac.Id,
            Ime = sudac.Ime,
            Prezime = sudac.Prezime,
            Email = sudac.Email,
            Telefon = sudac.Telefon
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, SudacFormViewModel model, CancellationToken ct)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var sudac = await _suci.GetByIdAsync(id, ct);
        if (sudac is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View("Obrazac", model);
        }

        Prepisi(model, sudac);
        await _suci.UpdateAsync(sudac, ct);

        TempData["Poruka"] = $"Sudac {sudac.PunoIme} je spremljen.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var sudac = await _suci.GetByIdAsync(id, ct);
        return sudac is null ? NotFound() : View(sudac);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        // SetNull u bazi: utakmice obrisanog suca ostaju, samo bez suca.
        switch (await _suci.DeleteAsync(id, ct))
        {
            case DeleteResult.Deleted:
                TempData["Poruka"] = "Sudac je obrisan.";
                return RedirectToAction(nameof(Index));
            case DeleteResult.InUse:
                TempData["Greska"] = "Sudac se ne može obrisati.";
                return RedirectToAction(nameof(Index));
            default:
                return NotFound();
        }
    }

    private static void Prepisi(SudacFormViewModel model, Sudac sudac)
    {
        sudac.Ime = model.Ime.Trim();
        sudac.Prezime = model.Prezime.Trim();
        sudac.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        sudac.Telefon = string.IsNullOrWhiteSpace(model.Telefon) ? null : model.Telefon.Trim();
    }
}
