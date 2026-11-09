using System.ComponentModel.DataAnnotations;

namespace SportskaLiga.Domain.Enums;

/// <summary>Stanje prijave momčadi u ligu: NaCekanju → Odobrena (momčad je upisana u sezonu) ili Odbijena.</summary>
public enum StatusPrijave
{
    [Display(Name = "Na čekanju")]
    NaCekanju,

    [Display(Name = "Odobrena")]
    Odobrena,

    [Display(Name = "Odbijena")]
    Odbijena
}
