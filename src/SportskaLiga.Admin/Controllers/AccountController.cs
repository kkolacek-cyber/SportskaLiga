using SportskaLiga.Admin.Models;
using SportskaLiga.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace SportskaLiga.Admin.Controllers;

/// <summary>Prijava i odjava u administraciju (ASP.NET Core Identity, prijava kolačićem). Kontroler je javan jer se korisnik tek prijavljuje.</summary>
[AllowAnonymous]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _korisnici;
    private readonly SignInManager<ApplicationUser> _prijava;

    public AccountController(UserManager<ApplicationUser> korisnici, SignInManager<ApplicationUser> prijava)
    {
        _korisnici = korisnici;
        _prijava = prijava;
    }

    // GET: prikazuje obrazac za prijavu; već prijavljenog korisnika odmah šalje na početnu stranicu.
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // POST: provjerava podatke i prijavljuje korisnika (postavlja kolačić prijave).
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var korisnik = await _korisnici.FindByEmailAsync(model.Email.Trim());
        // PasswordSignInAsync provjerava lozinku i postavlja kolačić; lockoutOnFailure: nakon 5 pogrešnih pokušaja račun se zaključava na 5 minuta.
        var rezultat = korisnik is null
            ? Microsoft.AspNetCore.Identity.SignInResult.Failed
            : await _prijava.PasswordSignInAsync(korisnik, model.Lozinka, model.ZapamtiMe, lockoutOnFailure: true);

        if (rezultat.Succeeded)
        {
            // Vraća na stranicu koju je korisnik tražio, ali samo ako je lokalna (zaštita od preusmjeravanja na tuđu adresu).
            return !string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl)
                ? LocalRedirect(model.ReturnUrl)
                : RedirectToAction("Index", "Home");
        }

        // Isti odgovor za nepostojeći račun i krivu lozinku, da se ne otkriva koji e-mailovi postoje.
        ModelState.AddModelError(string.Empty, rezultat.IsLockedOut
            ? "Račun je privremeno zaključan zbog previše neuspjelih pokušaja. Pokušajte ponovno za nekoliko minuta."
            : "Neispravan e-mail ili lozinka.");

        model.Lozinka = string.Empty;
        return View(model);
    }

    // Odjava je POST (ne GET) da je ne može izazvati tuđa poveznica; antiforgery token provjerava se globalno.
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _prijava.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    // Prikazuje se kad je korisnik prijavljen, ali njegova uloga nema pravo na traženu stranicu (HTTP 403).
    [HttpGet]
    public IActionResult AccessDenied() => View();
}
