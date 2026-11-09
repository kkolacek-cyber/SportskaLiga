using SportskaLiga.Domain.Models;

namespace SportskaLiga.Domain.Interfaces;

/// <summary>Sezone i prijave momčadi u sezonu (M:N).</summary>
public interface ISezonaRepository : IRepository<Sezona>
{
    /// <summary>Tekuća (aktivna) sezona ili, ako nijedna nije označena, najnovija.</summary>
    Task<Sezona?> GetAktivnaAsync(CancellationToken ct = default);

    /// <summary>Sve sezone, najnovija prva (za padajuće izbornike).</summary>
    Task<IReadOnlyList<Sezona>> GetSveAsync(CancellationToken ct = default);

    /// <summary>Sezone u koje se momčadi još mogu prijaviti: nisu završile i raspored im još nije generiran.</summary>
    Task<IReadOnlyList<Sezona>> GetOtvoreneZaPrijaveAsync(CancellationToken ct = default);

    /// <summary>Sezona s prijavljenim momčadima (samo za čitanje).</summary>
    Task<Sezona?> GetDetailsAsync(int id, CancellationToken ct = default);

    /// <summary>Momčadi prijavljene u sezonu, abecedno.</summary>
    Task<IReadOnlyList<Momcad>> GetMomcadiAsync(int sezonaId, CancellationToken ct = default);

    /// <summary>Prijavljuje momčad u sezonu. Nije moguće nakon što je raspored generiran.</summary>
    Task<RezultatOperacije> PrijaviMomcadAsync(int sezonaId, int momcadId, CancellationToken ct = default);

    /// <summary>Odjavljuje momčad iz sezone. Nije moguće ako momčad u sezoni već ima utakmice.</summary>
    Task<RezultatOperacije> OdjaviMomcadAsync(int sezonaId, int momcadId, CancellationToken ct = default);

    /// <summary>Označava sezonu kao aktivnu, a sve ostale kao neaktivne.</summary>
    Task<RezultatOperacije> PostaviAktivnuAsync(int sezonaId, CancellationToken ct = default);

    /// <summary>Je li naziv sezone zauzet; <c>ignorirajId</c> izuzima sezonu koja se uređuje.</summary>
    Task<bool> NazivPostojiAsync(string naziv, int? ignorirajId = null, CancellationToken ct = default);
}
