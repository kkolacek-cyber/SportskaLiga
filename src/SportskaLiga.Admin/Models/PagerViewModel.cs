namespace SportskaLiga.Admin.Models;

/// <summary>Podaci za partial <c>_Pager</c>: trenutna stranica, cilj linkova i filteri koji se čuvaju pri promjeni stranice.</summary>
public class PagerViewModel
{
    public required int Stranica { get; init; }

    public required int UkupnoStranica { get; init; }

    public required string Kontroler { get; init; }

    public string Akcija { get; init; } = "Index";

    /// <summary>Aktivni filteri (bez broja stranice).</summary>
    public IDictionary<string, string?> Parametri { get; init; } = new Dictionary<string, string?>();

    public string NazivParametra { get; init; } = "stranica";

    // Parametri adrese za zadanu stranicu: aktivni filteri plus broj stranice.
    public IDictionary<string, string?> ZaStranicu(int stranica)
        => new Dictionary<string, string?>(Parametri) { [NazivParametra] = stranica.ToString() };

    /// <summary>Brojevi stranica koji se prikazuju: prva, zadnja i dvije sa svake strane trenutne; null = "…".</summary>
    public IEnumerable<int?> Prikazane()
    {
        int? prethodna = null;
        for (var i = 1; i <= UkupnoStranica; i++)
        {
            if (i != 1 && i != UkupnoStranica && Math.Abs(i - Stranica) > 2)
            {
                continue;
            }

            if (prethodna is not null && i - prethodna > 1)
            {
                yield return null;
            }

            yield return i;
            prethodna = i;
        }
    }
}
