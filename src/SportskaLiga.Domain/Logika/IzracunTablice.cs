using SportskaLiga.Domain.Enums;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Domain.Logika;

/// <summary>
/// Izračun tablice lige – jedino mjesto s pravilima bodovanja i poretka.
/// "Čista" funkcija: dobije momčadi i utakmice, vrati tablicu; ne zna ni za bazu ni za HTTP.
/// </summary>
public static class IzracunTablice
{
    private const int DuljinaForme = 5;

    public static IReadOnlyList<TablicaRed> Izracunaj(
        IEnumerable<Momcad> momcadi,
        IEnumerable<Utakmica> utakmice,
        int bodoviZaPobjedu,
        int bodoviZaNerijeseno)
    {
        // 1) Svaka prijavljena momčad dobije prazan red – i one koje još nisu igrale.
        var redovi = momcadi.ToDictionary(
            m => m.Id,
            m => new TablicaRed { MomcadId = m.Id, Momcad = m.Naziv });

        // 2) Odigrane utakmice kronološki (zbog forme); svaka se upisuje u redove obje momčadi.
        var odigrane = utakmice
            .Where(u => u.Status == StatusUtakmice.Odigrana)
            .OrderBy(u => u.Pocetak);

        foreach (var u in odigrane)
        {
            if (u.GoloviDomacin is not int gd || u.GoloviGost is not int gg)
            {
                continue;   // nema rezultata
            }

            if (!redovi.TryGetValue(u.DomacinId, out var domacin) || !redovi.TryGetValue(u.GostId, out var gost))
            {
                continue;   // momčad nije prijavljena u sezonu
            }

            Upisi(domacin, dani: gd, primljeni: gg);
            Upisi(gost, dani: gg, primljeni: gd);
        }

        // 3) Bodovi prema pravilima sezone.
        foreach (var r in redovi.Values)
        {
            r.Bodovi = r.Pobjede * bodoviZaPobjedu + r.Nerijeseno * bodoviZaNerijeseno;
        }

        // 4) Poredak: bodovi → gol-razlika → više postignutih golova → abecedno.
        var poredak = redovi.Values
            .OrderByDescending(r => r.Bodovi)
            .ThenByDescending(r => r.GolRazlika)
            .ThenByDescending(r => r.GoloviDani)
            .ThenBy(r => r.Momcad)
            .ToList();

        for (var i = 0; i < poredak.Count; i++)
        {
            poredak[i].Pozicija = i + 1;
        }

        return poredak;
    }

    // Upisuje jednu utakmicu u red jedne momčadi.
    private static void Upisi(TablicaRed red, int dani, int primljeni)
    {
        red.Odigrano++;
        red.GoloviDani += dani;
        red.GoloviPrimljeni += primljeni;

        char ishod;
        if (dani > primljeni)
        {
            red.Pobjede++;
            ishod = 'P';
        }
        else if (dani < primljeni)
        {
            red.Porazi++;
            ishod = 'I';
        }
        else
        {
            red.Nerijeseno++;
            ishod = 'N';
        }

        red.Forma.Add(ishod);
        if (red.Forma.Count > DuljinaForme)
        {
            red.Forma.RemoveAt(0);   // čuvamo samo zadnjih 5
        }
    }
}
