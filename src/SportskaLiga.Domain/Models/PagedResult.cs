namespace SportskaLiga.Domain.Models;

/// <summary>Jedna stranica rezultata pretrage s podacima potrebnim za straničenje na stranici.</summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];

    public int UkupnoStavki { get; init; }

    public int Stranica { get; init; } = 1;

    public int VelicinaStranice { get; init; }

    // Izračunato iz ukupnog broja stavki i veličine stranice (zaokruženo prema gore).
    public int UkupnoStranica => VelicinaStranice <= 0 ? 0 : (int)Math.Ceiling(UkupnoStavki / (double)VelicinaStranice);

    public bool ImaPrethodnu => Stranica > 1;

    public bool ImaSljedecu => Stranica < UkupnoStranica;

    /// <summary>Preslikava stavke (npr. entitet → DTO) uz zadržavanje podataka o straničenju.</summary>
    public PagedResult<TOut> Map<TOut>(Func<T, TOut> selector) => new()
    {
        Items = Items.Select(selector).ToList(),
        UkupnoStavki = UkupnoStavki,
        Stranica = Stranica,
        VelicinaStranice = VelicinaStranice
    };
}
