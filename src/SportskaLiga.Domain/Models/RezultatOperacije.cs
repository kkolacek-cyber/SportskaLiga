using SportskaLiga.Domain.Enums;

namespace SportskaLiga.Domain.Models;

/// <summary>
/// Ishod operacije lige (generiranje rasporeda, unos rezultata, prijava momčadi...): uspjeh ili razlog neuspjeha.
/// Repozitorij ne baca iznimku za "očekivane" greške nego vraća ovaj objekt, a pozivatelj (Admin, Api) odluči
/// kako ga prikazati: poruka uz polje obrasca ili HTTP statusni kod.
/// </summary>
public class RezultatOperacije
{
    public PogreskaOperacije Pogreska { get; init; }

    /// <summary>Poruka za korisnika (na hrvatskom).</summary>
    public string? Poruka { get; init; }

    /// <summary>Naziv polja obrasca uz koje ide poruka (npr. "Golovi"); null = opća poruka.</summary>
    public string? Polje { get; init; }

    /// <summary>Dodatna vrijednost pri uspjehu, npr. broj stvorenih utakmica.</summary>
    public int? Vrijednost { get; init; }

    public bool Uspjeh => Pogreska == PogreskaOperacije.None;

    // Tvorničke metode da pozivatelji ne moraju slagati objekt ručno.
    public static RezultatOperacije Ok(int? vrijednost = null) => new() { Vrijednost = vrijednost };

    public static RezultatOperacije Greska(PogreskaOperacije pogreska, string poruka, string? polje = null)
        => new() { Pogreska = pogreska, Poruka = poruka, Polje = polje };
}
