using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SportskaLiga.Data;

/// <summary>
/// Koristi je samo <c>dotnet ef</c> (migracije) pa Data projekt ne treba pokretati kao aplikaciju.
/// Connection string se može nadjačati varijablom okruženja <c>ConnectionStrings__SportskaLiga</c>.
/// </summary>
public class SportskaLigaDbContextFactory : IDesignTimeDbContextFactory<SportskaLigaDbContext>
{
    // Zadana lokalna baza iz docker-compose.yml (pokreće se s `docker compose up -d db`).
    private const string RazvojniConnectionString =
        "Host=localhost;Port=5432;Database=sportskaliga;Username=liga;Password=liga_dev_password";

    public SportskaLigaDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__SportskaLiga")
            ?? RazvojniConnectionString;

        var options = new DbContextOptionsBuilder<SportskaLigaDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new SportskaLigaDbContext(options);
    }
}
