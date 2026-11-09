using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace SportskaLiga.Api.OpenApi;

/// <summary>Opis API-ja i definicija Bearer (JWT) sheme za Swagger UI (gumb "Authorize").</summary>
internal sealed class LigaDocumentTransformer : IOpenApiDocumentTransformer
{
    public const string ShemaBearer = "Bearer";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "SportskaLiga API",
            Version = "v1",
            Description = """
                Web API amaterske sportske lige: tablica, rezultati, raspored i strijelci za widget na web stranicama klubova,
                te unos rezultata i upravljanje momčadima za ovlaštene korisnike.

                **Javno (bez tokena):** sezone, tablica, raspored, rezultati, strijelci, detalji utakmice i momčadi.
                Ako sezona nije zadana (`sezonaId`), koristi se aktivna sezona.

                **Autentikacija:** pozovite `POST /api/auth/login` (npr. `delegat@liga.local` / `Delegat123!` ili
                `admin@liga.local` / `Admin123!`), zatim token pošaljite u zaglavlju `Authorization: Bearer {token}`.
                Delegat upisuje rezultate svojih utakmica; administrator sve te upravlja momčadima.
                """
        };

        document.Tags = new HashSet<OpenApiTag>
        {
            new() { Name = "Auth", Description = "Prijava i izdavanje JWT tokena." },
            new() { Name = "Liga", Description = "Javni podaci za widget: sezone, tablica, raspored, rezultati, strijelci." },
            new() { Name = "Utakmice", Description = "Detalji utakmice (javno), unos i poništavanje rezultata (token)." },
            new() { Name = "Momcadi", Description = "Momčadi i igrači (čitanje javno, izmjene samo Administrator)." }
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[ShemaBearer] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT dobiven s POST /api/auth/login. Unesite samo token (bez riječi \"Bearer\")."
        };

        return Task.CompletedTask;
    }
}

/// <summary>
/// Greške opisuje kao application/problem+json (kako ih poslužitelj stvarno šalje), a zaštićenim operacijama
/// dodaje Bearer zahtjev (lokot u Swaggeru) i odgovore 401 i 403.
/// </summary>
internal sealed class LigaOperationTransformer : IOpenApiOperationTransformer
{
    private const string TipJson = "application/json";
    private const string TipProblem = "application/problem+json";

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        OpisiGreskeKaoProblemJson(operation);

        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        var zasticena = metadata.OfType<IAuthorizeData>().Any() && !metadata.OfType<IAllowAnonymous>().Any();

        if (!zasticena)
        {
            return Task.CompletedTask;
        }

        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(LigaDocumentTransformer.ShemaBearer, context.Document)] = []
        });

        operation.Responses ??= [];
        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Nedostaje token ili nije valjan / istekao je." });

        // Uloga s akcije ima prednost pred ulogom s kontrolera (zadnji atribut je najbliži akciji).
        var uloge = metadata.OfType<IAuthorizeData>().Select(a => a.Roles).LastOrDefault(r => !string.IsNullOrEmpty(r));
        operation.Responses.TryAdd("403", new OpenApiResponse
        {
            Description = uloge is null ? "Nemate ovlasti." : $"Nemate ovlasti (potrebna uloga: {uloge.Replace(",", " ili ")})."
        });

        return Task.CompletedTask;
    }

    private static void OpisiGreskeKaoProblemJson(OpenApiOperation operation)
    {
        foreach (var (kod, odgovor) in operation.Responses ?? [])
        {
            if (!int.TryParse(kod, out var status) || status < 400 || odgovor.Content is not { } sadrzaj)
            {
                continue;
            }

            if (sadrzaj.TryGetValue(TipJson, out var medij))
            {
                sadrzaj.Remove(TipJson);
                sadrzaj[TipProblem] = medij;
            }
        }
    }
}
