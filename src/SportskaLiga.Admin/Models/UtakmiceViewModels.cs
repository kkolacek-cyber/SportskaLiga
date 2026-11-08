using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SportskaLiga.Domain.Enums;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Admin.Models;

/// <summary>Okvir stranice s utakmicama: početne vrijednosti filtera. Samu tablicu učitava utakmice.js (Fetch API).</summary>
public class UtakmiceIndexViewModel
{
    public int? SezonaId { get; init; }

    public int? Kolo { get; init; }

    public StatusUtakmice? Status { get; init; }

    public bool SamoMoje { get; init; }

    public bool SamoCekajuRezultat { get; init; }

    public IEnumerable<SelectListItem> Sezone { get; init; } = [];

    public IEnumerable<SelectListItem> Kola { get; init; } = [];
}

/// <summary>HTML fragment s tablicom utakmica (partial _Lista).</summary>
public class UtakmiceListaViewModel
{
    public PagedResult<Utakmica> Rezultat { get; init; } = new();

    /// <summary>Imena delegata po Id-u (Identity korisnici nisu u domeni, pa ih kontroler dohvaća zasebno).</summary>
    public IReadOnlyDictionary<string, string> Delegati { get; init; } = new Dictionary<string, string>();

    /// <summary>Id prijavljenog korisnika – da se uz "njegove" utakmice prikaže gumb za upis rezultata.</summary>
    public string? KorisnikId { get; init; }

    public bool JeAdministrator { get; init; }
}

/// <summary>Detalji utakmice s golovima.</summary>
public class UtakmicaDetailsViewModel
{
    public required Utakmica Utakmica { get; init; }

    public string? Delegat { get; init; }

    /// <summary>Smije li prijavljeni korisnik upisati rezultat (administrator ili dodijeljeni delegat).</summary>
    public bool SmijeUpisati { get; init; }
}

/// <summary>Obrazac za ručno dodavanje i uređivanje utakmice (termin, igralište, sudac, delegat, status).</summary>
public class UtakmicaFormViewModel
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Odaberite sezonu.")]
    [Display(Name = "Sezona")]
    public int SezonaId { get; set; }

    [Range(1, 100, ErrorMessage = "Kolo mora biti između {1} i {2}.")]
    [Display(Name = "Kolo")]
    public int Kolo { get; set; } = 1;

    /// <summary>Lokalno (hrvatsko) vrijeme; u bazu se sprema kao UTC.</summary>
    [Display(Name = "Početak")]
    public DateTime Pocetak { get; set; }

    [StringLength(100, ErrorMessage = "Igralište može imati najviše {1} znakova.")]
    [Display(Name = "Igralište")]
    public string? Lokacija { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Odaberite domaćina.")]
    [Display(Name = "Domaćin")]
    public int DomacinId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Odaberite gosta.")]
    [Display(Name = "Gost")]
    public int GostId { get; set; }

    [Display(Name = "Sudac")]
    public int? SudacId { get; set; }

    [Display(Name = "Delegat")]
    public string? DelegatId { get; set; }

    /// <summary>Ručno se bira samo Zakazana ili Odgođena; "Odigrana" postaje upisom rezultata.</summary>
    [Display(Name = "Status")]
    public StatusUtakmice Status { get; set; } = StatusUtakmice.Zakazana;

    [StringLength(500, ErrorMessage = "Napomena može imati najviše {1} znakova.")]
    [Display(Name = "Napomena")]
    public string? Napomena { get; set; }

    /// <summary>Utakmica već ima rezultat: momčadi i status se ne mogu mijenjati (najprije poništiti rezultat).</summary>
    public bool Odigrana { get; set; }

    public IEnumerable<SelectListItem> Sezone { get; set; } = [];

    public IEnumerable<SelectListItem> Momcadi { get; set; } = [];

    public IEnumerable<SelectListItem> Suci { get; set; } = [];

    public IEnumerable<SelectListItem> Delegati { get; set; } = [];

    public IEnumerable<SelectListItem> Statusi { get; set; } = [];
}

/// <summary>Obrazac za upis rezultata i strijelaca.</summary>
public class RezultatViewModel
{
    public Utakmica? Utakmica { get; set; }

    /// <summary>Ulaz koji ide repozitoriju; u obrascu su polja "Unos.GoloviDomacin", "Unos.Golovi[0].IgracId"...</summary>
    public UnosRezultata Unos { get; set; } = new();

    /// <summary>Igrači obje momčadi grupirani po momčadi (optgroup u padajućem izborniku strijelca).</summary>
    public IEnumerable<SelectListItem> Igraci { get; set; } = [];
}
