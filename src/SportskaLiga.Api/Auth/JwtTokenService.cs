using System.Security.Claims;
using System.Text;
using SportskaLiga.Data;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace SportskaLiga.Api.Auth;

/// <summary>Izdavanje JWT tokena prijavljenom korisniku.</summary>
public interface IJwtTokenService
{
    /// <summary>Izdaje potpisani JWT s oznakama sub, email, name i role (jedna po ulozi).</summary>
    (string Token, DateTime IstjeceUtc) Izdaj(ApplicationUser korisnik, IEnumerable<string> uloge);
}

/// <summary>Izdaje JWT potpisan HMAC-SHA256 tajnim ključem iz konfiguracije.</summary>
public class JwtTokenService : IJwtTokenService
{
    /// <summary>Nazivi oznaka u tokenu; JwtBearer ih čita istim imenima (vidi Program.cs).</summary>
    public const string OznakaIme = "name";
    public const string OznakaUloga = "role";

    private readonly JwtOptions _opcije;

    public JwtTokenService(IOptions<JwtOptions> opcije)
    {
        _opcije = opcije.Value;
    }

    public (string Token, DateTime IstjeceUtc) Izdaj(ApplicationUser korisnik, IEnumerable<string> uloge)
    {
        var istjece = DateTime.UtcNow.AddMinutes(_opcije.ExpiryMinutes);
        var email = korisnik.Email ?? korisnik.UserName ?? string.Empty;

        var oznake = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, korisnik.Id),
            new(JwtRegisteredClaimNames.Email, email),
            new(OznakaIme, string.IsNullOrWhiteSpace(korisnik.ImePrezime) ? email : korisnik.ImePrezime),
            // Jedinstveni identifikator ovog tokena.
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };
        // Jedna oznaka "role" po ulozi; po njoj [Authorize(Roles = ...)] odlučuje o pristupu.
        oznake.AddRange(uloge.Select(uloga => new Claim(OznakaUloga, uloga)));

        var opisnik = new SecurityTokenDescriptor
        {
            Issuer = _opcije.Issuer,
            Audience = _opcije.Audience,
            Subject = new ClaimsIdentity(oznake),
            Expires = istjece,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opcije.Key)),
                SecurityAlgorithms.HmacSha256)
        };

        // Token = zaglavlje + oznake + potpis; poslužitelj ga kasnije provjerava istim ključem, pa ga nitko ne može krivotvoriti.
        return (new JsonWebTokenHandler().CreateToken(opisnik), istjece);
    }
}
