using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportskaLiga.Api.Dtos;
using SportskaLiga.Domain;
using SportskaLiga.Domain.Enums;
using SportskaLiga.Domain.Interfaces;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Api.Controllers;

/// <summary>Momčadi: čitanje je javno, dodavanje, izmjena i brisanje samo za Administratora.</summary>
[Route("api/momcadi")]
[Tags("Momcadi")]
public class MomcadiController : ApiKontrolerBase
{
    private const string RutaDetalji = "MomcadDetalji";

    private readonly IMomcadRepository _momcadi;

    public MomcadiController(IMomcadRepository momcadi)
    {
        _momcadi = momcadi;
    }

    /// <summary>Popis momčadi s pretragom po nazivu i gradu.</summary>
    /// <param name="tekst">Dio naziva ili grada (ne razlikuju se velika i mala slova).</param>
    /// <param name="stranica">Broj stranice (od 1).</param>
    /// <param name="velicinaStranice">Broj stavki po stranici (1–100).</param>
    /// <response code="200">Stranica momčadi, abecedno.</response>
    /// <response code="400">Neispravno straničenje.</response>
    [HttpGet]
    [ProducesResponseType<StraniceDto<MomcadDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StraniceDto<MomcadDto>>> Popis(
        string? tekst,
        [Range(1, int.MaxValue, ErrorMessage = "Stranica mora biti 1 ili veća.")] int stranica = 1,
        [Range(1, 100, ErrorMessage = "Veličina stranice mora biti između {1} i {2}.")] int velicinaStranice = 20,
        CancellationToken ct = default)
    {
        var rezultat = await _momcadi.SearchAsync(tekst, stranica, velicinaStranice, ct);
        return Ok(rezultat.ToDto(m => m.ToDto()));
    }

    /// <summary>Momčad s igračima i sezonama.</summary>
    /// <param name="id">Id momčadi.</param>
    /// <response code="200">Momčad.</response>
    /// <response code="404">Momčad ne postoji.</response>
    [HttpGet("{id:int}", Name = RutaDetalji)]
    [ProducesResponseType<MomcadDetaljiDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MomcadDetaljiDto>> Detalji(int id, CancellationToken ct)
    {
        var momcad = await _momcadi.GetDetailsAsync(id, ct);
        return momcad is null
            ? NijePronadeno($"Momčad s Id-em {id} ne postoji.")
            : Ok(momcad.ToDetaljiDto());
    }

    /// <summary>Dodaje novu momčad.</summary>
    /// <param name="unos">Podaci momčadi.</param>
    /// <response code="201">Momčad je dodana; zaglavlje Location vodi na njezine detalje.</response>
    /// <response code="400">Neispravni podaci.</response>
    /// <response code="409">Momčad s tim nazivom već postoji.</response>
    [HttpPost]
    [Authorize(Roles = Roles.Administrator)]
    [ProducesResponseType<MomcadDetaljiDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MomcadDetaljiDto>> Dodaj(MomcadUnosDto unos, CancellationToken ct)
    {
        if (await _momcadi.NazivPostojiAsync(unos.Naziv, null, ct))
        {
            return Konflikt($"Momčad s nazivom \"{unos.Naziv.Trim()}\" već postoji.");
        }

        // Entitet se gradi iz DTO-a; klijent ne može poslati npr. Id ili igrače (zaštita od overpostinga).
        var momcad = new Momcad();
        unos.Primijeni(momcad);
        await _momcadi.AddAsync(momcad, ct);

        var kreirana = await _momcadi.GetDetailsAsync(momcad.Id, ct);
        return CreatedAtRoute(RutaDetalji, new { id = momcad.Id }, kreirana!.ToDetaljiDto());
    }

    /// <summary>Mijenja podatke momčadi (zamjenjuje sve podatke zadanima).</summary>
    /// <param name="id">Id momčadi.</param>
    /// <param name="unos">Novi podaci.</param>
    /// <response code="204">Momčad je izmijenjena.</response>
    /// <response code="400">Neispravni podaci.</response>
    /// <response code="404">Momčad ne postoji.</response>
    /// <response code="409">Naziv već koristi druga momčad.</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Izmijeni(int id, MomcadUnosDto unos, CancellationToken ct)
    {
        var momcad = await _momcadi.GetByIdAsync(id, ct);
        if (momcad is null)
        {
            return NijePronadeno($"Momčad s Id-em {id} ne postoji.");
        }

        if (await _momcadi.NazivPostojiAsync(unos.Naziv, id, ct))
        {
            return Konflikt($"Momčad s nazivom \"{unos.Naziv.Trim()}\" već postoji.");
        }

        unos.Primijeni(momcad);
        await _momcadi.UpdateAsync(momcad, ct);

        return NoContent();
    }

    /// <summary>Briše momčad.</summary>
    /// <remarks>Momčad koja ima igrače ili utakmice ne može se obrisati (najprije obrišite igrače).</remarks>
    /// <param name="id">Id momčadi.</param>
    /// <response code="204">Momčad je obrisana.</response>
    /// <response code="404">Momčad ne postoji.</response>
    /// <response code="409">Momčad ima igrače ili utakmice.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Obrisi(int id, CancellationToken ct)
    {
        return await _momcadi.DeleteAsync(id, ct) switch
        {
            DeleteResult.Deleted => NoContent(),
            DeleteResult.InUse => Konflikt("Momčad ima igrače ili utakmice pa se ne može obrisati."),
            _ => NijePronadeno($"Momčad s Id-em {id} ne postoji.")
        };
    }
}
