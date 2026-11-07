using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SportskaLiga.Domain.Enums;

/// <summary>Pomoćne metode za prikaz enumeracija korisniku.</summary>
public static class EnumExtensions
{
    /// <summary>Naziv iz <c>[Display(Name = ...)]</c> (npr. "Odgođena"); ako ga nema, ime vrijednosti.</summary>
    public static string NazivZaPrikaz(this Enum vrijednost)
    {
        var clan = vrijednost.GetType().GetField(vrijednost.ToString());
        return clan?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? vrijednost.ToString();
    }
}
