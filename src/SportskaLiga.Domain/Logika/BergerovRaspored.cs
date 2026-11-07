namespace SportskaLiga.Domain.Logika;

/// <summary>Jedan par u rasporedu: u kojem kolu, tko je domaćin, tko gost.</summary>
public record ParUtakmice(int Kolo, int DomacinId, int GostId);

/// <summary>
/// Raspored "svatko sa svakim" (round-robin) metodom kruga – Bergerove tablice.
///
/// Za 6 momčadi [A B C D E F]:
///   - u kolu se sparuju prvi i zadnji, drugi i predzadnji...   A-F, B-E, C-D
///   - zatim prva momčad stoji, a ostale se "zavrte" za jedno mjesto:  [A F B C D E]
///   - nakon n-1 kola svatko je igrao sa svakim točno jednom.
/// Neparan broj momčadi: dodaje se prazno mjesto (null); tko je uparen s njim, to je kolo slobodan.
/// </summary>
public static class BergerovRaspored
{
    public static IReadOnlyList<ParUtakmice> Generiraj(IReadOnlyList<int> momcadIds, bool dvostrukiKrug)
    {
        if (momcadIds.Count < 2)
        {
            return [];
        }

        var lista = momcadIds.Select(id => (int?)id).ToList();
        if (lista.Count % 2 == 1)
        {
            lista.Add(null);   // slobodan termin
        }

        var n = lista.Count;
        var brojKola = n - 1;
        var parovi = new List<ParUtakmice>();

        for (var kolo = 1; kolo <= brojKola; kolo++)
        {
            for (var i = 0; i < n / 2; i++)
            {
                var a = lista[i];
                var b = lista[n - 1 - i];
                if (a is null || b is null)
                {
                    continue;   // jedna momčad je ovo kolo slobodna
                }

                // Izmjenjivanje domaćina, da nitko ne igra stalno doma ili stalno vani.
                var zamijeni = i == 0 ? kolo % 2 == 0 : i % 2 == 1;
                parovi.Add(zamijeni
                    ? new ParUtakmice(kolo, b.Value, a.Value)
                    : new ParUtakmice(kolo, a.Value, b.Value));
            }

            // Rotacija: prvi ostaje na mjestu, zadnji prelazi na drugo mjesto.
            var zadnji = lista[n - 1];
            lista.RemoveAt(n - 1);
            lista.Insert(1, zadnji);
        }

        // Uzvratni dio: isti parovi sa zamijenjenim domaćinom i gostom; kola se nastavljaju.
        if (dvostrukiKrug)
        {
            var uzvrat = parovi
                .Select(p => new ParUtakmice(p.Kolo + brojKola, p.GostId, p.DomacinId))
                .ToList();
            parovi.AddRange(uzvrat);
        }

        return parovi;
    }
}
