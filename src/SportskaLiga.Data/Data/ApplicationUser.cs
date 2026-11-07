using Microsoft.AspNetCore.Identity;

namespace SportskaLiga.Data;

/// <summary>Korisnik administracije i API-ja (Administrator, Delegat).</summary>
public class ApplicationUser : IdentityUser
{
    // Prikazno ime koje se pokazuje u administraciji i na utakmicama kojima je korisnik delegat.
    public string? ImePrezime { get; set; }
}
