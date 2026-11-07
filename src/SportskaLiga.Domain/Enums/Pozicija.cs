using System.ComponentModel.DataAnnotations;

namespace SportskaLiga.Domain.Enums;

/// <summary>Pozicija igrača u momčadi.</summary>
public enum Pozicija
{
    [Display(Name = "Vratar")]
    Vratar,

    [Display(Name = "Obrana")]
    Obrana,

    [Display(Name = "Vezni red")]
    Vezni,

    [Display(Name = "Napad")]
    Napad
}
