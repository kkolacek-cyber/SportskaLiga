using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SportskaLiga.Domain.Models;
using SportskaLiga.Domain.Validation;

namespace SportskaLiga.Admin.Models;

/// <summary>
/// Obrazac za dodavanje i uređivanje sezone. Zaseban ViewModel (a ne entitet Sezona) štiti od overpostinga:
/// obrazac može promijeniti samo polja navedena ovdje (npr. ne i "Aktivna" – za to postoji posebna radnja).
/// </summary>
public class SezonaFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Naziv sezone je obavezan.")]
    [StringLength(20, ErrorMessage = "Naziv može imati najviše {1} znakova.")]
    [Display(Name = "Naziv (npr. 2026./2027.)")]
    public string Naziv { get; set; } = string.Empty;

    [Display(Name = "Početak sezone")]
    public DateOnly DatumPocetka { get; set; }

    [NakonDatuma(nameof(DatumPocetka), ErrorMessage = "Kraj sezone ne može biti prije početka.")]
    [Display(Name = "Kraj sezone")]
    public DateOnly DatumZavrsetka { get; set; }

    [Range(1, 10, ErrorMessage = "Bodovi za pobjedu moraju biti između {1} i {2}.")]
    [Display(Name = "Bodovi za pobjedu")]
    public int BodoviZaPobjedu { get; set; } = 3;

    [Range(0, 10, ErrorMessage = "Bodovi za neriješeno moraju biti između {1} i {2}.")]
    [Display(Name = "Bodovi za neriješeno")]
    public int BodoviZaNerijeseno { get; set; } = 1;
}

/// <summary>Detalji sezone: prijavljene momčadi, prijava nove momčadi, generiranje rasporeda.</summary>
public class SezonaDetailsViewModel
{
    public required Sezona Sezona { get; init; }

    public IReadOnlyList<Momcad> Momcadi { get; init; } = [];

    /// <summary>Momčadi koje se još mogu prijaviti (nisu u sezoni).</summary>
    public IEnumerable<SelectListItem> SlobodneMomcadi { get; init; } = [];

    public int BrojUtakmica { get; init; }

    public int BrojOdigranih { get; init; }

    public bool ImaRaspored => BrojUtakmica > 0;

    /// <summary>Obrazac za generiranje rasporeda (prikazuje se dok raspored ne postoji).</summary>
    public GenerirajRasporedZahtjev Raspored { get; init; } = new();
}
