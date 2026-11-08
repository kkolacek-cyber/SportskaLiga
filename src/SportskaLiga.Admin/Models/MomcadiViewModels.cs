using System.ComponentModel.DataAnnotations;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Admin.Models;

/// <summary>Popis momčadi s pretragom i straničenjem.</summary>
public class MomcadiIndexViewModel
{
    [Display(Name = "Pretraga")]
    public string? Tekst { get; set; }

    public PagedResult<Momcad> Rezultat { get; set; } = new();
}

/// <summary>Obrazac za registraciju (dodavanje) i uređivanje momčadi.</summary>
public class MomcadFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Naziv momčadi je obavezan.")]
    [StringLength(80, ErrorMessage = "Naziv može imati najviše {1} znakova.")]
    [Display(Name = "Naziv momčadi")]
    public string Naziv { get; set; } = string.Empty;

    [Required(ErrorMessage = "Grad je obavezan.")]
    [StringLength(60, ErrorMessage = "Grad može imati najviše {1} znakova.")]
    [Display(Name = "Grad")]
    public string Grad { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Igralište može imati najviše {1} znakova.")]
    [Display(Name = "Domaće igralište")]
    public string? Igraliste { get; set; }

    [Range(1850, 2100, ErrorMessage = "Unesite ispravnu godinu.")]
    [Display(Name = "Godina osnutka")]
    public int? GodinaOsnutka { get; set; }

    [EmailAddress(ErrorMessage = "Unesite ispravnu e-mail adresu.")]
    [StringLength(100, ErrorMessage = "E-mail može imati najviše {1} znakova.")]
    [Display(Name = "Kontakt e-mail")]
    public string? KontaktEmail { get; set; }
}
