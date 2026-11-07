namespace SportskaLiga.Domain.Models;

/// <summary>Jedan red tablice lige. Ne sprema se u bazu – računa se iz odigranih utakmica (vidi Logika/IzracunTablice).</summary>
public class TablicaRed
{
    public int Pozicija { get; set; }

    public int MomcadId { get; set; }

    public string Momcad { get; set; } = string.Empty;

    public int Odigrano { get; set; }

    public int Pobjede { get; set; }

    public int Nerijeseno { get; set; }

    public int Porazi { get; set; }

    public int GoloviDani { get; set; }

    public int GoloviPrimljeni { get; set; }

    public int GolRazlika => GoloviDani - GoloviPrimljeni;

    public int Bodovi { get; set; }

    /// <summary>Zadnjih (najviše) 5 rezultata, od najstarijeg: 'P' pobjeda, 'N' neriješeno, 'I' izgubljeno.</summary>
    public List<char> Forma { get; set; } = [];
}
