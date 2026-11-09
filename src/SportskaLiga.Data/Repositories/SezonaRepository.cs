using Microsoft.EntityFrameworkCore;
using SportskaLiga.Domain;
using SportskaLiga.Domain.Enums;
using SportskaLiga.Domain.Interfaces;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Data.Repositories;

/// <summary>Sezone i prijave momčadi u sezonu (M:N preko SezonaMomcad).</summary>
public class SezonaRepository : Repository<Sezona>, ISezonaRepository
{
    public SezonaRepository(SportskaLigaDbContext context) : base(context)
    {
    }

    // Prvo aktivna; ako nijedna nije označena, najnovija po datumu početka.
    public async Task<Sezona?> GetAktivnaAsync(CancellationToken ct = default)
        => await Context.Sezone.AsNoTracking()
            .OrderByDescending(s => s.Aktivna)
            .ThenByDescending(s => s.DatumPocetka)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<Sezona>> GetSveAsync(CancellationToken ct = default)
        => await Context.Sezone.AsNoTracking()
            .OrderByDescending(s => s.DatumPocetka)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Sezona>> GetOtvoreneZaPrijaveAsync(CancellationToken ct = default)
    {
        var danas = Kalendar.Danas;

        return await Context.Sezone.AsNoTracking()
            .Where(s => s.DatumZavrsetka >= danas && !s.Utakmice.Any())
            .OrderBy(s => s.DatumPocetka)
            .ToListAsync(ct);
    }

    public async Task<Sezona?> GetDetailsAsync(int id, CancellationToken ct = default)
        => await Context.Sezone.AsNoTracking()
            .Include(s => s.Prijave).ThenInclude(p => p.Momcad)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    // Preko spojne tablice do momčadi (SQL: JOIN SezoneMomcadi → Momcadi).
    public async Task<IReadOnlyList<Momcad>> GetMomcadiAsync(int sezonaId, CancellationToken ct = default)
        => await Context.SezoneMomcadi.AsNoTracking()
            .Where(sm => sm.SezonaId == sezonaId)
            .Select(sm => sm.Momcad!)
            .OrderBy(m => m.Naziv)
            .ToListAsync(ct);

    // Redoslijed provjera: postoje li sezona i momčad → je li raspored već generiran → je li momčad već prijavljena.
    public async Task<RezultatOperacije> PrijaviMomcadAsync(int sezonaId, int momcadId, CancellationToken ct = default)
    {
        if (!await Context.Sezone.AnyAsync(s => s.Id == sezonaId, ct))
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NijePronadeno, "Sezona ne postoji.");
        }

        if (!await Context.Momcadi.AnyAsync(m => m.Id == momcadId, ct))
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NijePronadeno, "Momčad ne postoji.", "MomcadId");
        }

        // Raspored je napravljen za točno određene momčadi; nova momčad u njemu ne bi imala utakmice.
        if (await Context.Utakmice.AnyAsync(u => u.SezonaId == sezonaId, ct))
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NeispravnoStanje,
                "Raspored ove sezone je već generiran – momčad se više ne može prijaviti.");
        }

        if (await Context.SezoneMomcadi.AnyAsync(sm => sm.SezonaId == sezonaId && sm.MomcadId == momcadId, ct))
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NeispravnoStanje,
                "Momčad je već prijavljena u ovu sezonu.", "MomcadId");
        }

        Context.SezoneMomcadi.Add(new SezonaMomcad
        {
            SezonaId = sezonaId,
            MomcadId = momcadId,
            DatumPrijave = Kalendar.Danas
        });
        await Context.SaveChangesAsync(ct);

        return RezultatOperacije.Ok();
    }

    public async Task<RezultatOperacije> OdjaviMomcadAsync(int sezonaId, int momcadId, CancellationToken ct = default)
    {
        var prijava = await Context.SezoneMomcadi
            .FirstOrDefaultAsync(sm => sm.SezonaId == sezonaId && sm.MomcadId == momcadId, ct);
        if (prijava is null)
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NijePronadeno, "Momčad nije prijavljena u ovu sezonu.");
        }

        var imaUtakmica = await Context.Utakmice.AnyAsync(u =>
            u.SezonaId == sezonaId && (u.DomacinId == momcadId || u.GostId == momcadId), ct);
        if (imaUtakmica)
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NeispravnoStanje,
                "Momčad već ima utakmice u ovoj sezoni – najprije obrišite raspored.");
        }

        Context.SezoneMomcadi.Remove(prijava);
        await Context.SaveChangesAsync(ct);

        return RezultatOperacije.Ok();
    }

    public async Task<RezultatOperacije> PostaviAktivnuAsync(int sezonaId, CancellationToken ct = default)
    {
        if (!await Context.Sezone.AnyAsync(s => s.Id == sezonaId, ct))
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NijePronadeno, "Sezona ne postoji.");
        }

        // Jedna UPDATE naredba za sve sezone: odabrana postaje aktivna, sve ostale neaktivne.
        await Context.Sezone.ExecuteUpdateAsync(s => s.SetProperty(x => x.Aktivna, x => x.Id == sezonaId), ct);

        return RezultatOperacije.Ok();
    }

    public async Task<bool> NazivPostojiAsync(string naziv, int? ignorirajId = null, CancellationToken ct = default)
    {
        var trazeni = naziv.Trim().ToLower();

        return await Context.Sezone.AsNoTracking()
            .AnyAsync(s => s.Naziv.ToLower() == trazeni && (ignorirajId == null || s.Id != ignorirajId), ct);
    }
}
