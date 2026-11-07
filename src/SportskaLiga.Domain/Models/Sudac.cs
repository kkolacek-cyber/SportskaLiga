using System.ComponentModel.DataAnnotations;

namespace SportskaLiga.Domain.Models;

/// <summary>Sudac. Jedan sudac sudi više utakmica (1:N); utakmica ne mora imati suca dok se ne odredi.</summary>
public class Sudac
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

    [EmailAddress(ErrorMessage = "E-mail adresa nije ispravna.")]
    [StringLength(100, ErrorMessage = "E-mail može imati najviše {1} znakova.")]
    [Display(Name = "E-mail")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Broj telefona nije ispravan.")]
    [StringLength(30, ErrorMessage = "Telefon može imati najviše {1} znakova.")]
    [Display(Name = "Telefon")]
    public string? Telefon { get; set; }

    public ICollection<Utakmica> Utakmice { get; set; } = new List<Utakmica>();

    public string PunoIme => $"{Ime} {Prezime}";
}
