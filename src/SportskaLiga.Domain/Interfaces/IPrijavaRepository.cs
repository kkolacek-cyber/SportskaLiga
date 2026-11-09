using SportskaLiga.Domain.Enums;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Domain.Interfaces;

/// <summary>Prijave momčadi u ligu: podnošenje s portala, praćenje kodom, obrada u administraciji.</summary>
public interface IPrijavaRepository : IRepository<PrijavaMomcadi>
{
    /// <summary>
    /// Sprema novu prijavu: dodjeljuje kod, status "Na čekanju" i vrijeme podnošenja (kod je nakon uspjeha u <c>prijava.Kod</c>).
    /// Prijave za sezonu su zatvorene kad je njezin raspored generiran ili je sezona završila.
    /// </summary>
    Task<RezultatOperacije> PodnesiAsync(PrijavaMomcadi prijava, CancellationToken ct = default);

    /// <summary>Prijava po kodu (bez obzira na velika i mala slova), sa sezonom; null ako ne postoji.</summary>
    Task<PrijavaMomcadi?> GetByKodAsync(string kod, CancellationToken ct = default);

    /// <summary>Prijava sa sezonom i momčadi (samo za čitanje).</summary>
    Task<PrijavaMomcadi?> GetDetailsAsync(int id, CancellationToken ct = default);

    /// <summary>Popis prijava za administraciju, najnovije prve.</summary>
    Task<PagedResult<PrijavaMomcadi>> SearchAsync(StatusPrijave? status, int stranica, int velicinaStranice, CancellationToken ct = default);

    Task<int> BrojNaCekanjuAsync(CancellationToken ct = default);

    /// <summary>
    /// Odobrava prijavu: momčad istog naziva se koristi ako postoji, inače se stvara; momčad se upisuje u sezonu.
    /// Sve se sprema jednim SaveChanges (jedna transakcija).
    /// </summary>
    Task<RezultatOperacije> OdobriAsync(int id, string? odgovor, CancellationToken ct = default);

    /// <summary>Odbija prijavu; odgovor (razlog) je obavezan.</summary>
    Task<RezultatOperacije> OdbijAsync(int id, string odgovor, CancellationToken ct = default);
}
