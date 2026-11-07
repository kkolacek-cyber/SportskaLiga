using System.ComponentModel.DataAnnotations;
using SportskaLiga.Domain.Validation;

namespace SportskaLiga.Domain.Models;

/// <summary>
/// Jedna sezona lige (npr. "2026./2027."). Momčadi se u sezonu prijavljuju preko <see cref="SezonaMomcad"/> (M:N),
/// a sezona ima svoje utakmice (1:N). Bodovanje je dio sezone, pa se pravila mogu mijenjati iz sezone u sezonu.
/// </summary>
public class Sezona
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Naziv sezone je obavezan.")]
    [StringLength(20, ErrorMessage = "Naziv može imati najviše {1} znakova.")]
    [Display(Name = "Sezona")]
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

    /// <summary>Tekuća sezona – nju javni portal i API prikazuju kad sezona nije zadana. Aktivna je najviše jedna.</summary>
    [Display(Name = "Aktivna")]
    public bool Aktivna { get; set; }

    // Momčadi prijavljene u sezonu (spojna tablica M:N).
    public ICollection<SezonaMomcad> Prijave { get; set; } = new List<SezonaMomcad>();

    public ICollection<Utakmica> Utakmice { get; set; } = new List<Utakmica>();
}
