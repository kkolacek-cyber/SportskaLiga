using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SportskaLiga.Api.Infrastructure;

/// <summary>
/// Odgovor 400 za neispravno tijelo zahtjeva. Zadano ponašanje za pogreške pri čitanju JSON-a ispisuje engleski tekst
/// s internim detaljima ("LineNumber", "BytePositionInLine") i dodaje suvišnu poruku "The unos field is required.";
/// ovdje se to pretvara u kratku poruku uz ime polja.
/// </summary>
public static class ApiValidacija
{
    public static IActionResult OdgovorNeispravanModel(ActionContext kontekst)
    {
        // Ključ "" znači da tijelo zahtjeva uopće nije poslano; "$..." da JSON nije moguće pročitati.
        var poljaSJsonPogreskom = kontekst.ModelState.Keys.Any(k => k.Length == 0 || k.StartsWith("$", StringComparison.Ordinal));
        var greske = new Dictionary<string, string[]>();

        foreach (var (kljuc, unos) in kontekst.ModelState)
        {
            if (unos.ValidationState != ModelValidationState.Invalid)
            {
                continue;
            }

            // "$.stanje" / "$.razdoblje.datumOd" -> "stanje" / "razdoblje.datumOd"
            var jeJsonPogreska = kljuc.StartsWith("$", StringComparison.Ordinal);
            var polje = jeJsonPogreska ? kljuc.TrimStart('$', '.') : kljuc;

            // Ako tijelo uopće nije moglo biti pročitano, parametar akcije (npr. "unos") se dodatno prijavljuje kao prazan.
            var jeImeParametra = kontekst.ActionDescriptor.Parameters.Any(p => p.Name == kljuc);
            if (jeImeParametra && poljaSJsonPogreskom)
            {
                continue;
            }

            var jePraznoTijelo = kljuc.Length == 0;
            greske[jePraznoTijelo ? "tijelo" : polje] = unos.Errors
                .Select(g => jePraznoTijelo ? "Tijelo zahtjeva je obavezno (JSON)."
                    : jeJsonPogreska ? "Vrijednost nije ispravnog oblika (pogrešan tip podatka, nepoznata vrijednost ili neispravan JSON)."
                    : g.ErrorMessage)
                .Distinct()
                .ToArray();
        }

        var problem = new ValidationProblemDetails(greske)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Podaci nisu ispravni",
            Detail = "Ispravite navedena polja i pokušajte ponovno."
        };
        problem.Extensions["traceId"] = kontekst.HttpContext.TraceIdentifier;

        return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
    }
}
