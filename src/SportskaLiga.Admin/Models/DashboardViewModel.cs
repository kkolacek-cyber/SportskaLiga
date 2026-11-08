using SportskaLiga.Domain.Models;

namespace SportskaLiga.Admin.Models;

/// <summary>Brojevi, tablica i kratki popisi za nadzornu ploču aktivne sezone.</summary>
public class DashboardViewModel
{
    public Sezona? Sezona { get; init; }

    public int Odigrano { get; init; }

    public int Zakazano { get; init; }

    public int Odgodeno { get; init; }

    /// <summary>Utakmice kojima je termin prošao, a rezultat nije upisan (za delegata: samo njegove).</summary>
    public int CekajuRezultat { get; init; }

    public int UkupnoGolova { get; init; }

    public IReadOnlyList<TablicaRed> Tablica { get; init; } = [];

    public IReadOnlyList<Strijelac> Strijelci { get; init; } = [];

    /// <summary>Utakmice koje čekaju rezultat (za delegata: njegove), najstarije prve.</summary>
    public IReadOnlyList<Utakmica> ZaUnos { get; init; } = [];

    /// <summary>Sljedeće zakazane utakmice.</summary>
    public IReadOnlyList<Utakmica> Sljedece { get; init; } = [];

    public bool JeDelegat { get; init; }
}
