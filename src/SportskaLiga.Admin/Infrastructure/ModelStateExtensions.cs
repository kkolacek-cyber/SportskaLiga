using Microsoft.AspNetCore.Mvc.ModelBinding;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Admin.Infrastructure;

/// <summary>Prijenos greške iz repozitorija (<see cref="RezultatOperacije"/>) u ModelState obrasca.</summary>
public static class ModelStateExtensions
{
    /// <summary>
    /// Dodaje poruku neuspjele operacije uz polje obrasca. Repozitorij zna samo naziv polja svog ulaznog modela
    /// (npr. "Golovi[2].IgracId"); ako je taj model u ViewModelu pod svojstvom "Unos", prefiks je "Unos".
    /// Greška bez polja ide u sažetak na vrhu obrasca.
    /// </summary>
    public static void DodajGresku(this ModelStateDictionary modelState, RezultatOperacije rezultat, string? prefiks = null)
    {
        if (rezultat.Uspjeh)
        {
            return;
        }

        var kljuc = rezultat.Polje is null
            ? string.Empty
            : string.IsNullOrEmpty(prefiks) ? rezultat.Polje : $"{prefiks}.{rezultat.Polje}";

        modelState.AddModelError(kljuc, rezultat.Poruka ?? "Operacija nije uspjela.");
    }
}
