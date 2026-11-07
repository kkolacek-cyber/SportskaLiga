using Microsoft.EntityFrameworkCore;
using SportskaLiga.Domain.Interfaces;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Data.Repositories;

/// <summary>Igrači: pretraga s filtrima i pravilo jedinstvenog broja dresa unutar momčadi.</summary>
public class IgracRepository : Repository<Igrac>, IIgracRepository
{
    public IgracRepository(SportskaLigaDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<Igrac>> SearchAsync(IgracFilter filter, CancellationToken ct = default)
    {
        // Upit se gradi korak po korak: svaki filter se dodaje samo ako je zadan, a baza ih izvršava u jednom SQL upitu.
        var query = Context.Igraci.AsNoTracking();

        if (filter.MomcadId is { } momcadId)
        {
            query = query.Where(i => i.MomcadId == momcadId);
        }

        if (filter.Pozicija is { } pozicija)
        {
            query = query.Where(i => i.Pozicija == pozicija);
        }

        if (!string.IsNullOrWhiteSpace(filter.Tekst))
        {
            var uzorak = Upiti.Uzorak(filter.Tekst);
            query = query.Where(i => EF.Functions.ILike(i.Ime, uzorak) || EF.Functions.ILike(i.Prezime, uzorak));
        }

        var ukupno = await query.CountAsync(ct);
        var (stranica, velicina) = Upiti.NormalizirajStranicenje(filter.Stranica, filter.VelicinaStranice);

        var items = await query
            .Include(i => i.Momcad)
            .OrderBy(i => i.Prezime).ThenBy(i => i.Ime)
            .Skip((stranica - 1) * velicina)
            .Take(velicina)
            .ToListAsync(ct);

        return new PagedResult<Igrac>
        {
            Items = items,
            UkupnoStavki = ukupno,
            Stranica = stranica,
            VelicinaStranice = velicina
        };
    }

    public async Task<Igrac?> GetDetailsAsync(int id, CancellationToken ct = default)
        => await Context.Igraci.AsNoTracking()
            .Include(i => i.Momcad)
            .Include(i => i.Golovi.Where(g => !g.Autogol)).ThenInclude(g => g.Utakmica)
            .AsSplitQuery()
            .FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<IReadOnlyList<Igrac>> GetByMomcadimaAsync(IEnumerable<int> momcadIds, CancellationToken ct = default)
    {
        var ids = momcadIds.Distinct().ToList();

        return await Context.Igraci.AsNoTracking()
            .Where(i => ids.Contains(i.MomcadId))   // SQL: WHERE "MomcadId" = ANY (...)
            .OrderBy(i => i.MomcadId).ThenBy(i => i.BrojDresa)
            .ToListAsync(ct);
    }

    public async Task<bool> BrojDresaZauzetAsync(int momcadId, int brojDresa, int? ignorirajId = null, CancellationToken ct = default)
        => await Context.Igraci.AsNoTracking()
            .AnyAsync(i => i.MomcadId == momcadId && i.BrojDresa == brojDresa
                && (ignorirajId == null || i.Id != ignorirajId), ct);
}
