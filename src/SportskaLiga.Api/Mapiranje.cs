using SportskaLiga.Api.Dtos;
using SportskaLiga.Domain;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Api;

/// <summary>Preslikavanje entiteta u DTO-ove i obrnuto (entiteti se nikad ne vraćaju ni ne primaju izravno).</summary>
internal static class Mapiranje
{
    public static SezonaDto ToDto(this Sezona s) => new(
        s.Id, s.Naziv, s.DatumPocetka, s.DatumZavrsetka, s.Aktivna, s.BodoviZaPobjedu, s.BodoviZaNerijeseno);

    public static TablicaRedDto ToDto(this TablicaRed r) => new(
        r.Pozicija, r.MomcadId, r.Momcad, r.Odigrano, r.Pobjede, r.Nerijeseno, r.Porazi,
        r.GoloviDani, r.GoloviPrimljeni, r.GolRazlika, r.Bodovi,
        string.Concat(r.Forma)); // ['P','N','I'] -> "PNI"

    public static StrijelacDto ToDto(this Strijelac s) => new(s.Pozicija, s.IgracId, s.Igrac, s.MomcadId, s.Momcad, s.Golovi);

    /// <summary>Očekuje učitane navigacije Domacin, Gost i Sudac.</summary>
    public static UtakmicaDto ToDto(this Utakmica u) => new(
        u.Id,
        u.SezonaId,
        u.Kolo,
        UHrvatskoVrijeme(u.Pocetak),
        u.Lokacija,
        new MomcadKratkoDto(u.DomacinId, u.Domacin?.Naziv ?? string.Empty),
        new MomcadKratkoDto(u.GostId, u.Gost?.Naziv ?? string.Empty),
        u.GoloviDomacin,
        u.GoloviGost,
        u.Rezultat,
        u.Status,
        u.Sudac?.PunoIme);

    /// <summary>Očekuje učitane navigacije Sezona, Domacin, Gost, Sudac i Golovi.Igrac.</summary>
    public static UtakmicaDetaljiDto ToDetaljiDto(this Utakmica u) => new(
        u.Id,
        u.SezonaId,
        u.Sezona?.Naziv ?? string.Empty,
        u.Kolo,
        UHrvatskoVrijeme(u.Pocetak),
        u.Lokacija,
        new MomcadKratkoDto(u.DomacinId, u.Domacin?.Naziv ?? string.Empty),
        new MomcadKratkoDto(u.GostId, u.Gost?.Naziv ?? string.Empty),
        u.GoloviDomacin,
        u.GoloviGost,
        u.Rezultat,
        u.Status,
        u.Sudac?.PunoIme,
        u.Napomena,
        u.Golovi
            .OrderBy(g => g.Minuta)
            .Select(g => new GolDto(g.Minuta, g.IgracId, g.Igrac?.PunoIme ?? string.Empty, g.MomcadId, g.Autogol))
            .ToList());

    /// <summary>Očekuje učitane igrače (za broj igrača).</summary>
    public static MomcadDto ToDto(this Momcad m) => new(
        m.Id, m.Naziv, m.Grad, m.Igraliste, m.GodinaOsnutka, m.KontaktEmail, m.Igraci.Count);

    /// <summary>Očekuje učitane navigacije Igraci i Sezone.Sezona.</summary>
    public static MomcadDetaljiDto ToDetaljiDto(this Momcad m) => new(
        m.Id, m.Naziv, m.Grad, m.Igraliste, m.GodinaOsnutka, m.KontaktEmail,
        m.Igraci
            .OrderBy(i => i.BrojDresa)
            .Select(i => new IgracDto(i.Id, i.Ime, i.Prezime, i.BrojDresa, i.Pozicija, i.DatumRodenja))
            .ToList(),
        m.Sezone
            .Select(s => s.Sezona?.Naziv ?? string.Empty)
            .OrderByDescending(n => n)
            .ToList());

    /// <summary>DTO unosa rezultata → domenski ulaz za repozitorij.</summary>
    public static UnosRezultata ToUnos(this RezultatUnosDto dto) => new()
    {
        GoloviDomacin = dto.GoloviDomacin,
        GoloviGost = dto.GoloviGost,
        Golovi = dto.Golovi
            .Select(g => new UnosGola { IgracId = g.IgracId, Minuta = g.Minuta, Autogol = g.Autogol })
            .ToList()
    };

    /// <summary>Upisuje podatke iz DTO-a u (novu ili postojeću) momčad; prazni tekst postaje null.</summary>
    public static void Primijeni(this MomcadUnosDto dto, Momcad m)
    {
        m.Naziv = dto.Naziv.Trim();
        m.Grad = dto.Grad.Trim();
        m.Igraliste = string.IsNullOrWhiteSpace(dto.Igraliste) ? null : dto.Igraliste.Trim();
        m.GodinaOsnutka = dto.GodinaOsnutka;
        m.KontaktEmail = string.IsNullOrWhiteSpace(dto.KontaktEmail) ? null : dto.KontaktEmail.Trim();
    }

    /// <summary>Preslikava cijelu stranicu: stavke postaju DTO-ovi, podaci o straničenju ostaju isti.</summary>
    public static StraniceDto<TDto> ToDto<TEntitet, TDto>(this PagedResult<TEntitet> rezultat, Func<TEntitet, TDto> preslikaj)
    {
        var stranica = rezultat.Map(preslikaj);
        return new StraniceDto<TDto>(stranica.Items, stranica.UkupnoStavki, stranica.Stranica, stranica.VelicinaStranice, stranica.UkupnoStranica);
    }

    /// <summary>
    /// UTC iz baze → hrvatsko vrijeme s pomakom (+01:00 zimi, +02:00 ljeti), da klijent (widget) dobije
    /// i lokalni sat i točan trenutak.
    /// </summary>
    private static DateTimeOffset UHrvatskoVrijeme(DateTime utc)
    {
        var lokalno = DateTime.SpecifyKind(Kalendar.ULokalno(utc), DateTimeKind.Unspecified);
        var pomak = lokalno - DateTime.SpecifyKind(utc, DateTimeKind.Unspecified);
        return new DateTimeOffset(lokalno, pomak);
    }
}
