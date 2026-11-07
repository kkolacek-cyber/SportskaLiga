using Microsoft.EntityFrameworkCore;
using SportskaLiga.Domain;
using SportskaLiga.Domain.Enums;
using SportskaLiga.Domain.Interfaces;
using SportskaLiga.Domain.Logika;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Data.Repositories;

/// <summary>
/// Utakmice i sve što iz njih proizlazi: raspored, rezultati, tablica i strijelci.
/// Operacije koje mijenjaju stanje vraćaju <see cref="RezultatOperacije"/> umjesto da bacaju iznimku.
/// </summary>
public class UtakmicaRepository : Repository<Utakmica>, IUtakmicaRepository
{
    public UtakmicaRepository(SportskaLigaDbContext context) : base(context)
    {
    }

    // ------------------------------------------------------------------ čitanje

    public async Task<PagedResult<Utakmica>> SearchAsync(UtakmicaFilter filter, CancellationToken ct = default)
    {
        var query = Context.Utakmice.AsNoTracking();

        if (filter.SezonaId is { } sezonaId)
        {
            query = query.Where(u => u.SezonaId == sezonaId);
        }

        if (filter.Kolo is { } kolo)
        {
            query = query.Where(u => u.Kolo == kolo);
        }

        if (filter.MomcadId is { } momcadId)
        {
            query = query.Where(u => u.DomacinId == momcadId || u.GostId == momcadId);
        }

        if (filter.Status is { } status)
        {
            query = query.Where(u => u.Status == status);
        }

        if (!string.IsNullOrEmpty(filter.DelegatId))
        {
            query = query.Where(u => u.DelegatId == filter.DelegatId);
        }

        var ukupno = await query.CountAsync(ct);
        var (stranica, velicina) = Upiti.NormalizirajStranicenje(filter.Stranica, filter.VelicinaStranice);

        // Raspored se čita kronološki, a rezultati od najnovijih.
        query = filter.NajnovijePrvo
            ? query.OrderByDescending(u => u.Pocetak).ThenBy(u => u.Id)
            : query.OrderBy(u => u.Pocetak).ThenBy(u => u.Id);

        var items = await query
            .Include(u => u.Domacin)
            .Include(u => u.Gost)
            .Include(u => u.Sudac)
            .Skip((stranica - 1) * velicina)
            .Take(velicina)
            .ToListAsync(ct);

        return new PagedResult<Utakmica>
        {
            Items = items,
            UkupnoStavki = ukupno,
            Stranica = stranica,
            VelicinaStranice = velicina
        };
    }

    public async Task<Utakmica?> GetDetailsAsync(int id, CancellationToken ct = default)
        => await Context.Utakmice.AsNoTracking()
            .Include(u => u.Sezona)
            .Include(u => u.Domacin)
            .Include(u => u.Gost)
            .Include(u => u.Sudac)
            .Include(u => u.Golovi.OrderBy(g => g.Minuta)).ThenInclude(g => g.Igrac)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<IReadOnlyList<int>> GetKolaAsync(int sezonaId, CancellationToken ct = default)
        => await Context.Utakmice.AsNoTracking()
            .Where(u => u.SezonaId == sezonaId)
            .Select(u => u.Kolo)
            .Distinct()
            .OrderBy(k => k)
            .ToListAsync(ct);

    // Tablica se ne sprema u bazu: računa se iz odigranih utakmica pri svakom zahtjevu (uvijek točna).
    public async Task<IReadOnlyList<TablicaRed>> GetTablicaAsync(int sezonaId, CancellationToken ct = default)
    {
        var sezona = await Context.Sezone.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sezonaId, ct);
        if (sezona is null)
        {
            return [];
        }

        var momcadi = await Context.SezoneMomcadi.AsNoTracking()
            .Where(sm => sm.SezonaId == sezonaId)
            .Select(sm => sm.Momcad!)
            .ToListAsync(ct);

        var odigrane = await Context.Utakmice.AsNoTracking()
            .Where(u => u.SezonaId == sezonaId && u.Status == StatusUtakmice.Odigrana)
            .ToListAsync(ct);

        // Sam izračun (bodovi, poredak, forma) je u domeni – Logika/IzracunTablice.
        return IzracunTablice.Izracunaj(momcadi, odigrane, sezona.BodoviZaPobjedu, sezona.BodoviZaNerijeseno);
    }

    public async Task<IReadOnlyList<Strijelac>> GetStrijelciAsync(int sezonaId, int? najvise = null, CancellationToken ct = default)
    {
        // GROUP BY se izvršava u bazi: po igraču i momčadi za koju je zabio prebrojavaju se golovi (bez autogolova).
        var query = Context.Golovi.AsNoTracking()
            .Where(g => !g.Autogol
                && g.Utakmica!.SezonaId == sezonaId
                && g.Utakmica.Status == StatusUtakmice.Odigrana)
            .GroupBy(g => new { g.IgracId, g.Igrac!.Ime, g.Igrac.Prezime, g.MomcadId, Momcad = g.Momcad!.Naziv })
            .Select(grupa => new
            {
                grupa.Key.IgracId,
                grupa.Key.Ime,
                grupa.Key.Prezime,
                grupa.Key.MomcadId,
                grupa.Key.Momcad,
                Golovi = grupa.Count()
            })
            .OrderByDescending(x => x.Golovi)
            .ThenBy(x => x.Prezime)
            .ThenBy(x => x.Ime);

        var redovi = najvise is > 0
            ? await query.Take(najvise.Value).ToListAsync(ct)
            : await query.ToListAsync(ct);

        // Igrači s istim brojem golova dijele poziciju: 1., 2., 2., 4.
        var lista = new List<Strijelac>(redovi.Count);
        for (var i = 0; i < redovi.Count; i++)
        {
            var r = redovi[i];
            var pozicija = i > 0 && redovi[i - 1].Golovi == r.Golovi ? lista[i - 1].Pozicija : i + 1;
            lista.Add(new Strijelac(pozicija, r.IgracId, $"{r.Ime} {r.Prezime}", r.MomcadId, r.Momcad, r.Golovi));
        }

        return lista;
    }

    public async Task<IReadOnlyDictionary<StatusUtakmice, int>> BrojPoStatusuAsync(int sezonaId, CancellationToken ct = default)
        => await Context.Utakmice.AsNoTracking()
            .Where(u => u.SezonaId == sezonaId)
            .GroupBy(u => u.Status)
            .Select(g => new { Status = g.Key, Broj = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Broj, ct);

    // ------------------------------------------------------------------ pravila

    public async Task<RezultatOperacije> ProvjeriAsync(Utakmica utakmica, CancellationToken ct = default)
    {
        if (utakmica.DomacinId == utakmica.GostId)
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NeispravniPodaci,
                "Domaćin i gost ne mogu biti ista momčad.", nameof(Utakmica.GostId));
        }

        var prijavljene = await Context.SezoneMomcadi.AsNoTracking()
            .Where(sm => sm.SezonaId == utakmica.SezonaId
                && (sm.MomcadId == utakmica.DomacinId || sm.MomcadId == utakmica.GostId))
            .Select(sm => sm.MomcadId)
            .ToListAsync(ct);

        if (!prijavljene.Contains(utakmica.DomacinId))
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NeispravniPodaci,
                "Domaćin nije prijavljen u ovu sezonu.", nameof(Utakmica.DomacinId));
        }

        if (!prijavljene.Contains(utakmica.GostId))
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NeispravniPodaci,
                "Gost nije prijavljen u ovu sezonu.", nameof(Utakmica.GostId));
        }

        return RezultatOperacije.Ok();
    }

    // Redoslijed provjera: sezona postoji → prvi termin unutar sezone → raspored još ne postoji
    // → barem dvije momčadi → zadnje kolo stane u sezonu. Tek tada se utakmice spremaju (jedan SaveChanges).
    public async Task<RezultatOperacije> GenerirajRasporedAsync(GenerirajRasporedZahtjev zahtjev, CancellationToken ct = default)
    {
        var sezona = await Context.Sezone.AsNoTracking().FirstOrDefaultAsync(s => s.Id == zahtjev.SezonaId, ct);
        if (sezona is null)
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NijePronadeno, "Sezona ne postoji.", nameof(zahtjev.SezonaId));
        }

        var prviDan = DateOnly.FromDateTime(zahtjev.PrviTermin);
        if (prviDan < sezona.DatumPocetka || prviDan > sezona.DatumZavrsetka)
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NeispravniPodaci,
                $"Prvo kolo mora biti unutar sezone ({sezona.DatumPocetka:dd.MM.yyyy.} – {sezona.DatumZavrsetka:dd.MM.yyyy.}).",
                nameof(zahtjev.PrviTermin));
        }

        if (await Context.Utakmice.AnyAsync(u => u.SezonaId == sezona.Id, ct))
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NeispravnoStanje,
                "Raspored ove sezone već postoji. Ako ga želite ponovno generirati, najprije ga obrišite.");
        }

        var momcadi = await Context.SezoneMomcadi.AsNoTracking()
            .Where(sm => sm.SezonaId == sezona.Id)
            .Select(sm => sm.Momcad!)
            .OrderBy(m => m.Naziv)
            .ToListAsync(ct);

        if (momcadi.Count < 2)
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NeispravnoStanje,
                "Za raspored su potrebne barem dvije momčadi prijavljene u sezonu.");
        }

        // Sam algoritam je u domeni (Logika/BergerovRaspored); ovdje parove pretvaramo u utakmice.
        var parovi = BergerovRaspored.Generiraj(momcadi.Select(m => m.Id).ToList(), zahtjev.DvostrukiKrug);
        var poId = momcadi.ToDictionary(m => m.Id);

        // Sve utakmice jednog kola igraju se istog dana u isto vrijeme, svaka na igralištu domaćina.
        // Datum se računa u lokalnom vremenu pa pretvara u UTC – tako prijelaz na zimsko/ljetno vrijeme ne pomakne sat.
        DateTime Termin(int kolo) => zahtjev.PrviTermin.AddDays((kolo - 1) * zahtjev.RazmakDana);

        var zadnjeKolo = parovi.Max(p => p.Kolo);
        var zadnjiDan = DateOnly.FromDateTime(Termin(zadnjeKolo));
        if (zadnjiDan > sezona.DatumZavrsetka)
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NeispravniPodaci,
                $"Raspored ne stane u sezonu: zadnje ({zadnjeKolo}.) kolo bilo bi {zadnjiDan:dd.MM.yyyy.}. " +
                "Pomaknite prvo kolo ranije ili smanjite razmak između kola.",
                nameof(zahtjev.RazmakDana));
        }

        var utakmice = parovi.Select(p => new Utakmica
        {
            SezonaId = sezona.Id,
            Kolo = p.Kolo,
            Pocetak = Kalendar.UUtc(Termin(p.Kolo)),
            Lokacija = poId[p.DomacinId].Igraliste ?? $"Igralište {poId[p.DomacinId].Grad}",
            DomacinId = p.DomacinId,
            GostId = p.GostId,
            Status = StatusUtakmice.Zakazana
        }).ToList();

        Context.Utakmice.AddRange(utakmice);
        await Context.SaveChangesAsync(ct);

        return RezultatOperacije.Ok(utakmice.Count);
    }

    public async Task<RezultatOperacije> ObrisiRasporedAsync(int sezonaId, CancellationToken ct = default)
    {
        if (await Context.Utakmice.AnyAsync(u => u.SezonaId == sezonaId && u.Status == StatusUtakmice.Odigrana, ct))
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NeispravnoStanje,
                "Raspored se ne može obrisati jer su neke utakmice već odigrane.");
        }

        // Jedna DELETE naredba izravno u bazi, bez učitavanja utakmica u memoriju.
        var obrisano = await Context.Utakmice.Where(u => u.SezonaId == sezonaId).ExecuteDeleteAsync(ct);

        return RezultatOperacije.Ok(obrisano);
    }

    // Redoslijed provjera: utakmica postoji → ovlast → status i vrijeme → broj strijelaca
    // → svaki strijelac igra za jednu od dvije momčadi → golovi po momčadi se slažu s rezultatom.
    public async Task<RezultatOperacije> UnesiRezultatAsync(int utakmicaId, UnosRezultata unos, string? korisnikId, bool jeAdministrator, CancellationToken ct = default)
    {
        // Bez AsNoTracking: utakmicu i njezine golove mijenjamo i spremamo.
        var utakmica = await Context.Utakmice
            .Include(u => u.Golovi)
            .FirstOrDefaultAsync(u => u.Id == utakmicaId, ct);

        if (utakmica is null)
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NijePronadeno, "Utakmica ne postoji.");
        }

        if (!jeAdministrator && (korisnikId is null || utakmica.DelegatId != korisnikId))
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NemaOvlasti,
                "Rezultat može upisati samo delegat dodijeljen ovoj utakmici ili administrator.");
        }

        if (utakmica.Status == StatusUtakmice.Odgodena)
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NeispravnoStanje,
                "Utakmica je odgođena – najprije joj odredite novi termin.");
        }

        if (utakmica.Pocetak > DateTime.UtcNow)
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NeispravnoStanje,
                "Rezultat se ne može upisati prije početka utakmice.");
        }

        var ukupno = unos.GoloviDomacin + unos.GoloviGost;
        if (unos.Golovi.Count != ukupno)
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NeispravniPodaci,
                $"Upisano je {unos.Golovi.Count} strijelaca, a rezultat {unos.GoloviDomacin}:{unos.GoloviGost} traži {ukupno}.",
                nameof(unos.Golovi));
        }

        var igracIds = unos.Golovi.Select(g => g.IgracId).Distinct().ToList();
        var momcadIgraca = await Context.Igraci.AsNoTracking()
            .Where(i => igracIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id, i => i.MomcadId, ct);

        var noviGolovi = new List<Gol>();
        for (var i = 0; i < unos.Golovi.Count; i++)
        {
            var g = unos.Golovi[i];
            var polje = $"{nameof(unos.Golovi)}[{i}].{nameof(UnosGola.IgracId)}";

            if (!momcadIgraca.TryGetValue(g.IgracId, out var momcadId))
            {
                return RezultatOperacije.Greska(PogreskaOperacije.NeispravniPodaci, "Odabrani igrač ne postoji.", polje);
            }

            if (momcadId != utakmica.DomacinId && momcadId != utakmica.GostId)
            {
                return RezultatOperacije.Greska(PogreskaOperacije.NeispravniPodaci,
                    "Igrač ne igra ni za domaćina ni za gosta.", polje);
            }

            // Autogol se pribraja protivniku; inače momčadi strijelca.
            var zasluzna = g.Autogol
                ? (momcadId == utakmica.DomacinId ? utakmica.GostId : utakmica.DomacinId)
                : momcadId;

            noviGolovi.Add(new Gol { IgracId = g.IgracId, MomcadId = zasluzna, Minuta = g.Minuta, Autogol = g.Autogol });
        }

        var zaDomacina = noviGolovi.Count(g => g.MomcadId == utakmica.DomacinId);
        var zaGosta = noviGolovi.Count - zaDomacina;
        if (zaDomacina != unos.GoloviDomacin || zaGosta != unos.GoloviGost)
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NeispravniPodaci,
                $"Strijelci se ne slažu s rezultatom: domaćinu se pribraja {zaDomacina}, a gostu {zaGosta} golova.",
                nameof(unos.Golovi));
        }

        // Sve je u redu: stari golovi se brišu (Clear na praćenoj kolekciji – obavezna veza bez roditelja = DELETE),
        // novi se dodaju, a rezultat i status upisuju. Sve u jednom SaveChanges = jednoj transakciji.
        utakmica.Golovi.Clear();
        foreach (var gol in noviGolovi.OrderBy(g => g.Minuta))
        {
            utakmica.Golovi.Add(gol);
        }

        utakmica.GoloviDomacin = unos.GoloviDomacin;
        utakmica.GoloviGost = unos.GoloviGost;
        utakmica.Status = StatusUtakmice.Odigrana;

        await Context.SaveChangesAsync(ct);
        return RezultatOperacije.Ok();
    }

    public async Task<RezultatOperacije> PonistiRezultatAsync(int utakmicaId, CancellationToken ct = default)
    {
        var utakmica = await Context.Utakmice
            .Include(u => u.Golovi)
            .FirstOrDefaultAsync(u => u.Id == utakmicaId, ct);

        if (utakmica is null)
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NijePronadeno, "Utakmica ne postoji.");
        }

        if (utakmica.Status != StatusUtakmice.Odigrana)
        {
            return RezultatOperacije.Greska(PogreskaOperacije.NeispravnoStanje, "Utakmica nema upisan rezultat.");
        }

        utakmica.Golovi.Clear();
        utakmica.GoloviDomacin = null;
        utakmica.GoloviGost = null;
        utakmica.Status = StatusUtakmice.Zakazana;

        await Context.SaveChangesAsync(ct);
        return RezultatOperacije.Ok();
    }
}
