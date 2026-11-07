using System.ComponentModel.DataAnnotations;

namespace SportskaLiga.Domain.Enums;

/// <summary>
/// Stanje utakmice: Zakazana → Odigrana (kad delegat upiše rezultat). Odgođena utakmica čeka novi termin.
/// U bazi se sprema kao tekst ("Odigrana").
/// </summary>
public enum StatusUtakmice
{
    [Display(Name = "Zakazana")]
    Zakazana,

    [Display(Name = "Odigrana")]
    Odigrana,

    [Display(Name = "Odgođena")]
    Odgodena
}
