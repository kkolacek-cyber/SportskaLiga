using System.ComponentModel.DataAnnotations;
using SportskaLiga.Domain.Enums;

namespace SportskaLiga.Api.Dtos;

/// <summary>Momčad u popisu.</summary>
public record MomcadDto(
    int Id,
    string Naziv,
    string Grad,
    string? Igraliste,
    int? GodinaOsnutka,
    string? KontaktEmail,
    int BrojIgraca);

/// <summary>Momčad s igračima i sezonama u kojima je nastupala.</summary>
public record MomcadDetaljiDto(
    int Id,
    string Naziv,
    string Grad,
    string? Igraliste,
    int? GodinaOsnutka,
    string? KontaktEmail,
    IReadOnlyList<IgracDto> Igraci,
    IReadOnlyList<string> Sezone);

/// <summary>Igrač momčadi.</summary>
public record IgracDto(int Id, string Ime, string Prezime, int BrojDresa, Pozicija Pozicija, DateOnly DatumRodenja);

/// <summary>Podaci za dodavanje (POST) i izmjenu (PUT) momčadi.</summary>
public class MomcadUnosDto
{
    /// <summary>Jedinstveni naziv (ne razlikuju se velika i mala slova).</summary>
    /// <example>NK Zeleni Vrt</example>
    [Required(ErrorMessage = "Naziv momčadi je obavezan.")]
    [StringLength(80, ErrorMessage = "Naziv može imati najviše {1} znakova.")]
    public string Naziv { get; set; } = string.Empty;

    /// <example>Varaždin</example>
    [Required(ErrorMessage = "Grad je obavezan.")]
    [StringLength(60, ErrorMessage = "Grad može imati najviše {1} znakova.")]
    public string Grad { get; set; } = string.Empty;

    /// <example>Igralište uz Dravu</example>
    [StringLength(100, ErrorMessage = "Igralište može imati najviše {1} znakova.")]
    public string? Igraliste { get; set; }

    /// <example>1998</example>
    [Range(1850, 2100, ErrorMessage = "Unesite ispravnu godinu.")]
    public int? GodinaOsnutka { get; set; }

    /// <example>tajnik@zelenivrt.hr</example>
    [EmailAddress(ErrorMessage = "E-mail adresa nije ispravna.")]
    [StringLength(100, ErrorMessage = "E-mail može imati najviše {1} znakova.")]
    public string? KontaktEmail { get; set; }
}
