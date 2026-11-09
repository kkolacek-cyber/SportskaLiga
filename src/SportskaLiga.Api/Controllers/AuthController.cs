using SportskaLiga.Api.Auth;
using SportskaLiga.Api.Dtos;
using SportskaLiga.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace SportskaLiga.Api.Controllers;

/// <summary>Prijava i izdavanje JWT tokena.</summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _korisnici;
    private readonly SignInManager<ApplicationUser> _prijava;
    private readonly IJwtTokenService _tokeni;

    public AuthController(UserManager<ApplicationUser> korisnici, SignInManager<ApplicationUser> prijava, IJwtTokenService tokeni)
    {
        _korisnici = korisnici;
        _prijava = prijava;
        _tokeni = tokeni;
    }

    /// <summary>Prijavljuje korisnika i vraća JWT token.</summary>
    /// <remarks>
    /// Token je potreban za sve operacije izmjene (POST, PUT, DELETE) i za popis utakmica koje čekaju rezultat.
    /// Nakon 5 neuspjelih pokušaja račun se na 5 minuta zaključava.
    /// </remarks>
    /// <response code="200">Prijava uspjela; odgovor sadrži token.</response>
    /// <response code="400">Podaci nisu ispravni (nedostaje e-mail ili lozinka).</response>
    /// <response code="401">Neispravan e-mail ili lozinka, ili je račun privremeno zaključan.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<LoginOdgovorDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginOdgovorDto>> Login(LoginZahtjevDto zahtjev)
    {
        // Traži korisnika po e-mailu i provjerava lozinku; lockoutOnFailure: neuspjeli pokušaji se broje, a nakon 5 se račun zaključava.
        var korisnik = await _korisnici.FindByEmailAsync(zahtjev.Email.Trim());
        var rezultat = korisnik is null
            ? Microsoft.AspNetCore.Identity.SignInResult.Failed
            : await _prijava.CheckPasswordSignInAsync(korisnik, zahtjev.Lozinka, lockoutOnFailure: true);

        if (!rezultat.Succeeded)
        {
            // Isti odgovor za nepostojeći račun i krivu lozinku, da se ne otkriva koji e-mailovi postoje.
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Prijava nije uspjela",
                detail: rezultat.IsLockedOut
                    ? "Račun je privremeno zaključan zbog previše neuspjelih pokušaja. Pokušajte ponovno za nekoliko minuta."
                    : "Neispravan e-mail ili lozinka.");
        }

        // Uloge idu u token; po njima API odlučuje smije li korisnik pozvati pojedini endpoint.
        var uloge = (await _korisnici.GetRolesAsync(korisnik!)).ToList();
        var (token, istjece) = _tokeni.Izdaj(korisnik!, uloge);

        return Ok(new LoginOdgovorDto(token, "Bearer", istjece, korisnik!.Email ?? string.Empty, uloge));
    }
}
