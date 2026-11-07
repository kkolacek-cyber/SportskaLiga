using SportskaLiga.Domain.Models;

namespace SportskaLiga.Domain.Interfaces;

/// <summary>Igrači: pretraga s filtrima, igrači momčadi i pravilo jedinstvenog broja dresa.</summary>
public interface IIgracRepository : IRepository<Igrac>
{
    Task<PagedResult<Igrac>> SearchAsync(IgracFilter filter, CancellationToken ct = default);

    /// <summary>Igrač s momčadi i golovima (samo za čitanje).</summary>
    Task<Igrac?> GetDetailsAsync(int id, CancellationToken ct = default);

    /// <summary>Igrači zadanih momčadi (npr. domaćina i gosta – za odabir strijelaca), po momčadi i broju dresa.</summary>
    Task<IReadOnlyList<Igrac>> GetByMomcadimaAsync(IEnumerable<int> momcadIds, CancellationToken ct = default);

    /// <summary>Nosi li već neki igrač te momčadi taj broj dresa; <c>ignorirajId</c> izuzima igrača koji se uređuje.</summary>
    Task<bool> BrojDresaZauzetAsync(int momcadId, int brojDresa, int? ignorirajId = null, CancellationToken ct = default);
}
