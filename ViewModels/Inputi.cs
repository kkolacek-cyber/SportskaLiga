using System.ComponentModel.DataAnnotations;

namespace SportskaLiga.ViewModels;

/// <summary>
/// Obrazac prijave momčadi u ligu. Zaseban ulazni model (a ne entitet PrijavaMomcadi) štiti od overpostinga:
/// podnositelj ne može sam postaviti status, kod ili odgovor lige.
/// </summary>
public class PrijavaInput
{
    [Range(1, int.MaxValue, ErrorMessage = "Odaberite sezonu.")]
    [Display(Name = "Sezona")]
    public int SezonaId { get; set; }

    [Required(ErrorMessage = "Naziv momčadi je obavezan.")]
    [StringLength(80, MinimumLength = 3, ErrorMessage = "Naziv mora imati između {2} i {1} znakova.")]
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
    [Display(Name = "Kontakt osoba (ime i prezime)")]
    public string KontaktOsoba { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail je obavezan.")]
    [EmailAddress(ErrorMessage = "Unesite ispravnu e-mail adresu.")]
    [StringLength(100, ErrorMessage = "E-mail može imati najviše {1} znakova.")]
    [Display(Name = "E-mail")]
    public string KontaktEmail { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Broj telefona nije ispravan.")]
    [StringLength(30, ErrorMessage = "Telefon može imati najviše {1} znakova.")]
    [Display(Name = "Telefon")]
    public string? KontaktTelefon { get; set; }

    [Range(7, 40, ErrorMessage = "Broj igrača mora biti između {1} i {2}.")]
    [Display(Name = "Broj igrača")]
    public int BrojIgraca { get; set; } = 15;

    [StringLength(500, ErrorMessage = "Napomena može imati najviše {1} znakova.")]
    [Display(Name = "Napomena")]
    public string? Napomena { get; set; }

    // Provjerava se u PageModelu (OnPostAsync): [Required] ne pomaže jer je i false vrijednost,
    // a [Range] za bool na klijentu ne radi (jQuery Validation ga uspoređuje kao broj).
    [Display(Name = "Prihvaćam pravila lige i obvezu redovitog dolaska na utakmice")]
    public bool PrihvacamPravila { get; set; }
}

/// <summary>Obrazac "Provjera statusa": prijava se pronalazi po kodu, a e-mail potvrđuje da je prijava vaša.</summary>
public class StatusInput
{
    [Required(ErrorMessage = "Unesite kod prijave.")]
    [StringLength(12, ErrorMessage = "Kod prijave može imati najviše {1} znakova.")]
    [Display(Name = "Kod prijave")]
    public string Kod { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail je obavezan.")]
    [EmailAddress(ErrorMessage = "Unesite ispravnu e-mail adresu.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;
}
