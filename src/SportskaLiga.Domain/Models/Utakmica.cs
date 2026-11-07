using System.ComponentModel.DataAnnotations;
using SportskaLiga.Domain.Enums;

namespace SportskaLiga.Domain.Models;

/// <summary>
/// Utakmica jednog kola sezone. Ima dvije veze prema momčadi (domaćin i gost), suca (neobavezno),
/// delegata (Identity korisnik koji upisuje rezultat) i golove (1:N).
/// Rezultat (GoloviDomacin:GoloviGost) je null dok utakmica nije odigrana.
/// </summary>
public class Utakmica
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Odaberite sezonu.")]
    [Display(Name = "Sezona")]
    public int SezonaId { get; set; }

    public Sezona? Sezona { get; set; }

    [Range(1, 100, ErrorMessage = "Kolo mora biti između {1} i {2}.")]
    [Display(Name = "Kolo")]
    public int Kolo { get; set; }

    /// <summary>Početak utakmice u UTC-u; za prikaz se pretvara u hrvatsko vrijeme (<see cref="Kalendar.ULokalno"/>).</summary>
    [Display(Name = "Početak")]
    public DateTime Pocetak { get; set; }

    [StringLength(100, ErrorMessage = "Lokacija može imati najviše {1} znakova.")]
    [Display(Name = "Igralište")]
    public string? Lokacija { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Odaberite domaćina.")]
    [Display(Name = "Domaćin")]
    public int DomacinId { get; set; }

    public Momcad? Domacin { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Odaberite gosta.")]
    [Display(Name = "Gost")]
    public int GostId { get; set; }

    public Momcad? Gost { get; set; }

    [Range(0, 99, ErrorMessage = "Broj golova mora biti između {1} i {2}.")]
    [Display(Name = "Golovi domaćina")]
    public int? GoloviDomacin { get; set; }

    [Range(0, 99, ErrorMessage = "Broj golova mora biti između {1} i {2}.")]
    [Display(Name = "Golovi gosta")]
    public int? GoloviGost { get; set; }

    [Display(Name = "Status")]
    public StatusUtakmice Status { get; set; } = StatusUtakmice.Zakazana;

    [Display(Name = "Sudac")]
    public int? SudacId { get; set; }

    public Sudac? Sudac { get; set; }

    /// <summary>Id Identity korisnika (delegata) koji upisuje rezultat. Domena ne ovisi o Identityju, FK je u Data sloju.</summary>
    [Display(Name = "Delegat")]
    public string? DelegatId { get; set; }

    [StringLength(500, ErrorMessage = "Napomena može imati najviše {1} znakova.")]
    [Display(Name = "Napomena")]
    public string? Napomena { get; set; }

    public ICollection<Gol> Golovi { get; set; } = new List<Gol>();

    /// <summary>"3 : 1" za odigranu utakmicu, inače "-:-".</summary>
    public string Rezultat => GoloviDomacin is int d && GoloviGost is int g ? $"{d} : {g}" : "- : -";
}
