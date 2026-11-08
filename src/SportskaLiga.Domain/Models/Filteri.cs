using SportskaLiga.Domain.Enums;

namespace SportskaLiga.Domain.Models;

/// <summary>Kriteriji pretrage utakmica (raspored i rezultati na portalu, popis u administraciji, API).</summary>
public class UtakmicaFilter
{
    public int? SezonaId { get; set; }

    public int? Kolo { get; set; }

    /// <summary>Utakmice u kojima momčad igra kao domaćin ili gost.</summary>
    public int? MomcadId { get; set; }

    public StatusUtakmice? Status { get; set; }

    /// <summary>Samo utakmice dodijeljene ovom delegatu (Identity Id).</summary>
    public string? DelegatId { get; set; }

    /// <summary>Samo utakmice kojima je termin prošao, a rezultat još nije upisan (posao za delegata).</summary>
    public bool SamoCekajuRezultat { get; set; }

    /// <summary>true = najnovije prvo (rezultati); false = kronološki (raspored).</summary>
    public bool NajnovijePrvo { get; set; }

    public int Stranica { get; set; } = 1;

    public int VelicinaStranice { get; set; } = 20;
}

/// <summary>Kriteriji pretrage igrača.</summary>
public class IgracFilter
{
    public int? MomcadId { get; set; }

    /// <summary>Traži se u imenu i prezimenu (bez razlike velikih/malih slova).</summary>
    public string? Tekst { get; set; }

    public Pozicija? Pozicija { get; set; }

    public int Stranica { get; set; } = 1;

    public int VelicinaStranice { get; set; } = 20;
}
