using System.ComponentModel.DataAnnotations;

namespace SportskaLiga.Api.Dtos;

/// <summary>Podaci za prijavu. Isti korisnici kao u administraciji (Delegat, Administrator).</summary>
public class LoginZahtjevDto
{
    /// <summary>E-mail korisnika.</summary>
    /// <example>delegat@liga.local</example>
    [Required(ErrorMessage = "E-mail je obavezan.")]
    [EmailAddress(ErrorMessage = "Unesite ispravnu e-mail adresu.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Lozinka korisnika.</summary>
    /// <example>Delegat123!</example>
    [Required(ErrorMessage = "Lozinka je obavezna.")]
    public string Lozinka { get; set; } = string.Empty;
}

/// <summary>Odgovor na uspješnu prijavu. Token se šalje u zaglavlju <c>Authorization: Bearer {token}</c>.</summary>
/// <param name="Token">Potpisani JWT.</param>
/// <param name="TipTokena">Uvijek "Bearer".</param>
/// <param name="IstjeceUtc">Trenutak isteka tokena (UTC).</param>
/// <param name="Email">E-mail prijavljenog korisnika.</param>
/// <param name="Uloge">Uloge korisnika (Administrator, Delegat).</param>
public record LoginOdgovorDto(string Token, string TipTokena, DateTime IstjeceUtc, string Email, IReadOnlyList<string> Uloge);
