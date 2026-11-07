using System.ComponentModel.DataAnnotations;

namespace SportskaLiga.Domain.Models;

/// <summary>Momčad (klub). Ima igrače (1:N), igra u više sezona (M:N preko <see cref="SezonaMomcad"/>).</summary>
public class Momcad
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Naziv momčadi je obavezan.")]
    [StringLength(80, ErrorMessage = "Naziv može imati najviše {1} znakova.")]
    // Jedinstven u bazi (unique indeks u SportskaLigaDbContext).
    [Display(Name = "Naziv momčadi")]
    public string Naziv { get; set; } = string.Empty;

    [Required(ErrorMessage = "Grad je obavezan.")]
    [StringLength(60, ErrorMessage = "Grad može imati najviše {1} znakova.")]
    [Display(Name = "Grad")]
    public string Grad { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Igralište može imati najviše {1} znakova.")]
    // Domaće igralište – postaje lokacija utakmica koje momčad igra kao domaćin.
    [Display(Name = "Igralište")]
    public string? Igraliste { get; set; }

    [Range(1850, 2100, ErrorMessage = "Unesite ispravnu godinu.")]
    [Display(Name = "Godina osnutka")]
    public int? GodinaOsnutka { get; set; }

    [EmailAddress(ErrorMessage = "E-mail adresa nije ispravna.")]
    [StringLength(100, ErrorMessage = "E-mail može imati najviše {1} znakova.")]
    [Display(Name = "Kontakt e-mail")]
    public string? KontaktEmail { get; set; }

    // 1:N – igrači ove momčadi. Momčad s igračima se ne može obrisati (Restrict).
    public ICollection<Igrac> Igraci { get; set; } = new List<Igrac>();

    // M:N – sezone u kojima momčad sudjeluje.
    public ICollection<SezonaMomcad> Sezone { get; set; } = new List<SezonaMomcad>();

    // Utakmica ima dvije veze prema momčadi (domaćin i gost), pa momčad ima dvije kolekcije.
    public ICollection<Utakmica> DomaceUtakmice { get; set; } = new List<Utakmica>();

    public ICollection<Utakmica> GostujuceUtakmice { get; set; } = new List<Utakmica>();
}
