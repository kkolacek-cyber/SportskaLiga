using System.ComponentModel.DataAnnotations;
using SportskaLiga.Domain.Enums;

namespace SportskaLiga.Domain.Models;

/// <summary>
/// Prijava (nove ili postojeće) momčadi za sezonu, poslana s javnog portala bez korisničkog računa.
/// Podnositelj dobiva kod kojim prati status; administrator prijavu odobrava (momčad se upisuje u sezonu) ili odbija.
/// </summary>
public class PrijavaMomcadi
{
    public int Id { get; set; }

    /// <summary>Kratki nasumični kod za praćenje statusa (npr. "K7M2QX9A"); jedinstven u bazi.</summary>
    [StringLength(8)]
    [Display(Name = "Kod prijave")]
    public string Kod { get; set; } = string.Empty;

    [Display(Name = "Sezona")]
    public int SezonaId { get; set; }

    public Sezona? Sezona { get; set; }

    [Required(ErrorMessage = "Naziv momčadi je obavezan.")]
    [StringLength(80, ErrorMessage = "Naziv može imati najviše {1} znakova.")]
    [Display(Name = "Naziv momčadi")]
    public string NazivMomcadi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Grad je obavezan.")]
    [StringLength(60, ErrorMessage = "Grad može imati najviše {1} znakova.")]
    [Display(Name = "Grad")]
    public string Grad { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Igralište može imati najviše {1} znakova.")]
    [Display(Name = "Domaće igralište")]
    public string? Igraliste { get; set; }

    [Required(ErrorMessage = "Kontakt osoba je obavezna.")]
    [StringLength(100, ErrorMessage = "Ime može imati najviše {1} znakova.")]
    [Display(Name = "Kontakt osoba")]
    public string KontaktOsoba { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail je obavezan.")]
    [EmailAddress(ErrorMessage = "E-mail adresa nije ispravna.")]
    [StringLength(100, ErrorMessage = "E-mail može imati najviše {1} znakova.")]
    [Display(Name = "E-mail")]
    public string KontaktEmail { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Broj telefona nije ispravan.")]
    [StringLength(30, ErrorMessage = "Telefon može imati najviše {1} znakova.")]
    [Display(Name = "Telefon")]
    public string? KontaktTelefon { get; set; }

    [Range(7, 40, ErrorMessage = "Broj igrača mora biti između {1} i {2}.")]
    [Display(Name = "Broj igrača")]
    public int BrojIgraca { get; set; }

    [StringLength(500, ErrorMessage = "Napomena može imati najviše {1} znakova.")]
    [Display(Name = "Napomena")]
    public string? Napomena { get; set; }

    [Display(Name = "Status")]
    public StatusPrijave Status { get; set; } = StatusPrijave.NaCekanju;

    /// <summary>Kada je prijava poslana (UTC).</summary>
    [Display(Name = "Podnesena")]
    public DateTime DatumPodnosenja { get; set; }

    /// <summary>Kada ju je administrator odobrio ili odbio (UTC).</summary>
    [Display(Name = "Obrađena")]
    public DateTime? DatumObrade { get; set; }

    /// <summary>Odgovor lige podnositelju (obavezan kod odbijanja).</summary>
    [StringLength(500, ErrorMessage = "Odgovor može imati najviše {1} znakova.")]
    [Display(Name = "Odgovor lige")]
    public string? OdgovorLige { get; set; }

    /// <summary>Momčad upisana u sezonu nakon odobrenja (nova ili postojeća istog naziva).</summary>
    public int? MomcadId { get; set; }

    public Momcad? Momcad { get; set; }
}
