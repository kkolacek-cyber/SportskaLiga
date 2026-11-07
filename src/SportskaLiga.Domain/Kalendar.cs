namespace SportskaLiga.Domain;

/// <summary>
/// Hrvatsko vrijeme. Kontejneri rade u UTC-u, a vremena utakmica u bazi su u UTC-u (timestamptz),
/// pa se za prikaz i unos pretvaraju u vrijeme Europe/Zagreb i natrag.
/// </summary>
public static class Kalendar
{
    // Zona se traži samo jednom, pri prvom korištenju klase.
    private static readonly TimeZoneInfo Zagreb = Pronadji();

    /// <summary>Današnji datum po hrvatskom vremenu.</summary>
    public static DateOnly Danas => DateOnly.FromDateTime(Sada);

    /// <summary>Trenutno hrvatsko lokalno vrijeme.</summary>
    public static DateTime Sada => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Zagreb);

    /// <summary>UTC vrijeme iz baze → hrvatsko lokalno vrijeme za prikaz.</summary>
    public static DateTime ULokalno(DateTime utc)
        => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Zagreb);

    /// <summary>Hrvatsko lokalno vrijeme (npr. iz obrasca) → UTC za spremanje u bazu.</summary>
    public static DateTime UUtc(DateTime lokalno)
        => TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(lokalno, DateTimeKind.Unspecified), Zagreb);

    // Ako sustav nema bazu vremenskih zona (npr. Alpine slika bez paketa tzdata), vraća se UTC umjesto pada aplikacije.
    private static TimeZoneInfo Pronadji()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Zagreb");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
    }
}
