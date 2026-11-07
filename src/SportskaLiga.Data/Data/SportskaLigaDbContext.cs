using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Data;

/// <summary>
/// Jedini EF Core kontekst cijelog rješenja. Nasljeđuje <c>IdentityDbContext</c> pa u istoj bazi drži i tablice
/// ASP.NET Core Identityja (korisnici i uloge) i sve domenske tablice. Dijele ga Portal, Admin i Api.
/// </summary>
public class SportskaLigaDbContext : IdentityDbContext<ApplicationUser>
{
    public SportskaLigaDbContext(DbContextOptions<SportskaLigaDbContext> options)
        : base(options)
    {
    }

    // Jedan DbSet po tablici. Imena su množina na hrvatskom i odgovaraju nazivima tablica u bazi.
    public DbSet<Sezona> Sezone { get; set; } = default!;
    public DbSet<Momcad> Momcadi { get; set; } = default!;
    public DbSet<SezonaMomcad> SezoneMomcadi { get; set; } = default!;
    public DbSet<Igrac> Igraci { get; set; } = default!;
    public DbSet<Sudac> Suci { get; set; } = default!;
    public DbSet<Utakmica> Utakmice { get; set; } = default!;
    public DbSet<Gol> Golovi { get; set; } = default!;

    // Fluent konfiguracija: indeksi, ograničenja, veze između tablica i ponašanje pri brisanju.
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Najprije osnovna klasa konfigurira Identity tablice (AspNetUsers, AspNetRoles...).
        base.OnModelCreating(builder);

        builder.Entity<Sezona>(e =>
        {
            e.HasIndex(x => x.Naziv).IsUnique();
            // Baza sama odbija sezonu koja završava prije nego počne (zaštita i mimo aplikacije).
            e.ToTable(t => t.HasCheckConstraint("CK_Sezona_Razdoblje", "\"DatumZavrsetka\" >= \"DatumPocetka\""));
        });

        builder.Entity<Momcad>(e =>
        {
            e.HasIndex(x => x.Naziv).IsUnique();
        });

        // M:N Sezona ↔ Momcad preko eksplicitne spojne tablice.
        builder.Entity<SezonaMomcad>(e =>
        {
            // Složeni primarni ključ: momčad može biti prijavljena u istu sezonu samo jednom.
            e.HasKey(x => new { x.SezonaId, x.MomcadId });

            e.HasOne(x => x.Sezona)
                .WithMany(s => s.Prijave)
                .HasForeignKey(x => x.SezonaId)
                // Cascade: brisanjem sezone brišu se i njezine prijave.
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Momcad)
                .WithMany(m => m.Sezone)
                .HasForeignKey(x => x.MomcadId)
                // Restrict: momčad koja je igrala u nekoj sezoni ne može se obrisati.
                .OnDelete(DeleteBehavior.Restrict);
        });

        // 1:N Momcad → Igrac. Broj dresa je jedinstven unutar momčadi (složeni unique indeks).
        builder.Entity<Igrac>(e =>
        {
            // Enumi se u bazi spremaju kao tekst ("Napad"), ne kao broj: čitljivije u SQL-u i sigurno pri promjeni redoslijeda.
            e.Property(x => x.Pozicija).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => new { x.MomcadId, x.BrojDresa }).IsUnique();
            e.HasIndex(x => new { x.Prezime, x.Ime });

            e.HasOne(x => x.Momcad)
                .WithMany(m => m.Igraci)
                .HasForeignKey(x => x.MomcadId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Utakmica>(e =>
        {
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            // Indeks ubrzava najčešći upit: utakmice jedne sezone po kolima.
            e.HasIndex(x => new { x.SezonaId, x.Kolo });

            // Momčad ne može igrati sama protiv sebe; rezultat ne može biti negativan.
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Utakmica_RazliciteMomcadi", "\"DomacinId\" <> \"GostId\"");
                t.HasCheckConstraint("CK_Utakmica_Golovi", "\"GoloviDomacin\" >= 0 AND \"GoloviGost\" >= 0");
            });

            e.HasOne(x => x.Sezona)
                .WithMany(s => s.Utakmice)
                .HasForeignKey(x => x.SezonaId)
                // Cascade: brisanjem sezone brišu se i njezine utakmice (a s njima i golovi).
                .OnDelete(DeleteBehavior.Cascade);

            // Dvije veze prema istoj tablici: EF-u izričito kažemo koja navigacija ide s kojim stranim ključem.
            e.HasOne(x => x.Domacin)
                .WithMany(m => m.DomaceUtakmice)
                .HasForeignKey(x => x.DomacinId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Gost)
                .WithMany(m => m.GostujuceUtakmice)
                .HasForeignKey(x => x.GostId)
                .OnDelete(DeleteBehavior.Restrict);

            // SetNull: brisanjem suca utakmica ostaje, samo bez suca.
            e.HasOne(x => x.Sudac)
                .WithMany(s => s.Utakmice)
                .HasForeignKey(x => x.SudacId)
                .OnDelete(DeleteBehavior.SetNull);

            // Domena ne zna za Identity: FK prema delegatu postoji samo ovdje.
            e.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.DelegatId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Gol>(e =>
        {
            e.HasOne(x => x.Utakmica)
                .WithMany(u => u.Golovi)
                .HasForeignKey(x => x.UtakmicaId)
                // Cascade: brisanjem utakmice (ili poništavanjem rezultata) brišu se njezini golovi.
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict: igrač koji je zabio gol ne može se obrisati (povijest strijelaca ostaje točna).
            e.HasOne(x => x.Igrac)
                .WithMany(i => i.Golovi)
                .HasForeignKey(x => x.IgracId)
                .OnDelete(DeleteBehavior.Restrict);

            // Momčad kojoj se gol pribraja; momčad nema kolekciju golova, pa je WithMany() bez navigacije.
            e.HasOne(x => x.Momcad)
                .WithMany()
                .HasForeignKey(x => x.MomcadId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
