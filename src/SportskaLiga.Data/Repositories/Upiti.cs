namespace SportskaLiga.Data.Repositories;

/// <summary>Pomoćne metode koje dijele repozitoriji.</summary>
internal static class Upiti
{
    /// <summary>Stranica najmanje 1, veličina stranice između 1 i 100 (zaštita od ?velicina=1000000).</summary>
    public static (int Stranica, int Velicina) NormalizirajStranicenje(int stranica, int velicina)
        => (Math.Max(stranica, 1), Math.Clamp(velicina, 1, 100));

    /// <summary>Escapira znakove sa značenjem u LIKE/ILIKE uzorku (\ % _), da ih korisnik ne može zloupotrijebiti.</summary>
    public static string EscapeLike(string tekst)
        => tekst.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    /// <summary>Uzorak "%tekst%" za ILIKE pretragu.</summary>
    public static string Uzorak(string tekst) => $"%{EscapeLike(tekst.Trim())}%";
}
