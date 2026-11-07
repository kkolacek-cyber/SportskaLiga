using SportskaLiga.Domain.Enums;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Domain.Interfaces;

/// <summary>Utakmice, raspored, rezultati te tablica i strijelci (računaju se iz odigranih utakmica).</summary>
public interface IUtakmicaRepository : IRepository<Utakmica>
{
    /// <summary>Pretraga s filtrima (sezona, kolo, momčad, status, delegat) i straničenjem; uključuje momčadi i suca.</summary>
    Task<PagedResult<Utakmica>> SearchAsync(UtakmicaFilter filter, CancellationToken ct = default);

    /// <summary>Utakmica s momčadima, sucem, sezonom i golovima (samo za čitanje).</summary>
    Task<Utakmica?> GetDetailsAsync(int id, CancellationToken ct = default);

    /// <summary>Brojevi kola koja postoje u sezoni, uzlazno.</summary>
    Task<IReadOnlyList<int>> GetKolaAsync(int sezonaId, CancellationToken ct = default);

    /// <summary>Tablica lige za sezonu (prazna lista ako sezona ne postoji).</summary>
    Task<IReadOnlyList<TablicaRed>> GetTablicaAsync(int sezonaId, CancellationToken ct = default);

    /// <summary>Lista strijelaca sezone bez autogolova; <paramref name="najvise"/> ograničava broj redova.</summary>
    Task<IReadOnlyList<Strijelac>> GetStrijelciAsync(int sezonaId, int? najvise = null, CancellationToken ct = default);

    /// <summary>Broj utakmica sezone po statusu (za nadzornu ploču).</summary>
    Task<IReadOnlyDictionary<StatusUtakmice, int>> BrojPoStatusuAsync(int sezonaId, CancellationToken ct = default);

    /// <summary>
    /// Provjere za ručno dodanu ili izmijenjenu utakmicu: obje momčadi moraju biti prijavljene u sezonu
    /// i biti različite. Vraća grešku s nazivom polja ili uspjeh.
    /// </summary>
    Task<RezultatOperacije> ProvjeriAsync(Utakmica utakmica, CancellationToken ct = default);

    /// <summary>Generira raspored "svatko sa svakim" za sve momčadi prijavljene u sezonu (Vrijednost = broj utakmica).</summary>
    Task<RezultatOperacije> GenerirajRasporedAsync(GenerirajRasporedZahtjev zahtjev, CancellationToken ct = default);

    /// <summary>Briše cijeli raspored sezone – samo ako još nijedna utakmica nije odigrana.</summary>
    Task<RezultatOperacije> ObrisiRasporedAsync(int sezonaId, CancellationToken ct = default);

    /// <summary>
    /// Upisuje rezultat i strijelce (zamjenjuje postojeće). Smije administrator ili delegat dodijeljen utakmici;
    /// utakmica mora biti počela, broj golova po momčadi mora se slagati sa strijelcima.
    /// </summary>
    Task<RezultatOperacije> UnesiRezultatAsync(int utakmicaId, UnosRezultata unos, string? korisnikId, bool jeAdministrator, CancellationToken ct = default);

    /// <summary>Briše rezultat i golove; utakmica se vraća u status Zakazana.</summary>
    Task<RezultatOperacije> PonistiRezultatAsync(int utakmicaId, CancellationToken ct = default);
}
