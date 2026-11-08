using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportskaLiga.Admin.Models;
using SportskaLiga.Domain;
using SportskaLiga.Domain.Enums;
using SportskaLiga.Domain.Interfaces;
using SportskaLiga.Domain.Models;

namespace SportskaLiga.Admin.Controllers;

/// <summary>Nadzorna ploča aktivne sezone, podaci za grafove (Fetch API) i stranica greške.</summary>
[Authorize(Roles = Roles.Osoblje)]
public class HomeController : Controller
{
    private readonly ISezonaRepository _sezone;
    private readonly IUtakmicaRepository _utakmice;

    public HomeController(ISezonaRepository sezone, IUtakmicaRepository utakmice)
    {
        _sezone = sezone;
        _utakmice = utakmice;
    }

    /// <summary>Brojevi, tablica, strijelci i utakmice koje traže pažnju; grafove dohvaća dashboard.js iz <see cref="Statistika"/>.</summary>
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var sezona = await _sezone.GetAktivnaAsync(ct);
        if (sezona is null)
        {
            return View(new DashboardViewModel());
        }

        // Delegat vidi samo svoje utakmice koje čekaju rezultat; administrator sve.
        var jeDelegat = !User.IsInRole(Roles.Administrator);
        var delegatId = jeDelegat ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;

        var poStatusu = await _utakmice.BrojPoStatusuAsync(sezona.Id, ct);
        var tablica = await _utakmice.GetTablicaAsync(sezona.Id, ct);

        var zaUnos = await _utakmice.SearchAsync(new UtakmicaFilter
        {
            SezonaId = sezona.Id,
            DelegatId = delegatId,
            SamoCekajuRezultat = true,
            VelicinaStranice = 10
        }, ct);

        var sljedece = await _utakmice.SearchAsync(new UtakmicaFilter
        {
            SezonaId = sezona.Id,
            Status = StatusUtakmice.Zakazana,
            DelegatId = delegatId,
            VelicinaStranice = 50
        }, ct);

        return View(new DashboardViewModel
        {
            Sezona = sezona,
            Odigrano = poStatusu.GetValueOrDefault(StatusUtakmice.Odigrana),
            Zakazano = poStatusu.GetValueOrDefault(StatusUtakmice.Zakazana),
            Odgodeno = poStatusu.GetValueOrDefault(StatusUtakmice.Odgodena),
            CekajuRezultat = zaUnos.UkupnoStavki,
            UkupnoGolova = tablica.Sum(r => r.GoloviDani),
            Tablica = tablica,
            Strijelci = await _utakmice.GetStrijelciAsync(sezona.Id, 5, ct),
            ZaUnos = zaUnos.Items,
            // Iz zakazanih izbacujemo one kojima je termin prošao (one su već u "čekaju rezultat").
            Sljedece = sljedece.Items.Where(u => u.Pocetak >= DateTime.UtcNow).Take(5).ToList(),
            JeDelegat = jeDelegat
        });
    }

    /// <summary>JSON za grafove (Chart.js): bodovi po momčadi i golovi po kolu u zadanoj (ili aktivnoj) sezoni.</summary>
    [HttpGet]
    public async Task<IActionResult> Statistika(int? sezonaId, CancellationToken ct)
    {
        var id = sezonaId ?? (await _sezone.GetAktivnaAsync(ct))?.Id;
        if (id is null)
        {
            return NotFound();
        }

        var tablica = await _utakmice.GetTablicaAsync(id.Value, ct);
        var poKolu = await _utakmice.GoloviPoKoluAsync(id.Value, ct);

        // Anonimni objekt se pretvara u JSON koji čita wwwroot/js/dashboard.js.
        return Json(new
        {
            bodovi = tablica.Select(r => new { momcad = r.Momcad, bodovi = r.Bodovi }),
            goloviPoKolu = poKolu.OrderBy(k => k.Key).Select(k => new { kolo = k.Key, golovi = k.Value })
        });
    }

    [AllowAnonymous]
    // Stranica greške mora biti dostupna i neprijavljenima; ResponseCache isključuje njezino keširanje.
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
