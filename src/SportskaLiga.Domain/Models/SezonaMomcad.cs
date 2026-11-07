namespace SportskaLiga.Domain.Models;

/// <summary>
/// Spojna tablica M:N veze Sezona ↔ Momcad: momčad prijavljena u sezonu.
/// Složeni primarni ključ (SezonaId, MomcadId) definiran je u DbContextu.
/// </summary>
public class SezonaMomcad
{
    public int SezonaId { get; set; }

    public Sezona? Sezona { get; set; }

    public int MomcadId { get; set; }

    public Momcad? Momcad { get; set; }

    /// <summary>Kada je momčad prijavljena u sezonu.</summary>
    public DateOnly DatumPrijave { get; set; }
}
