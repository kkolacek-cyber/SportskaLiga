using System.ComponentModel.DataAnnotations;
using SportskaLiga.Domain.Enums;

namespace SportskaLiga.Domain.Models;

/// <summary>Igrač jedne momčadi (N:1). Broj dresa je jedinstven unutar momčadi.</summary>
public class Igrac
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
    public DateOnly DatumRodenja { get; set; }

    [Range(1, 99, ErrorMessage = "Broj dresa mora biti između {1} i {2}.")]
    [Display(Name = "Broj dresa")]
    public int BrojDresa { get; set; }

    [Display(Name = "Pozicija")]
    public Pozicija Pozicija { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Odaberite momčad.")]
    [Display(Name = "Momčad")]
    public int MomcadId { get; set; }

    // Navigacije su nullable da MVC ne bi tražio da stignu s forme (implicitni [Required] za non-nullable tipove).
    public Momcad? Momcad { get; set; }

    public ICollection<Gol> Golovi { get; set; } = new List<Gol>();

    // Samo getter – EF ga ne sprema u bazu, služi za prikaz.
    public string PunoIme => $"{Ime} {Prezime}";
}
