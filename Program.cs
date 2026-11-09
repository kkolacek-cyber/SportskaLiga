using System.Globalization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using SportskaLiga.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages()
    // Razor Pages: stranice su u mapi Pages. Poruke ispod ASP.NET koristi kad vrijednost s obrasca ne može povezati s modelom.
    .AddMvcOptions(options =>
    {
        var poruke = options.ModelBindingMessageProvider;
        poruke.SetValueMustNotBeNullAccessor(_ => "Vrijednost je obavezna.");
        poruke.SetMissingBindRequiredValueAccessor(naziv => $"Vrijednost za '{naziv}' je obavezna.");
        poruke.SetAttemptedValueIsInvalidAccessor((vrijednost, naziv) => $"Vrijednost '{vrijednost}' nije ispravna za polje '{naziv}'.");
        poruke.SetUnknownValueIsInvalidAccessor(naziv => $"Vrijednost za '{naziv}' nije ispravna.");
        poruke.SetValueIsInvalidAccessor(vrijednost => $"Vrijednost '{vrijednost}' nije ispravna.");
        poruke.SetValueMustBeANumberAccessor(naziv => $"Polje '{naziv}' mora biti broj.");
    });

// Podatkovni sloj (DbContext i repozitoriji) iz projekta SportskaLiga.Data – isti kao u Adminu i Api-ju.
builder.Services.AddSportskaLigaData(builder.Configuration);

// DataProtection: ključevi za antiforgery tokene obrazaca. Bez trajnog spremišta ključevi nestanu kad se kontejner
// ponovno stvori pa otvoreni obrasci prestanu vrijediti. U Dockeru je KeysPath mapa na imenovanom volumenu.
var zastitaPodataka = builder.Services.AddDataProtection().SetApplicationName("SportskaLiga.Portal");
if (builder.Configuration["DataProtection:KeysPath"] is { Length: > 0 } mapaKljuceva)
{
    zastitaPodataka.PersistKeysToFileSystem(new DirectoryInfo(mapaKljuceva));
}

builder.Services.AddHealthChecks();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Cijeli portal je na hrvatskom: datumi i brojevi uvijek u hr-HR, neovisno o jeziku preglednika.
var hrvatska = new CultureInfo("hr-HR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(hrvatska),
    SupportedCultures = [hrvatska],
    SupportedUICultures = [hrvatska],
    RequestCultureProviders = []
});

// Redoslijed: lokalizacija → usmjeravanje → autorizacija → rute. Portal je javan (nema prijave).
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

// Rute dolaze iz mape Pages (npr. Pages/Tablica/Index.cshtml → /Tablica).
app.MapRazorPages()
   .WithStaticAssets();

// Koristi ga Docker healthcheck.
app.MapHealthChecks("/health");

app.Run();
