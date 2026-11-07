using Microsoft.EntityFrameworkCore;
using SportskaLiga.Domain.Interfaces;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Data.Repositories;

/// <summary>Momčadi: pretraga, detalji s igračima i sezonama, jedinstvenost naziva.</summary>
public class MomcadRepository : Repository<Momcad>, IMomcadRepository
{
    public MomcadRepository(SportskaLigaDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<Momcad>> SearchAsync(string? tekst, int stranica, int velicinaStranice, CancellationToken ct = default)
    {
        var query = Context.Momcadi.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(tekst))
        {
            // ILike = traženje bez razlike velikih i malih slova (PostgreSQL).
            var uzorak = Upiti.Uzorak(tekst);
            query = query.Where(m => EF.Functions.ILike(m.Naziv, uzorak) || EF.Functions.ILike(m.Grad, uzorak));
        }

        // Ukupan broj prije straničenja potreban je za prikaz stranica; zatim se dohvaća samo tražena stranica.
        var ukupno = await query.CountAsync(ct);
        var (str, velicina) = Upiti.NormalizirajStranicenje(stranica, velicinaStranice);

        var items = await query
            .Include(m => m.Igraci)
            .OrderBy(m => m.Naziv)
            .Skip((str - 1) * velicina)
            .Take(velicina)
            .ToListAsync(ct);

        return new PagedResult<Momcad>
        {
            Items = items,
            UkupnoStavki = ukupno,
            Stranica = str,
            VelicinaStranice = velicina
        };
    }

    public async Task<IReadOnlyList<Momcad>> GetSveAsync(CancellationToken ct = default)
        => await Context.Momcadi.AsNoTracking()
            .OrderBy(m => m.Naziv)
            .ToListAsync(ct);

    // AsSplitQuery: dvije kolekcije (igrači, sezone) dohvaćaju se odvojenim upitima da se izbjegne množenje redaka.
    public async Task<Momcad?> GetDetailsAsync(int id, CancellationToken ct = default)
        => await Context.Momcadi.AsNoTracking()
            .Include(m => m.Igraci.OrderBy(i => i.BrojDresa))
            .Include(m => m.Sezone).ThenInclude(sm => sm.Sezona)
            .AsSplitQuery()
            .FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<bool> NazivPostojiAsync(string naziv, int? ignorirajId = null, CancellationToken ct = default)
    {
        var trazeni = naziv.Trim().ToLower();

        return await Context.Momcadi.AsNoTracking()
            .AnyAsync(m => m.Naziv.ToLower() == trazeni && (ignorirajId == null || m.Id != ignorirajId), ct);
    }
}
