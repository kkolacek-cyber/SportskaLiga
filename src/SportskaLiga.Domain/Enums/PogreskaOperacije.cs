namespace SportskaLiga.Domain.Enums;

/// <summary>
/// Razlog zbog kojeg operacija lige (generiranje rasporeda, unos rezultata, prijava momčadi...) nije provedena.
/// Admin ga pretvara u poruku uz polje obrasca, a Api u HTTP statusni kod (vidi komentare).
/// </summary>
public enum PogreskaOperacije
{
    /// <summary>Operacija je uspjela.</summary>
    None,

    /// <summary>Zapis ne postoji. (API: 404)</summary>
    NijePronadeno,

    /// <summary>Poslani podaci nisu ispravni, npr. broj strijelaca ne odgovara rezultatu. (API: 400)</summary>
    NeispravniPodaci,

    /// <summary>Podaci su ispravni, ali stanje sustava to ne dopušta, npr. raspored već postoji. (API: 409)</summary>
    NeispravnoStanje,

    /// <summary>Korisnik nema pravo na ovu operaciju, npr. delegat tuđe utakmice. (API: 403)</summary>
    NemaOvlasti
}
