using System.ComponentModel.DataAnnotations;

namespace SportskaLiga.Admin.Models;

/// <summary>Obrazac za dodavanje i uređivanje suca.</summary>
public class SudacFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ime je obavezno.")]
    [StringLength(50, ErrorMessage = "Ime može imati najviše {1} znakova.")]
    [Display(Name = "Ime")]
    public string Ime { get; set; } = string.Empty;

    [Required(ErrorMessage = "Prezime je obavezno.")]
    [StringLength(50, ErrorMessage = "Prezime može imati najviše {1} znakova.")]
    [Display(Name = "Prezime")]
    public string Prezime { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Unesite ispravnu e-mail adresu.")]
    [StringLength(100, ErrorMessage = "E-mail može imati najviše {1} znakova.")]
    [Display(Name = "E-mail")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Broj telefona nije ispravan.")]
    [StringLength(30, ErrorMessage = "Telefon može imati najviše {1} znakova.")]
    [Display(Name = "Telefon")]
    public string? Telefon { get; set; }
}
