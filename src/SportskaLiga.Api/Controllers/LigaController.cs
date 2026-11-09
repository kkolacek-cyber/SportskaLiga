using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SportskaLiga.Api.Dtos;
using SportskaLiga.Domain.Enums;
using SportskaLiga.Domain.Interfaces;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Api.Controllers;

/// <summary>
/// Javni podaci lige za widget na web stranicama klubova: sezone, tablica, raspored, rezultati i strijelci.
/// Sve je javno (bez tokena) i samo za čitanje. Ako sezona nije zadana, koristi se aktivna.
/// </summary>
[Route("api")]
[Tags("Liga")]
public class LigaController : ApiKontrolerBase
{
    private readonly ISezonaRepository _sezone;
    private readonly IUtakmicaRepository _utakmice;

    public LigaController(ISezonaRepository sezone, IUtakmicaRepository utakmice)
    {
        _sezone = sezone;
        _utakmice = utakmice;
    }

    /// <summary>Sve sezone, najnovija prva.</summary>
    /// <response code="200">Popis sezona.</response>
    [HttpGet("sezone")]
    [ProducesResponseType<IReadOnlyList<SezonaDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SezonaDto>>> Sezone(CancellationToken ct)
    {
        var sezone = await _sezone.GetSveAsync(ct);
        return Ok(sezone.Select(s => s.ToDto()).ToList());
    }

    /// <summary>Tablica lige.</summary>
    /// <remarks>Računa se iz odigranih utakmica: bodovi, zatim gol-razlika, zatim više postignutih golova.</remarks>
    /// <param name="sezonaId">Id sezone; ako se izostavi, aktivna sezona.</param>
    /// <response code="200">Tablica sezone.</response>
    /// <response code="404">Sezona ne postoji.</response>
    [HttpGet("tablica")]
    [ProducesResponseType<TablicaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TablicaDto>> Tablica(int? sezonaId, CancellationToken ct)
    {
        var sezona = await PronadiSezonuAsync(sezonaId, ct);
        if (sezona is null)
        {
            return NemaSezone(sezonaId);
        }

        var redovi = await _utakmice.GetTablicaAsync(sezona.Id, ct);
        return Ok(new TablicaDto(sezona.ToDto(), redovi.Select(r => r.ToDto()).ToList()));
    }

    /// <summary>Lista strijelaca.</summary>
    /// <param name="sezonaId">Id sezone; ako se izostavi, aktivna sezona.</param>
    /// <param name="najvise">Najviše toliko redova (1–100); ako se izostavi, svi strijelci.</param>
    /// <response code="200">Strijelci sezone, od najboljeg.</response>
    /// <response code="400">Neispravan parametar.</response>
    /// <response code="404">Sezona ne postoji.</response>
    [HttpGet("strijelci")]
    [ProducesResponseType<StrijelciDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StrijelciDto>> Strijelci(
        int? sezonaId,
        [Range(1, 100, ErrorMessage = "Parametar 'najvise' mora biti između {1} i {2}.")] int? najvise,
        CancellationToken ct)
    {
        var sezona = await PronadiSezonuAsync(sezonaId, ct);
        if (sezona is null)
        {
            return NemaSezone(sezonaId);
        }

        var strijelci = await _utakmice.GetStrijelciAsync(sezona.Id, najvise, ct);
        return Ok(new StrijelciDto(sezona.ToDto(), strijelci.Select(s => s.ToDto()).ToList()));
    }

    /// <summary>Raspored: utakmice bez rezultata (zakazane i odgođene), kronološki.</summary>
    /// <response code="200">Stranica rasporeda.</response>
    /// <response code="400">Neispravan filtar ili straničenje.</response>
    /// <response code="404">Sezona ne postoji.</response>
    [HttpGet("raspored")]
    [ProducesResponseType<StraniceDto<UtakmicaDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<ActionResult<StraniceDto<UtakmicaDto>>> Raspored([FromQuery] UtakmiceUpitDto upit, CancellationToken ct)
        => PopisUtakmicaAsync(upit, rezultati: false, ct);

    /// <summary>Rezultati odigranih utakmica, najnoviji prvi.</summary>
    /// <response code="200">Stranica rezultata.</response>
    /// <response code="400">Neispravan filtar ili straničenje.</response>
    /// <response code="404">Sezona ne postoji.</response>
    [HttpGet("rezultati")]
    [ProducesResponseType<StraniceDto<UtakmicaDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<ActionResult<StraniceDto<UtakmicaDto>>> Rezultati([FromQuery] UtakmiceUpitDto upit, CancellationToken ct)
        => PopisUtakmicaAsync(upit, rezultati: true, ct);

    // Raspored i rezultati su ista pretraga, razlikuju se samo statusom i smjerom sortiranja.
    private async Task<ActionResult<StraniceDto<UtakmicaDto>>> PopisUtakmicaAsync(UtakmiceUpitDto upit, bool rezultati, CancellationToken ct)
    {
        var sezona = await PronadiSezonuAsync(upit.SezonaId, ct);
        if (sezona is null)
        {
            return NemaSezone(upit.SezonaId);
        }

        var stranica = await _utakmice.SearchAsync(new UtakmicaFilter
        {
            SezonaId = sezona.Id,
            Kolo = upit.Kolo,
            MomcadId = upit.MomcadId,
            Status = rezultati ? StatusUtakmice.Odigrana : null,
            SamoNeodigrane = !rezultati,
            NajnovijePrvo = rezultati,
            Stranica = upit.Stranica,
            VelicinaStranice = upit.VelicinaStranice
        }, ct);

        return Ok(stranica.ToDto(u => u.ToDto()));
    }

    // Zadana sezona ili, ako nije zadana, aktivna.
    private async Task<Sezona?> PronadiSezonuAsync(int? sezonaId, CancellationToken ct)
        => sezonaId is int id
            ? await _sezone.GetByIdAsync(id, ct)
            : await _sezone.GetAktivnaAsync(ct);

    private ActionResult NemaSezone(int? sezonaId)
        => NijePronadeno(sezonaId is null ? "Liga još nema nijednu sezonu." : $"Sezona s Id-em {sezonaId} ne postoji.");
}
