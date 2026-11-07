using System.ComponentModel.DataAnnotations;

namespace SportskaLiga.Domain.Models;

/// <summary>Ulaz za generiranje rasporeda sezone ("svatko sa svakim").</summary>
public class GenerirajRasporedZahtjev
{
    [Range(1, int.MaxValue, ErrorMessage = "Odaberite sezonu.")]
    [Display(Name = "Sezona")]
    public int SezonaId { get; set; }

    /// <summary>Datum i vrijeme prvog kola po hrvatskom vremenu; sva kola igraju se u isto doba dana.</summary>
    [Display(Name = "Prvo kolo")]
    public DateTime PrviTermin { get; set; }

    [Range(1, 30, ErrorMessage = "Razmak mora biti između {1} i {2} dana.")]
    [Display(Name = "Razmak između kola (dana)")]
    public int RazmakDana { get; set; } = 7;

    /// <summary>Dvostruki krug: svaka momčad igra sa svakom doma i u gostima.</summary>
    [Display(Name = "Dvostruki krug (jesen i proljeće)")]
    public bool DvostrukiKrug { get; set; } = true;
}

/// <summary>Ulaz za upis rezultata utakmice sa strijelcima.</summary>
public class UnosRezultata
{
    [Range(0, 99, ErrorMessage = "Broj golova mora biti između {1} i {2}.")]
    [Display(Name = "Golovi domaćina")]
    public int GoloviDomacin { get; set; }

    [Range(0, 99, ErrorMessage = "Broj golova mora biti između {1} i {2}.")]
    [Display(Name = "Golovi gosta")]
    public int GoloviGost { get; set; }

    /// <summary>Po jedan zapis za svaki gol; broj zapisa mora biti jednak zbroju golova.</summary>
    public List<UnosGola> Golovi { get; set; } = [];
}

/// <summary>Jedan gol u <see cref="UnosRezultata"/>. Momčad kojoj se gol pribraja izračuna se iz igrača i oznake autogola.</summary>
public class UnosGola
{
    [Range(1, int.MaxValue, ErrorMessage = "Odaberite strijelca.")]
    [Display(Name = "Strijelac")]
    public int IgracId { get; set; }

    [Range(1, 120, ErrorMessage = "Minuta mora biti između {1} i {2}.")]
    [Display(Name = "Minuta")]
    public int Minuta { get; set; }

    [Display(Name = "Autogol")]
    public bool Autogol { get; set; }
}
