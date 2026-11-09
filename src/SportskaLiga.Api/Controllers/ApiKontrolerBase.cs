using Microsoft.AspNetCore.Mvc;
using SportskaLiga.Domain.Enums;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Api.Controllers;

/// <summary>
/// Zajedničke pomoćne metode kontrolera: svi odgovori o pogreškama su problem+json.
/// Namjerno nema <c>[Produces("application/json")]</c>: taj atribut bi i ProblemDetails odgovorima
/// postavio application/json umjesto application/problem+json.
/// </summary>
[ApiController]
public abstract class ApiKontrolerBase : ControllerBase
{
    protected ActionResult NijePronadeno(string detalj)
        => Problem(statusCode: StatusCodes.Status404NotFound, title: "Nije pronađeno", detail: detalj);

    protected ActionResult Konflikt(string detalj)
        => Problem(statusCode: StatusCodes.Status409Conflict, title: "Sukob s trenutnim stanjem", detail: detalj);

    protected ActionResult Zabranjeno(string detalj)
        => Problem(statusCode: StatusCodes.Status403Forbidden, title: "Nemate ovlasti", detail: detalj);

    /// <summary>400 s jednom pogreškom na polju (isti oblik kao automatska validacija).</summary>
    protected ActionResult NeispravanZahtjev(string polje, string poruka)
    {
        ModelState.AddModelError(polje, poruka);
        return NeispravanModel();
    }

    /// <summary>400 sa svim pogreškama zabilježenim u ModelState.</summary>
    protected ActionResult NeispravanModel()
        => ValidationProblem(
            detail: "Ispravite navedena polja i pokušajte ponovno.",
            statusCode: StatusCodes.Status400BadRequest,
            title: "Podaci nisu ispravni",
            modelStateDictionary: ModelState);

    /// <summary>
    /// Neuspjeh iz repozitorija → HTTP statusni kod. Ista pravila (npr. "rezultat se ne upisuje prije početka utakmice")
    /// vrijede u administraciji i API-ju jer su u repozitoriju; ovdje se samo bira oblik odgovora.
    /// </summary>
    protected ActionResult Neuspjeh(RezultatOperacije rezultat)
    {
        var poruka = rezultat.Poruka ?? "Operacija nije uspjela.";

        return rezultat.Pogreska switch
        {
            PogreskaOperacije.NijePronadeno => NijePronadeno(poruka),        // 404
            PogreskaOperacije.NemaOvlasti => Zabranjeno(poruka),             // 403
            PogreskaOperacije.NeispravnoStanje => Konflikt(poruka),          // 409
            _ => NeispravanZahtjev(rezultat.Polje ?? "zahtjev", poruka)      // 400
        };
    }
}
