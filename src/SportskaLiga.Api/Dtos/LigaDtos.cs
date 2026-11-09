using System.ComponentModel.DataAnnotations;
using SportskaLiga.Domain.Enums;

namespace SportskaLiga.Api.Dtos;

/// <summary>Jedna stranica rezultata s podacima za straničenje.</summary>
public record StraniceDto<T>(IReadOnlyList<T> Stavke, int UkupnoStavki, int Stranica, int VelicinaStranice, int UkupnoStranica);

/// <summary>Sezona lige.</summary>
/// <param name="Aktivna">Tekuća sezona; nju API koristi kad sezona nije zadana.</param>
public record SezonaDto(
    int Id,
    string Naziv,
    DateOnly DatumPocetka,
    DateOnly DatumZavrsetka,
    bool Aktivna,
    int BodoviZaPobjedu,
    int BodoviZaNerijeseno);

/// <summary>Tablica lige za jednu sezonu.</summary>
public record TablicaDto(SezonaDto Sezona, IReadOnlyList<TablicaRedDto> Redovi);

/// <summary>Red tablice.</summary>
/// <param name="Forma">Zadnjih (najviše) 5 rezultata, od najstarijeg: P pobjeda, N neriješeno, I izgubljeno (npr. "PPNIP").</param>
public record TablicaRedDto(
    int Pozicija,
    int MomcadId,
    string Momcad,
    int Odigrano,
    int Pobjede,
    int Nerijeseno,
    int Porazi,
    int GoloviDani,
    int GoloviPrimljeni,
    int GolRazlika,
    int Bodovi,
    string Forma);

/// <summary>Lista strijelaca sezone (autogolovi se ne broje).</summary>
public record StrijelciDto(SezonaDto Sezona, IReadOnlyList<StrijelacDto> Strijelci);

/// <summary>Red liste strijelaca; igrači s istim brojem golova dijele poziciju.</summary>
public record StrijelacDto(int Pozicija, int IgracId, string Igrac, int MomcadId, string Momcad, int Golovi);

/// <summary>Kratki podatak o momčadi unutar utakmice.</summary>
public record MomcadKratkoDto(int Id, string Naziv);

/// <summary>Utakmica u popisu (raspored, rezultati).</summary>
/// <param name="Pocetak">Početak po hrvatskom vremenu, s pomakom od UTC-a (npr. 2026-11-14T15:00:00+01:00).</param>
/// <param name="Rezultat">"3 : 1" za odigranu utakmicu, inače "- : -".</param>
public record UtakmicaDto(
    int Id,
    int SezonaId,
    int Kolo,
    DateTimeOffset Pocetak,
    string? Lokacija,
    MomcadKratkoDto Domacin,
    MomcadKratkoDto Gost,
    int? GoloviDomacin,
    int? GoloviGost,
    string Rezultat,
    StatusUtakmice Status,
    string? Sudac);

/// <summary>Utakmica sa strijelcima.</summary>
public record UtakmicaDetaljiDto(
    int Id,
    int SezonaId,
    string Sezona,
    int Kolo,
    DateTimeOffset Pocetak,
    string? Lokacija,
    MomcadKratkoDto Domacin,
    MomcadKratkoDto Gost,
    int? GoloviDomacin,
    int? GoloviGost,
    string Rezultat,
    StatusUtakmice Status,
    string? Sudac,
    string? Napomena,
    IReadOnlyList<GolDto> Golovi);

/// <summary>Jedan gol.</summary>
/// <param name="MomcadId">Momčad kojoj se gol pribraja (kod autogola to je protivnik strijelca).</param>
public record GolDto(int Minuta, int IgracId, string Igrac, int MomcadId, bool Autogol);

/// <summary>Filtri i straničenje za popis utakmica (raspored, rezultati).</summary>
public class UtakmiceUpitDto
{
    /// <summary>Id sezone; ako se izostavi, koristi se aktivna sezona.</summary>
    public int? SezonaId { get; set; }

    /// <summary>Samo zadano kolo.</summary>
    [Range(1, 100, ErrorMessage = "Kolo mora biti između {1} i {2}.")]
    public int? Kolo { get; set; }

    /// <summary>Samo utakmice ove momčadi (kao domaćin ili gost).</summary>
    public int? MomcadId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Stranica mora biti 1 ili veća.")]
    public int Stranica { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Veličina stranice mora biti između {1} i {2}.")]
    public int VelicinaStranice { get; set; } = 20;
}

/// <summary>Rezultat utakmice sa strijelcima (PUT /api/utakmice/{id}/rezultat).</summary>
public class RezultatUnosDto
{
    /// <example>2</example>
    [Range(0, 99, ErrorMessage = "Broj golova mora biti između {1} i {2}.")]
    public int GoloviDomacin { get; set; }

    /// <example>1</example>
    [Range(0, 99, ErrorMessage = "Broj golova mora biti između {1} i {2}.")]
    public int GoloviGost { get; set; }

    /// <summary>Po jedan zapis za svaki gol; broj zapisa mora biti jednak zbroju golova.</summary>
    public List<GolUnosDto> Golovi { get; set; } = [];
}

/// <summary>Jedan gol u unosu rezultata.</summary>
public class GolUnosDto
{
    /// <summary>Strijelac; mora igrati za domaćina ili gosta.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Odaberite strijelca.")]
    public int IgracId { get; set; }

    [Range(1, 120, ErrorMessage = "Minuta mora biti između {1} i {2}.")]
    public int Minuta { get; set; }

    /// <summary>Autogol se pribraja protivniku i ne broji se strijelcu.</summary>
    public bool Autogol { get; set; }
}
