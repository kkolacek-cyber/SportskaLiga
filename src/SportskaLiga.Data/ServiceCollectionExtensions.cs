using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SportskaLiga.Data.Repositories;
using SportskaLiga.Domain.Interfaces;

namespace SportskaLiga.Data;

/// <summary>Registracija podatkovnog sloja u DI spremnik; isti poziv koriste Portal, Admin i Api.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registrira <see cref="SportskaLigaDbContext"/> (PostgreSQL) i repozitorije. Zajedničko za Portal, Admin i Api.
    /// Connection string se čita iz <c>ConnectionStrings:SportskaLiga</c> (u Dockeru iz varijable okruženja).
    /// </summary>
    public static IServiceCollection AddSportskaLigaData(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SportskaLiga")
            ?? throw new InvalidOperationException("Connection string 'SportskaLiga' nije postavljen.");

        // DbContext je Scoped (jedna instanca po HTTP zahtjevu); baza je PostgreSQL preko Npgsql providera.
        services.AddDbContext<SportskaLigaDbContext>(options => options.UseNpgsql(connectionString));

        // Repozitoriji su Scoped i dijele DbContext zahtjeva. Opći Repository<T> pokriva jednostavne entitete (suci),
        // a svaki entitet s posebnom logikom ima vlastito sučelje i implementaciju.
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ISezonaRepository, SezonaRepository>();
        services.AddScoped<IMomcadRepository, MomcadRepository>();
        services.AddScoped<IIgracRepository, IgracRepository>();
        services.AddScoped<IUtakmicaRepository, UtakmicaRepository>();
        services.AddScoped<IPrijavaRepository, PrijavaRepository>();

        return services;
    }

    /// <summary>
    /// Primjenjuje migracije i puni bazu početnim podacima. Poziva se samo iz Admin aplikacije
    /// (ostale čekaju da Admin postane zdrav), da tri aplikacije ne migriraju istovremeno.
    /// </summary>
    public static async Task MigrateAndSeedAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        using (var scope = services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<SportskaLigaDbContext>();
            await context.Database.MigrateAsync(ct);
        }

        await DbSeeder.SeedAsync(services, ct);
    }
}
