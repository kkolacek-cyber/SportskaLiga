using System.Globalization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using SportskaLiga.Admin.Infrastructure;
using SportskaLiga.Admin.Validation;
using SportskaLiga.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    // Sve je zaštićeno prijavom osim akcija označenih s [AllowAnonymous] (prijava, greška);
    // uloge se zahtijevaju na pojedinim kontrolerima/akcijama. Svaki POST traži antiforgery token.
    options.Filters.Add(new AuthorizeFilter());
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());

    // Poruke koje MVC sam generira kad vrijednost s forme ne može povezati s modelom.
    var poruke = options.ModelBindingMessageProvider;
    poruke.SetValueMustNotBeNullAccessor(_ => "Vrijednost je obavezna.");
    poruke.SetMissingBindRequiredValueAccessor(naziv => $"Vrijednost za '{naziv}' je obavezna.");
    poruke.SetAttemptedValueIsInvalidAccessor((vrijednost, naziv) => $"Vrijednost '{vrijednost}' nije ispravna za polje '{naziv}'.");
    poruke.SetUnknownValueIsInvalidAccessor(naziv => $"Vrijednost za '{naziv}' nije ispravna.");
    poruke.SetValueIsInvalidAccessor(vrijednost => $"Vrijednost '{vrijednost}' nije ispravna.");
    poruke.SetValueMustBeANumberAccessor(naziv => $"Polje '{naziv}' mora biti broj.");
});

// Podatkovni sloj (DbContext i repozitoriji) iz projekta SportskaLiga.Data.
builder.Services.AddSportskaLigaData(builder.Configuration);

// Klijentska validacija za vlastiti atribut [NakonDatuma] (vidi mapu Validation i wwwroot/js/nakondatuma.js).
builder.Services.AddSingleton<IValidationAttributeAdapterProvider, LigaValidationAttributeAdapterProvider>();

// ASP.NET Core Identity: korisnici i uloge u istoj bazi; prijava kolačićem.
// Pravila prijave: jedinstven e-mail, zaključavanje nakon 5 pogrešnih lozinki.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddEntityFrameworkStores<SportskaLigaDbContext>()
    .AddDefaultTokenProviders()   // potrebno za reset lozinke koji provodi Administrator
    .AddErrorDescriber<HrIdentityErrorDescriber>();

// Kolačić prijave: kamo preusmjeriti neprijavljenog korisnika (ili onoga bez prava) i koliko dugo prijava vrijedi.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.Name = "SportskaLiga.Admin";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// DataProtection: ključevi kojima se potpisuje kolačić prijave i antiforgery tokeni. Bez trajnog spremišta ključevi
// nestanu kad se kontejner ponovno stvori i svi bi bili odjavljeni. U Dockeru je KeysPath mapa na imenovanom volumenu.
var zastitaPodataka = builder.Services.AddDataProtection().SetApplicationName("SportskaLiga.Admin");
if (builder.Configuration["DataProtection:KeysPath"] is { Length: > 0 } mapaKljuceva)
{
    zastitaPodataka.PersistKeysToFileSystem(new DirectoryInfo(mapaKljuceva));
}

builder.Services.AddHealthChecks();

// --- HTTP pipeline: redoslijed middlewarea je bitan ---
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Cijela administracija je na hrvatskom: datumi i brojevi uvijek u hr-HR.
var hrvatska = new CultureInfo("hr-HR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(hrvatska),
    SupportedCultures = [hrvatska],
    SupportedUICultures = [hrvatska],
    RequestCultureProviders = []
});

app.UseRouting();

// Najprije autentikacija (tko je korisnik, iz kolačića), zatim autorizacija (smije li na traženu stranicu).
app.UseAuthentication();
app.UseAuthorization();

// Statičke datoteke iz wwwroot (CSS, JS) uz kompresiju i verzioniranje.
app.MapStaticAssets();

// Zadana ruta: /Kontroler/Akcija/Id; početna stranica je Home/Index.
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Koristi ga Docker healthcheck.
app.MapHealthChecks("/health");

// Admin je jedina aplikacija koja seeda bazu (i migrira je kod dotnet run; u Dockeru to radi kontejner "migracije").
// Server počinje slušati tek u app.Run(), pa je /health "zdrav" tek nakon seeda – na to čekaju Portal i Api (depends_on).
await app.Services.MigrateAndSeedAsync();

app.Run();
