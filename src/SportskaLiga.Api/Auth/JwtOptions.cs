using System.ComponentModel.DataAnnotations;

namespace SportskaLiga.Api.Auth;

/// <summary>
/// Postavke JWT tokena (konfiguracijski odjeljak <c>Jwt</c>; u Dockeru varijable <c>Jwt__Key</c>, <c>Jwt__Issuer</c>, ...).
/// Provjeravaju se pri pokretanju: bez ispravnog ključa API se ne podiže.
/// </summary>
public class JwtOptions
{
    public const string Section = "Jwt";

    /// <summary>Tajni ključ za potpisivanje (HMAC-SHA256), najmanje 32 znaka. Nikad se ne commita.</summary>
    [Required(ErrorMessage = "Nedostaje Jwt:Key (varijabla okruženja Jwt__Key / JWT_KEY).")]
    [MinLength(32, ErrorMessage = "Jwt:Key mora imati najmanje 32 znaka.")]
    public string Key { get; set; } = string.Empty;

    /// <summary>Izdavatelj tokena; API odbija tokene s drugim izdavateljem.</summary>
    [Required]
    public string Issuer { get; set; } = "sportskaliga-api";

    /// <summary>Primatelj tokena; API odbija tokene namijenjene drugom primatelju.</summary>
    [Required]
    public string Audience { get; set; } = "sportskaliga-clients";

    /// <summary>Koliko minuta token vrijedi nakon prijave.</summary>
    [Range(1, 24 * 60, ErrorMessage = "Jwt:ExpiryMinutes mora biti između 1 i 1440.")]
    public int ExpiryMinutes { get; set; } = 60;
}
