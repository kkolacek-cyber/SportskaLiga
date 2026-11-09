using System.ComponentModel.DataAnnotations;
using SportskaLiga.Domain.Enums;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Admin.Models;

/// <summary>Popis prijava momčadi s filtrom statusa.</summary>
public class PrijaveIndexViewModel
{
    /// <summary>null = sve prijave.</summary>
    public StatusPrijave? Status { get; init; }

    public PagedResult<PrijavaMomcadi> Rezultat { get; init; } = new();
}

/// <summary>Detalji prijave i obrazac za odgovor (odobri / odbij).</summary>
public class PrijavaObradaViewModel
{
    public required PrijavaMomcadi Prijava { get; init; }

    [StringLength(500, ErrorMessage = "Odgovor može imati najviše {1} znakova.")]
    [Display(Name = "Odgovor podnositelju")]
    public string? Odgovor { get; init; }
}
