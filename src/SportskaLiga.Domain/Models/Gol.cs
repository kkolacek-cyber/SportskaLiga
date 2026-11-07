using System.ComponentModel.DataAnnotations;

namespace SportskaLiga.Domain.Models;

/// <summary>
/// Jedan gol na utakmici: strijelac, minuta i momčad kojoj se gol pribraja.
/// <see cref="MomcadId"/> se sprema zasebno (ne čita se iz igrača) jer:
/// (1) kod autogola gol se pribraja protivniku, (2) igrač kasnije može prijeći u drugi klub, a povijest mora ostati točna.
/// </summary>
public class Gol
{
    public int Id { get; set; }

    public int UtakmicaId { get; set; }

    public Utakmica? Utakmica { get; set; }

    [Display(Name = "Strijelac")]
    public int IgracId { get; set; }

    public Igrac? Igrac { get; set; }

    /// <summary>Momčad kojoj se gol pribraja.</summary>
    public int MomcadId { get; set; }

    public Momcad? Momcad { get; set; }

    [Range(1, 120, ErrorMessage = "Minuta mora biti između {1} i {2}.")]
    [Display(Name = "Minuta")]
    public int Minuta { get; set; }

    /// <summary>Autogol se ne broji strijelcu na listi strijelaca.</summary>
    [Display(Name = "Autogol")]
    public bool Autogol { get; set; }
}
