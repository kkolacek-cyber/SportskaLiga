using Microsoft.EntityFrameworkCore;
using SportskaLiga.Domain;
using SportskaLiga.Domain.Enums;
using SportskaLiga.Domain.Interfaces;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Data.Repositories;

/// <summary>Prijave momčadi u ligu: podnošenje (portal), praćenje kodom, odobravanje i odbijanje (administracija).</summary>
public class PrijavaRepository : Repository<PrijavaMomcadi>, IPrijavaRepository
{
    public PrijavaRepository(SportskaLigaDbContext context) : base(context)
    {
    }

    // Redoslijed provjera: sezona postoji → prijave su otvorene → momčad tog naziva nije već u sezoni
    // → nema druge otvorene prijave istog naziva. Tek tada se dodjeljuje kod i prijava sprema.
    public async Task<RezultatOperacije> PodnesiAsync(PrijavaMomcadi prijava, CancellationToken ct = default)
    {
        var sezona = await Context.Sezone.AsNoTracking().FirstOrDefaultAsync(s => s.Id == prijava.SezonaId, ct);
        if (sezona is null)
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NijePronadeno, "Odabrana sezona ne postoji.", nameof(prijava.SezonaId));
        }

        if (sezona.DatumZavrsetka < Kalendar.Danas || await Context.Utakmice.AnyAsync(u => u.SezonaId == sezona.Id, ct))
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NeispravnoStanje,
                $"Prijave za sezonu {sezona.Naziv} su zatvorene (raspored je već objavljen).", nameof(prijava.SezonaId));
        }

        var naziv = prijava.NazivMomcadi.Trim();
        var nazivMalo = naziv.ToLower();

        var vecUSezoni = await Context.SezoneMomcadi.AnyAsync(sm =>
            sm.SezonaId == sezona.Id && sm.Momcad!.Naziv.ToLower() == nazivMalo, ct);
        if (vecUSezoni)
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NeispravnoStanje,
                "Momčad tog naziva je već upisana u ovu sezonu.", nameof(prijava.NazivMomcadi));
        }

        var otvorenaPrijava = await Context.PrijaveMomcadi.AnyAsync(p =>
            p.SezonaId == sezona.Id && p.Status == StatusPrijave.NaCekanju && p.NazivMomcadi.ToLower() == nazivMalo, ct);
        if (otvorenaPrijava)
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NeispravnoStanje,
                "Za momčad tog naziva već postoji prijava koja čeka obradu.", nameof(prijava.NazivMomcadi));
        }

        prijava.NazivMomcadi = naziv;
        prijava.Kod = await NoviKodAsync(ct);
        prijava.Status = StatusPrijave.NaCekanju;
        prijava.DatumPodnosenja = DateTime.UtcNow;
        prijava.DatumObrade = null;
        prijava.OdgovorLige = null;
        prijava.MomcadId = null;

        Context.PrijaveMomcadi.Add(prijava);
        await Context.SaveChangesAsync(ct);

        return RezultatOperacije.Ok(prijava.Id);
    }

    public async Task<PrijavaMomcadi?> GetByKodAsync(string kod, CancellationToken ct = default)
    {
        var normaliziran = kod.Trim().ToUpperInvariant();

        return await Context.PrijaveMomcadi.AsNoTracking()
            .Include(p => p.Sezona)
            .FirstOrDefaultAsync(p => p.Kod == normaliziran, ct);
    }

    public async Task<PrijavaMomcadi?> GetDetailsAsync(int id, CancellationToken ct = default)
        => await Context.PrijaveMomcadi.AsNoTracking()
            .Include(p => p.Sezona)
            .Include(p => p.Momcad)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<PagedResult<PrijavaMomcadi>> SearchAsync(StatusPrijave? status, int stranica, int velicinaStranice, CancellationToken ct = default)
    {
        var query = Context.PrijaveMomcadi.AsNoTracking();
        if (status is { } s)
        {
            query = query.Where(p => p.Status == s);
        }

        var ukupno = await query.CountAsync(ct);
        var (str, velicina) = Upiti.NormalizirajStranicenje(stranica, velicinaStranice);

        var items = await query
            .Include(p => p.Sezona)
            .OrderByDescending(p => p.DatumPodnosenja)
            .Skip((str - 1) * velicina)
            .Take(velicina)
            .ToListAsync(ct);

        return new PagedResult<PrijavaMomcadi>
        {
            Items = items,
            UkupnoStavki = ukupno,
            Stranica = str,
            VelicinaStranice = velicina
        };
    }

    public Task<int> BrojNaCekanjuAsync(CancellationToken ct = default)
        => Context.PrijaveMomcadi.CountAsync(p => p.Status == StatusPrijave.NaCekanju, ct);

    public async Task<RezultatOperacije> OdobriAsync(int id, string? odgovor, CancellationToken ct = default)
    {
        var prijava = await Context.PrijaveMomcadi.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (prijava is null)
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NijePronadeno, "Prijava ne postoji.");
        }

        if (prijava.Status != StatusPrijave.NaCekanju)
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NeispravnoStanje, "Prijava je već obrađena.");
        }

        if (await Context.Utakmice.AnyAsync(u => u.SezonaId == prijava.SezonaId, ct))
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NeispravnoStanje,
                "Raspored sezone je već generiran – momčad se više ne može upisati. Prijavu možete odbiti.");
        }

        // Postojeća momčad istog naziva (npr. klub iz prošle sezone) se koristi, inače se registrira nova.
        var nazivMalo = prijava.NazivMomcadi.ToLower();
        var momcad = await Context.Momcadi.FirstOrDefaultAsync(m => m.Naziv.ToLower() == nazivMalo, ct);
        if (momcad is null)
        {
            momcad = new Momcad
            {
                Naziv = prijava.NazivMomcadi,
                Grad = prijava.Grad,
                Igraliste = prijava.Igraliste,
                KontaktEmail = prijava.KontaktEmail
            };
            Context.Momcadi.Add(momcad);
        }
        else if (await Context.SezoneMomcadi.AnyAsync(sm => sm.SezonaId == prijava.SezonaId && sm.MomcadId == momcad.Id, ct))
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NeispravnoStanje, "Momčad je već upisana u ovu sezonu.");
        }

        // Prijava u sezonu (M:N) – preko navigacije, pa EF sam upiše Id nove momčadi.
        Context.SezoneMomcadi.Add(new SezonaMomcad
        {
            SezonaId = prijava.SezonaId,
            Momcad = momcad,
            DatumPrijave = Kalendar.Danas
        });

        prijava.Status = StatusPrijave.Odobrena;
        prijava.Momcad = momcad;
        prijava.DatumObrade = DateTime.UtcNow;
        prijava.OdgovorLige = string.IsNullOrWhiteSpace(odgovor) ? null : odgovor.Trim();

        // Nova momčad, upis u sezonu i promjena prijave – jedan SaveChanges = jedna transakcija.
        await Context.SaveChangesAsync(ct);

        return RezultatOperacije.Ok(momcad.Id);
    }

    public async Task<RezultatOperacije> OdbijAsync(int id, string odgovor, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(odgovor))
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NeispravniPodaci, "Upišite razlog odbijanja.", "Odgovor");
        }

        var prijava = await Context.PrijaveMomcadi.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (prijava is null)
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NijePronadeno, "Prijava ne postoji.");
        }

        if (prijava.Status != StatusPrijave.NaCekanju)
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NeispravnoStanje, "Prijava je već obrađena.");
        }

        prijava.Status = StatusPrijave.Odbijena;
        prijava.DatumObrade = DateTime.UtcNow;
        prijava.OdgovorLige = odgovor.Trim();

        await Context.SaveChangesAsync(ct);
        return RezultatOperacije.Ok();
    }

    // Kod je nasumičan pa se provjerava je li slučajno već zauzet (praktički nemoguće).
    private async Task<string> NoviKodAsync(CancellationToken ct)
    {
        for (var pokusaj = 0; pokusaj < 5; pokusaj++)
        {
            var kod = KodGenerator.Novi();
            if (!await Context.PrijaveMomcadi.AnyAsync(p => p.Kod == kod, ct))
            {
                return kod;
            }
        }

        throw new InvalidOperationException("Nije moguće generirati jedinstven kod prijave.");
    }
}
