using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SportskaLiga.Domain;
using SportskaLiga.Domain.Enums;
using SportskaLiga.Domain.Logika;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Data;

/// <summary>
/// Početni podaci: uloge i testni korisnici (uvijek, ako ne postoje) te demo liga (samo kad je baza prazna).
/// Testni računi se mogu zadati kroz konfiguraciju <c>Seed:*</c> (u Dockeru varijable SEED_*);
/// zadane vrijednosti su samo DEMO i dokumentirane u README-u.
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Redoslijed: uloge → administrator i delegati → (samo ako je baza prazna) demo liga. Sigurno je pozvati više puta:
    /// postojeći korisnici i uloge se ne dupliciraju, a demo podaci se ne dodaju ako već postoje.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var context = sp.GetRequiredService<SportskaLigaDbContext>();
        var configuration = sp.GetRequiredService<IConfiguration>();

        await SeedRolesAsync(sp.GetRequiredService<RoleManager<IdentityRole>>());

        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        await SeedUserAsync(userManager, Roles.Administrator,
            Postavka(configuration, "Seed:AdminEmail", "admin@liga.local"),
            Postavka(configuration, "Seed:AdminPassword", "Admin123!"),
            "Ana Administrator");

        var lozinkaDelegata = Postavka(configuration, "Seed:DelegatPassword", "Delegat123!");
        var delegat = await SeedUserAsync(userManager, Roles.Delegat,
            Postavka(configuration, "Seed:DelegatEmail", "delegat@liga.local"),
            lozinkaDelegata,
            "Davor Delegat");

        // Drugi delegat služi za demonstraciju pravila "delegat upisuje samo svoje utakmice".
        var delegat2 = await SeedUserAsync(userManager, Roles.Delegat, "delegat2@liga.local", lozinkaDelegata, "Goran Delegat");

        if (!await context.Momcadi.AnyAsync(ct))
        {
            await SeedLigaAsync(context, [delegat.Id, delegat2.Id], ct);
        }
    }

    /// <summary>Prazna vrijednost (npr. <c>SEED_ADMIN_PASSWORD=</c> u .env) tretira se kao da nije zadana.</summary>
    private static string Postavka(IConfiguration configuration, string kljuc, string zadano)
        => string.IsNullOrWhiteSpace(configuration[kljuc]) ? zadano : configuration[kljuc]!;

    /// <summary>Stvara uloge Administrator i Delegat ako ne postoje.</summary>
    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { Roles.Administrator, Roles.Delegat })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                Provjeri(await roleManager.CreateAsync(new IdentityRole(role)), $"uloga {role}");
            }
        }
    }

    /// <summary>Stvara korisnika (ako ne postoji) i dodjeljuje mu ulogu; vraća korisnika.</summary>
    private static async Task<ApplicationUser> SeedUserAsync(
        UserManager<ApplicationUser> userManager, string role, string email, string password, string imePrezime)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                ImePrezime = imePrezime
            };

            Provjeri(await userManager.CreateAsync(user, password), $"korisnik {email}");
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            Provjeri(await userManager.AddToRoleAsync(user, role), $"uloga {role} za {email}");
        }

        return user;
    }

    // Ako Identity odbije stvaranje (npr. preslaba lozinka), pokretanje se prekida s razumljivom porukom.
    private static void Provjeri(IdentityResult result, string sto)
    {
        if (!result.Succeeded)
        {
            var greske = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Seed nije uspio ({sto}): {greske}");
        }
    }

    /// <summary>
    /// Demo liga: 8 momčadi s po 9 igrača, 4 suca, dvije sezone.
    ///  - 2025./2026.: cijela odigrana (dvostruki krug), s golovima – tablica i strijelci odmah imaju podatke;
    ///  - 2026./2027. (aktivna): utakmice čiji je termin prošao su odigrane, ostale zakazane;
    ///    zadnja prošla utakmica namjerno je bez rezultata (čeka delegata), a jedna sljedeća je odgođena.
    /// Rezultati su "slučajni", ali uvijek isti (Random sa sjemenom), da demo izgleda jednako pri svakom pokretanju.
    /// </summary>
    private static async Task SeedLigaAsync(SportskaLigaDbContext context, string[] delegati, CancellationToken ct)
    {
        var rnd = new Random(2026);

        // --- Momčadi (izmišljene) ---
        var momcadi = new List<Momcad>
        {
            new() { Naziv = "NK Vukovi Dioš", Grad = "Dioš", Igraliste = "Igralište Vukova", GodinaOsnutka = 1974 },
            new() { Naziv = "NK Sokolovi Daruvar", Grad = "Daruvar", Igraliste = "Sportski park Daruvar", GodinaOsnutka = 1952 },
            new() { Naziv = "NK Rode Bjelovar", Grad = "Bjelovar", Igraliste = "Igralište Rode", GodinaOsnutka = 1961 },
            new() { Naziv = "NK Jeleni Garešnica", Grad = "Garešnica", Igraliste = "Šumsko igralište", GodinaOsnutka = 1968 },
            new() { Naziv = "NK Orlovi Pakrac", Grad = "Pakrac", Igraliste = "Igralište pod gradom", GodinaOsnutka = 1949 },
            new() { Naziv = "NK Medvjedi Grubišno Polje", Grad = "Grubišno Polje", Igraliste = "Gradsko igralište", GodinaOsnutka = 1971 },
            new() { Naziv = "NK Lisice Končanica", Grad = "Končanica", Igraliste = "Igralište uz školu", GodinaOsnutka = 1983 },
            new() { Naziv = "NK Sove Đulovac", Grad = "Đulovac", Igraliste = "Igralište Sova", GodinaOsnutka = 1979 }
        };
        foreach (var m in momcadi)
        {
            m.KontaktEmail = $"tajnik@{Slug(m.Grad)}.liga.local";
        }

        context.Momcadi.AddRange(momcadi);

        // --- Igrači: 9 po momčadi (vratar, 3 obrana, 3 vezni, 2 napad) ---
        string[] imena = ["Ivan", "Marko", "Luka", "Petar", "Josip", "Ante", "Filip", "Matej", "Tomislav", "Karlo",
                          "David", "Dario", "Nikola", "Domagoj", "Mario", "Stjepan", "Fran", "Leon", "Bruno", "Mateo"];
        string[] prezimena = ["Horvat", "Kovačević", "Babić", "Marić", "Novak", "Jurić", "Knežević", "Vuković", "Pavlović",
                              "Božić", "Tomić", "Kolar", "Matić", "Šimić", "Perić", "Lovrić", "Grgić", "Barišić", "Pavić",
                              "Radić", "Kos", "Blažević", "Vidović", "Klarić", "Brkić", "Petrović", "Filipović", "Đurić",
                              "Mikulić", "Lončar"];
        (Pozicija Pozicija, int Broj)[] sastav =
        [
            (Pozicija.Vratar, 1),
            (Pozicija.Obrana, 2), (Pozicija.Obrana, 4), (Pozicija.Obrana, 5),
            (Pozicija.Vezni, 6), (Pozicija.Vezni, 8), (Pozicija.Vezni, 10),
            (Pozicija.Napad, 9), (Pozicija.Napad, 11)
        ];

        foreach (var m in momcadi)
        {
            foreach (var (pozicija, broj) in sastav)
            {
                m.Igraci.Add(new Igrac
                {
                    Ime = imena[rnd.Next(imena.Length)],
                    Prezime = prezimena[rnd.Next(prezimena.Length)],
                    DatumRodenja = new DateOnly(1988 + rnd.Next(19), 1 + rnd.Next(12), 1 + rnd.Next(28)),
                    BrojDresa = broj,
                    Pozicija = pozicija
                });
            }
        }

        // --- Suci ---
        var suci = new List<Sudac>
        {
            new() { Ime = "Zoran", Prezime = "Šarić", Email = "zoran.saric@suci.liga.local", Telefon = "091 111 2233" },
            new() { Ime = "Krešimir", Prezime = "Bilić", Email = "kresimir.bilic@suci.liga.local", Telefon = "092 222 3344" },
            new() { Ime = "Igor", Prezime = "Mandić", Email = "igor.mandic@suci.liga.local", Telefon = "095 333 4455" },
            new() { Ime = "Hrvoje", Prezime = "Žganec", Email = "hrvoje.zganec@suci.liga.local", Telefon = "098 444 5566" }
        };
        context.Suci.AddRange(suci);

        // --- Sezone i prijave momčadi (M:N) ---
        var prosla = new Sezona
        {
            Naziv = "2025./2026.",
            DatumPocetka = new DateOnly(2025, 8, 1),
            DatumZavrsetka = new DateOnly(2026, 6, 30),
            Aktivna = false
        };
        var tekuca = new Sezona
        {
            Naziv = "2026./2027.",
            DatumPocetka = new DateOnly(2026, 8, 1),
            DatumZavrsetka = new DateOnly(2027, 6, 30),
            Aktivna = true
        };

        foreach (var sezona in new[] { prosla, tekuca })
        {
            foreach (var m in momcadi)
            {
                sezona.Prijave.Add(new SezonaMomcad { Momcad = m, DatumPrijave = sezona.DatumPocetka.AddDays(-20) });
            }
        }

        context.Sezone.AddRange(prosla, tekuca);

        // Prvo spremanje: momčadi, igrači, suci i sezone dobivaju Id-eve iz baze (trebaju rasporedu i golovima).
        await context.SaveChangesAsync(ct);

        // --- Raspored i rezultati ---
        var igraciPoMomcadi = momcadi.ToDictionary(m => m.Id, m => m.Igraci.ToList());
        var sada = DateTime.UtcNow;

        List<Utakmica> Raspored(Sezona sezona, DateTime prviTermin, int razmakDana)
        {
            var parovi = BergerovRaspored.Generiraj(momcadi.Select(m => m.Id).ToList(), dvostrukiKrug: true);
            var poId = momcadi.ToDictionary(m => m.Id);
            var brojac = 0;

            return parovi.Select(p => new Utakmica
            {
                SezonaId = sezona.Id,
                Kolo = p.Kolo,
                Pocetak = Kalendar.UUtc(prviTermin.AddDays((p.Kolo - 1) * razmakDana)),
                Lokacija = poId[p.DomacinId].Igraliste,
                DomacinId = p.DomacinId,
                GostId = p.GostId,
                SudacId = suci[rnd.Next(suci.Count)].Id,
                DelegatId = delegati[brojac++ % delegati.Length],
                Status = StatusUtakmice.Zakazana
            }).ToList();
        }

        var utakmiceProsle = Raspored(prosla, new DateTime(2025, 8, 30, 17, 0, 0), razmakDana: 14);
        var utakmiceTekuce = Raspored(tekuca, new DateTime(2026, 8, 29, 16, 0, 0), razmakDana: 14);

        foreach (var u in utakmiceProsle)
        {
            Odigraj(u);
        }

        // Tekuća sezona: odigrane su samo utakmice kojima je termin prošao.
        var prosleTekuce = utakmiceTekuce.Where(u => u.Pocetak < sada).ToList();
        var cekaRezultat = prosleTekuce
            .Where(u => u.DelegatId == delegati[0])
            .OrderByDescending(u => u.Pocetak)
            .FirstOrDefault();

        foreach (var u in prosleTekuce.Where(u => u != cekaRezultat))
        {
            Odigraj(u);
        }

        // Jedna od sljedećih utakmica je odgođena (demo statusa "Odgođena").
        var odgodena = utakmiceTekuce.Where(u => u.Pocetak >= sada).OrderBy(u => u.Pocetak).Skip(1).FirstOrDefault();
        if (odgodena is not null)
        {
            odgodena.Status = StatusUtakmice.Odgodena;
            odgodena.Napomena = "Teren nije za igru nakon obilnih kiša – novi termin naknadno.";
        }

        context.Utakmice.AddRange(utakmiceProsle);
        context.Utakmice.AddRange(utakmiceTekuce);
        await context.SaveChangesAsync(ct);

        // ---------------------------------------------------------------- pomoćne funkcije

        // Upisuje "slučajan" rezultat i strijelce (minute od 1 do 90, kronološki).
        void Odigraj(Utakmica u)
        {
            var golDomacin = BrojGolova();
            var golGost = BrojGolova();

            var minute = Enumerable.Range(1, 90).OrderBy(_ => rnd.Next()).Take(golDomacin + golGost).Order().ToList();
            var zaDomacina = Enumerable.Repeat(true, golDomacin).Concat(Enumerable.Repeat(false, golGost))
                .OrderBy(_ => rnd.Next()).ToList();

            for (var i = 0; i < minute.Count; i++)
            {
                var zasluzna = zaDomacina[i] ? u.DomacinId : u.GostId;
                var protivnik = zaDomacina[i] ? u.GostId : u.DomacinId;

                // Otprilike svaki 25. gol je autogol: zabio ga je igrač protivnika.
                var autogol = rnd.Next(25) == 0;
                var strijelac = autogol
                    ? igraciPoMomcadi[protivnik].Where(x => x.Pozicija == Pozicija.Obrana).ElementAt(rnd.Next(3))
                    : Strijelac(igraciPoMomcadi[zasluzna]);

                u.Golovi.Add(new Gol { IgracId = strijelac.Id, MomcadId = zasluzna, Minuta = minute[i], Autogol = autogol });
            }

            u.GoloviDomacin = golDomacin;
            u.GoloviGost = golGost;
            u.Status = StatusUtakmice.Odigrana;
        }

        // Raspodjela golova po utakmici nalik amaterskom nogometu (0–5, najčešće 1).
        int BrojGolova()
        {
            var x = rnd.NextDouble();
            return x switch
            {
                < 0.22 => 0,
                < 0.52 => 1,
                < 0.77 => 2,
                < 0.90 => 3,
                < 0.97 => 4,
                _ => 5
            };
        }

        // Napadači zabijaju najčešće, vratari nikad.
        Igrac Strijelac(List<Igrac> igraci)
        {
            var tezine = igraci.Select(i => i.Pozicija switch
            {
                Pozicija.Napad => 5,
                Pozicija.Vezni => 3,
                Pozicija.Obrana => 1,
                _ => 0
            }).ToList();

            var x = rnd.Next(tezine.Sum());
            for (var i = 0; i < igraci.Count; i++)
            {
                if (x < tezine[i])
                {
                    return igraci[i];
                }

                x -= tezine[i];
            }

            return igraci[^1];
        }
    }

    // "Grubišno Polje" → "grubisno-polje" (za demo e-mail adrese).
    private static string Slug(string tekst)
        => tekst.ToLowerInvariant()
            .Replace('č', 'c').Replace('ć', 'c').Replace('đ', 'd').Replace('š', 's').Replace('ž', 'z')
            .Replace(' ', '-');
}
