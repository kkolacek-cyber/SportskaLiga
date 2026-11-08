using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SportskaLiga.Admin.Models;

/// <summary>Jedan redak popisa korisnika (podaci za prikaz, ne za izmjenu).</summary>
public class KorisnikListItem
{
    public required string Id { get; init; }

    public required string Email { get; init; }

    public string? ImePrezime { get; init; }

    public string? Uloga { get; init; }

    /// <summary>Račun je privremeno zaključan zbog previše pogrešnih lozinki.</summary>
    public bool Zakljucan { get; init; }

    /// <summary>Redak je račun prijavljenog administratora (njega ne smije obrisati ni degradirati).</summary>
    public bool JeTrenutni { get; init; }
}

/// <summary>Obrazac za dodavanje i uređivanje korisnika administracije. Pri uređivanju je lozinka neobavezna (prazno = bez promjene).</summary>
public class KorisnikFormViewModel
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "E-mail je obavezan.")]
    [EmailAddress(ErrorMessage = "Unesite ispravnu e-mail adresu.")]
    [StringLength(150, ErrorMessage = "E-mail može imati najviše {1} znakova.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Ime i prezime može imati najviše {1} znakova.")]
    [Display(Name = "Ime i prezime")]
    public string? ImePrezime { get; set; }

    [Required(ErrorMessage = "Odaberite ulogu.")]
    [Display(Name = "Uloga")]
    public string Uloga { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Lozinka")]
    public string? Lozinka { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(Lozinka), ErrorMessage = "Lozinke se ne podudaraju.")]
    // Mora se podudarati s lozinkom ([Compare]); ne sprema se nigdje, služi samo kao zaštita od tipfelera.
    [Display(Name = "Potvrda lozinke")]
    public string? PotvrdaLozinke { get; set; }

    public bool JeTrenutni { get; set; }

    public IEnumerable<SelectListItem>? Uloge { get; set; }
}
