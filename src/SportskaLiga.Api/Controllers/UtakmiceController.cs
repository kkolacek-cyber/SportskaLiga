using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using SportskaLiga.Api.Dtos;
using SportskaLiga.Domain;
using SportskaLiga.Domain.Interfaces;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Api.Controllers;

/// <summary>Detalji utakmice (javno) te unos i poništavanje rezultata (JWT).</summary>
[Route("api/utakmice")]
[Tags("Utakmice")]
public class UtakmiceController : ApiKontrolerBase
{
    private const string RutaDetalji = "UtakmicaDetalji";

    private readonly IUtakmicaRepository _utakmice;

    public UtakmiceController(IUtakmicaRepository utakmice)
    {
        _utakmice = utakmice;
    }

    // Id korisnika iz tokena (oznaka "sub") i je li administrator (oznaka "role").
    private string? KorisnikId => User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

    private bool JeAdministrator => User.IsInRole(Roles.Administrator);

    /// <summary>Utakmica sa strijelcima.</summary>
    /// <param name="id">Id utakmice.</param>
    /// <response code="200">Utakmica.</response>
    /// <response code="404">Utakmica ne postoji.</response>
    [HttpGet("{id:int}", Name = RutaDetalji)]
    [ProducesResponseType<UtakmicaDetaljiDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UtakmicaDetaljiDto>> Detalji(int id, CancellationToken ct)
    {
        var utakmica = await _utakmice.GetDetailsAsync(id, ct);
        return utakmica is null
            ? NijePronadeno($"Utakmica s Id-em {id} ne postoji.")
            : Ok(utakmica.ToDetaljiDto());
    }

    /// <summary>Utakmice kojima je termin prošao, a rezultat još nije upisan.</summary>
    /// <remarks>Delegat vidi samo utakmice koje su njemu dodijeljene, administrator sve.</remarks>
    /// <response code="200">Stranica utakmica, najstarije prve.</response>
    /// <response code="400">Neispravno straničenje.</response>
    [HttpGet("cekaju-rezultat")]
    [Authorize(Roles = Roles.Osoblje)]
    [ProducesResponseType<StraniceDto<UtakmicaDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StraniceDto<UtakmicaDto>>> CekajuRezultat(
        [Range(1, int.MaxValue, ErrorMessage = "Stranica mora biti 1 ili veća.")] int stranica = 1,
        [Range(1, 100, ErrorMessage = "Veličina stranice mora biti između {1} i {2}.")] int velicinaStranice = 20,
        CancellationToken ct = default)
    {
        var rezultat = await _utakmice.SearchAsync(new UtakmicaFilter
        {
            DelegatId = JeAdministrator ? null : KorisnikId,
            SamoCekajuRezultat = true,
            Stranica = stranica,
            VelicinaStranice = velicinaStranice
        }, ct);

        return Ok(rezultat.ToDto(u => u.ToDto()));
    }

    /// <summary>Upisuje (ili ispravlja) rezultat i strijelce.</summary>
    /// <remarks>
    /// Smije delegat dodijeljen utakmici ili administrator. Utakmica mora biti počela i ne smije biti odgođena.
    /// Broj zapisa u "golovi" mora biti jednak zbroju golova, a golovi po momčadi moraju se slagati s rezultatom
    /// (autogol se pribraja protivniku). Postojeći strijelci se zamjenjuju.
    /// </remarks>
    /// <param name="id">Id utakmice.</param>
    /// <param name="unos">Rezultat i strijelci.</param>
    /// <response code="204">Rezultat je upisan; utakmica je odigrana.</response>
    /// <response code="400">Neispravni podaci (npr. broj strijelaca, strijelac ne igra na utakmici).</response>
    /// <response code="403">Utakmica nije dodijeljena ovom delegatu.</response>
    /// <response code="404">Utakmica ne postoji.</response>
    /// <response code="409">Utakmica još nije počela ili je odgođena.</response>
    [HttpPut("{id:int}/rezultat")]
    [Authorize(Roles = Roles.Osoblje)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> UnesiRezultat(int id, RezultatUnosDto unos, CancellationToken ct)
    {
        // Sva pravila su u repozitoriju (ista kao u administraciji); kontroler samo prevodi ishod u HTTP kod.
        var rezultat = await _utakmice.UnesiRezultatAsync(id, unos.ToUnos(), KorisnikId, JeAdministrator, ct);
        return rezultat.Uspjeh ? NoContent() : Neuspjeh(rezultat);
    }

    /// <summary>Poništava rezultat: briše golove, utakmica se vraća u status Zakazana.</summary>
    /// <param name="id">Id utakmice.</param>
    /// <response code="204">Rezultat je poništen.</response>
    /// <response code="404">Utakmica ne postoji.</response>
    /// <response code="409">Utakmica nema upisan rezultat.</response>
    [HttpDelete("{id:int}/rezultat")]
    [Authorize(Roles = Roles.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> PonistiRezultat(int id, CancellationToken ct)
    {
        var rezultat = await _utakmice.PonistiRezultatAsync(id, ct);
        return rezultat.Uspjeh ? NoContent() : Neuspjeh(rezultat);
    }
}
