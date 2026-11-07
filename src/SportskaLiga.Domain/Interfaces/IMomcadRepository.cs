using SportskaLiga.Domain.Models;

namespace SportskaLiga.Domain.Interfaces;

/// <summary>Momčadi: pretraga, detalji s igračima i provjera jedinstvenosti naziva.</summary>
public interface IMomcadRepository : IRepository<Momcad>
{
    /// <summary>Pretraga po nazivu i gradu, uz straničenje; uključuje igrače (za broj igrača u popisu).</summary>
    Task<PagedResult<Momcad>> SearchAsync(string? tekst, int stranica, int velicinaStranice, CancellationToken ct = default);

    /// <summary>Sve momčadi abecedno (za padajuće izbornike).</summary>
    Task<IReadOnlyList<Momcad>> GetSveAsync(CancellationToken ct = default);

    /// <summary>Momčad s igračima i sezonama (samo za čitanje).</summary>
    Task<Momcad?> GetDetailsAsync(int id, CancellationToken ct = default);

    /// <summary>Je li naziv zauzet (bez razlike velikih i malih slova); <c>ignorirajId</c> izuzima momčad koja se uređuje.</summary>
    Task<bool> NazivPostojiAsync(string naziv, int? ignorirajId = null, CancellationToken ct = default);
}
