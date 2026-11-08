using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SportskaLiga.Domain.Enums;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Admin.Models;

/// <summary>Popis igrača s filtrima i straničenjem.</summary>
public class IgraciIndexViewModel
{
    [Display(Name = "Pretraga")]
    public string? Tekst { get; set; }

    [Display(Name = "Momčad")]
    public int? MomcadId { get; set; }

    [Display(Name = "Pozicija")]
    public Pozicija? Pozicija { get; set; }

    public PagedResult<Igrac> Rezultat { get; set; } = new();

    public IEnumerable<SelectListItem> Momcadi { get; set; } = [];

    public bool ImaFiltera => !string.IsNullOrWhiteSpace(Tekst) || MomcadId is not null || Pozicija is not null;
}

/// <summary>Obrazac za registraciju (dodavanje) i uređivanje igrača.</summary>
public class IgracFormViewModel
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

    [Display(Name = "Datum rođenja")]
    public DateOnly DatumRodenja { get; set; } = new(2000, 1, 1);

    [Range(1, 99, ErrorMessage = "Broj dresa mora biti između {1} i {2}.")]
    [Display(Name = "Broj dresa")]
    public int BrojDresa { get; set; }

    [Display(Name = "Pozicija")]
    public Pozicija Pozicija { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Odaberite momčad.")]
    [Display(Name = "Momčad")]
    public int MomcadId { get; set; }

    public IEnumerable<SelectListItem> Momcadi { get; set; } = [];
}
