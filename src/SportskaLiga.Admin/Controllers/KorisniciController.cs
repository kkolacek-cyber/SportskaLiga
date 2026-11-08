using System.Security.Claims;
using SportskaLiga.Admin.Models;
using SportskaLiga.Data;
using SportskaLiga.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace SportskaLiga.Admin.Controllers;

/// <summary>
/// Upravljanje korisnicima administracije (Administrator i Delegat). Samo Administrator.
/// Sustav se ne može "zaključati": ne može se obrisati ni degradirati zadnji administrator niti vlastiti račun.
/// </summary>
[Authorize(Roles = Roles.Administrator)]
public class KorisniciController : Controller
{
    // Uloge koje se smiju dodijeliti kroz sučelje.
    private static readonly string[] DostupneUloge = [Roles.Administrator, Roles.Delegat];

    private readonly UserManager<ApplicationUser> _korisnici;

    public KorisniciController(UserManager<ApplicationUser> korisnici)
    {
        _korisnici = korisnici;
    }

    // Id prijavljenog administratora; služi da se ne može obrisati ni degradirati vlastiti račun.
    private string? TrenutniId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var korisnici = await _korisnici.Users.OrderBy(u => u.Email).ToListAsync(ct);
        var stavke = new List<KorisnikListItem>();

        // Za svakog korisnika dohvaćaju se uloga i je li zaključan (Identity ga zaključava nakon previše pogrešnih lozinki).
        foreach (var korisnik in korisnici)
        {
            var uloge = await _korisnici.GetRolesAsync(korisnik);
            stavke.Add(new KorisnikListItem
            {
                Id = korisnik.Id,
                Email = korisnik.Email ?? korisnik.UserName ?? string.Empty,
                ImePrezime = korisnik.ImePrezime,
                Uloga = uloge.FirstOrDefault(),
                Zakljucan = await _korisnici.IsLockedOutAsync(korisnik),
                JeTrenutni = korisnik.Id == TrenutniId
            });
        }

        return View(stavke);
    }

    // Create GET prikazuje prazan obrazac (zadana uloga je Delegat), Create POST stvara korisnika.
    [HttpGet]
    public IActionResult Create() => View("Obrazac", UcitajUloge(new KorisnikFormViewModel { Uloga = Roles.Delegat }));

    // POST: lozinka je obavezna samo pri stvaranju (pri uređivanju se prazna lozinka ne mijenja).
    [HttpPost]
    public async Task<IActionResult> Create(KorisnikFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Lozinka))
        {
            ModelState.AddModelError(nameof(KorisnikFormViewModel.Lozinka), "Lozinka je obavezna.");
        }

        if (!DostupneUloge.Contains(model.Uloga))
        {
            ModelState.AddModelError(nameof(KorisnikFormViewModel.Uloga), "Odaberite ispravnu ulogu.");
        }

        if (!ModelState.IsValid)
        {
            return View("Obrazac", UcitajUloge(model));
        }

        var email = model.Email.Trim();
        var korisnik = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            ImePrezime = string.IsNullOrWhiteSpace(model.ImePrezime) ? null : model.ImePrezime.Trim()
        };

        // Identity provjerava pravila lozinke i sprema njezin hash (lozinka se nikad ne sprema u čitljivom obliku).
        var rezultat = await _korisnici.CreateAsync(korisnik, model.Lozinka!);
        if (!rezultat.Succeeded)
        {
            DodajGreske(rezultat);
            return View("Obrazac", UcitajUloge(model));
        }

        await _korisnici.AddToRoleAsync(korisnik, model.Uloga);

        TempData["Poruka"] = $"Korisnik {email} je dodan kao {model.Uloga}.";
        return RedirectToAction(nameof(Index));
    }

    // Edit GET učitava korisnika u obrazac; Edit POST sprema izmjene (podaci, lozinka, uloga).
    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var korisnik = await _korisnici.FindByIdAsync(id);
        if (korisnik is null)
        {
            return NotFound();
        }

        return View("Obrazac", UcitajUloge(new KorisnikFormViewModel
        {
            Id = korisnik.Id,
            Email = korisnik.Email ?? string.Empty,
            ImePrezime = korisnik.ImePrezime,
            Uloga = (await _korisnici.GetRolesAsync(korisnik)).FirstOrDefault() ?? Roles.Delegat,
            JeTrenutni = korisnik.Id == TrenutniId
        }));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string id, KorisnikFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var korisnik = await _korisnici.FindByIdAsync(id);
        if (korisnik is null)
        {
            return NotFound();
        }

        model.JeTrenutni = korisnik.Id == TrenutniId;
        var staraUloga = (await _korisnici.GetRolesAsync(korisnik)).FirstOrDefault();

        if (!DostupneUloge.Contains(model.Uloga))
        {
            ModelState.AddModelError(nameof(KorisnikFormViewModel.Uloga), "Odaberite ispravnu ulogu.");
        }
        else if (model.Uloga != staraUloga)
        {
            if (model.JeTrenutni)
            {
                ModelState.AddModelError(nameof(KorisnikFormViewModel.Uloga), "Ne možete promijeniti vlastitu ulogu.");
            }
            else if (staraUloga == Roles.Administrator && await BrojAdministratoraAsync() <= 1)
            {
                ModelState.AddModelError(nameof(KorisnikFormViewModel.Uloga), "Zadnjem administratoru se ne može promijeniti uloga.");
            }
        }

        if (!ModelState.IsValid)
        {
            return View("Obrazac", UcitajUloge(model));
        }

        var email = model.Email.Trim();
        korisnik.ImePrezime = string.IsNullOrWhiteSpace(model.ImePrezime) ? null : model.ImePrezime.Trim();

        // Izmjene idu redom: e-mail i korisničko ime → ostali podaci → lozinka (ako je unesena); prvi neuspjeh prekida ostale korake.
        var rezultat = IdentityResult.Success;
        if (!string.Equals(korisnik.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            rezultat = await _korisnici.SetEmailAsync(korisnik, email);
            if (rezultat.Succeeded)
            {
                rezultat = await _korisnici.SetUserNameAsync(korisnik, email);
            }
        }

        if (rezultat.Succeeded)
        {
            rezultat = await _korisnici.UpdateAsync(korisnik);
        }

        if (rezultat.Succeeded && !string.IsNullOrWhiteSpace(model.Lozinka))
        {
            // Administrator postavlja novu lozinku bez poznavanja stare, a Identity za to traži token za reset lozinke.
            var zeton = await _korisnici.GeneratePasswordResetTokenAsync(korisnik);
            rezultat = await _korisnici.ResetPasswordAsync(korisnik, zeton, model.Lozinka);
        }

        if (!rezultat.Succeeded)
        {
            DodajGreske(rezultat);
            return View("Obrazac", UcitajUloge(model));
        }

        if (model.Uloga != staraUloga)
        {
            if (staraUloga is not null)
            {
                await _korisnici.RemoveFromRoleAsync(korisnik, staraUloga);
            }

            await _korisnici.AddToRoleAsync(korisnik, model.Uloga);
        }

        TempData["Poruka"] = $"Korisnik {email} je spremljen.";
        return RedirectToAction(nameof(Index));
    }

    // Delete GET traži potvrdu; Delete POST (DeleteConfirmed) briše, uz zaštitu vlastitog računa i zadnjeg administratora.
    [HttpGet]
    public async Task<IActionResult> Delete(string id)
    {
        var korisnik = await _korisnici.FindByIdAsync(id);
        if (korisnik is null)
        {
            return NotFound();
        }

        return View(new KorisnikListItem
        {
            Id = korisnik.Id,
            Email = korisnik.Email ?? string.Empty,
            ImePrezime = korisnik.ImePrezime,
            Uloga = (await _korisnici.GetRolesAsync(korisnik)).FirstOrDefault(),
            JeTrenutni = korisnik.Id == TrenutniId
        });
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var korisnik = await _korisnici.FindByIdAsync(id);
        if (korisnik is null)
        {
            return NotFound();
        }

        if (korisnik.Id == TrenutniId)
        {
            TempData["Greska"] = "Ne možete obrisati vlastiti račun.";
            return RedirectToAction(nameof(Index));
        }

        if (await _korisnici.IsInRoleAsync(korisnik, Roles.Administrator) && await BrojAdministratoraAsync() <= 1)
        {
            TempData["Greska"] = "Zadnji administrator se ne može obrisati.";
            return RedirectToAction(nameof(Index));
        }

        var rezultat = await _korisnici.DeleteAsync(korisnik);
        TempData[rezultat.Succeeded ? "Poruka" : "Greska"] = rezultat.Succeeded
            ? $"Korisnik {korisnik.Email} je obrisan."
            : string.Join(" ", rezultat.Errors.Select(e => e.Description));

        return RedirectToAction(nameof(Index));
    }

    // Koliko administratora postoji; zadnji se ne smije obrisati ni degradirati jer bi se sustav zaključao.
    private async Task<int> BrojAdministratoraAsync() => (await _korisnici.GetUsersInRoleAsync(Roles.Administrator)).Count;

    private void DodajGreske(IdentityResult rezultat)
    {
        foreach (var greska in rezultat.Errors)
        {
            // Pogreške lozinke prikazujemo uz polje lozinke, ostale na vrhu obrasca.
            var polje = greska.Code.StartsWith("Password", StringComparison.Ordinal) ? nameof(KorisnikFormViewModel.Lozinka) : string.Empty;
            ModelState.AddModelError(polje, greska.Description);
        }
    }

    // Puni padajući izbornik uloga u obrascu.
    private static KorisnikFormViewModel UcitajUloge(KorisnikFormViewModel model)
    {
        model.Uloge = DostupneUloge.Select(u => new SelectListItem(u, u, u == model.Uloga)).ToList();
        return model;
    }
}
