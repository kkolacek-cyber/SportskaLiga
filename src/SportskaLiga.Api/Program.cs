using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SportskaLiga.Api.Auth;
using SportskaLiga.Api.Infrastructure;
using SportskaLiga.Api.OpenApi;
using SportskaLiga.Data;

const string PolitikaWidget = "Widget";

var builder = WebApplication.CreateBuilder(args);

// --- Podaci (isti repozitoriji kao portal i administracija) ---
builder.Services.AddSportskaLigaData(builder.Configuration);

// --- Autentikacija: JWT za iste Identity korisnike kao u administraciji ---
builder.Services.AddOptions<JwtOptions>()
    .BindConfiguration(JwtOptions.Section)
    .ValidateDataAnnotations()
    .ValidateOnStart(); // bez ispravnog Jwt:Key API se ne podiže
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

// Identity bez kolačića: API treba samo UserManager (korisnik i uloge) i SignInManager (provjera lozinke + zaključavanje).
builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<SportskaLigaDbContext>()
    .AddSignInManager();

// JwtBearer: zaštićeni zahtjev mora nositi zaglavlje "Authorization: Bearer {token}".
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

// Parametri provjere tokena čitaju se iz konfiguracije (ključ nikad nije u kodu).
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((bearer, jwt) =>
    {
        var opcije = jwt.Value;

        bearer.MapInboundClaims = false; // oznake ostaju "sub", "role", "name" (kako ih izdaje JwtTokenService)
        bearer.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = opcije.Issuer,
            ValidateAudience = true,
            ValidAudience = opcije.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opcije.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = JwtTokenService.OznakaIme,
            RoleClaimType = JwtTokenService.OznakaUloga
        };
    });

builder.Services.AddAuthorization();

// --- CORS: widget na tuđoj stranici (web stranica kluba) smije ČITATI podatke lige ---
// Dopušten je samo GET; izmjene (POST/PUT/DELETE) iz preglednika s drugih domena su blokirane.
builder.Services.AddCors(options => options.AddPolicy(PolitikaWidget, politika => politika
    .AllowAnyOrigin()
    .WithMethods("GET")
    .AllowAnyHeader()));

// --- MVC / JSON ---
builder.Services.AddControllers()
    .AddMvcOptions(options =>
    {
        // API govori samo JSON.
        options.OutputFormatters.RemoveType<StringOutputFormatter>();
        foreach (var formater in options.OutputFormatters.OfType<SystemTextJsonOutputFormatter>())
        {
            formater.SupportedMediaTypes.Remove("text/json");
        }

        // Poruke koje MVC sam generira kad vrijednost iz upita/tijela ne može povezati s modelom.
        var poruke = options.ModelBindingMessageProvider;
        poruke.SetValueMustNotBeNullAccessor(_ => "Vrijednost je obavezna.");
        poruke.SetMissingBindRequiredValueAccessor(naziv => $"Vrijednost za '{naziv}' je obavezna.");
        poruke.SetAttemptedValueIsInvalidAccessor((vrijednost, naziv) => $"Vrijednost '{vrijednost}' nije ispravna za polje '{naziv}'.");
        poruke.SetUnknownValueIsInvalidAccessor(naziv => $"Vrijednost za '{naziv}' nije ispravna.");
        poruke.SetValueIsInvalidAccessor(vrijednost => $"Vrijednost '{vrijednost}' nije ispravna.");
        poruke.SetValueMustBeANumberAccessor(naziv => $"Polje '{naziv}' mora biti broj.");
        poruke.SetNonPropertyAttemptedValueIsInvalidAccessor(vrijednost => $"Vrijednost '{vrijednost}' nije ispravna.");
        poruke.SetNonPropertyUnknownValueIsInvalidAccessor(() => "Vrijednost nije ispravna.");
        poruke.SetNonPropertyValueMustBeANumberAccessor(() => "Vrijednost mora biti broj.");
    })
    .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = ApiValidacija.OdgovorNeispravanModel)
    .AddJsonOptions(options =>
    {
        // Enumi se u JSON-u pišu imenom ("Odigrana"), ne brojem.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Isto za generator OpenAPI dokumenta, da shema opisuje enume kao tekst.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Greške (400/401/403/404/409/500) su uvijek application/problem+json.
builder.Services.AddProblemDetails();

// OpenAPI dokument (/openapi/v1.json) + Swagger UI (/swagger).
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<LigaDocumentTransformer>();
    options.AddOperationTransformer<LigaOperationTransformer>();
});
builder.Services.AddHealthChecks();

// --- HTTP pipeline: redoslijed middlewarea je bitan ---
var app = builder.Build();

app.UseExceptionHandler();   // neuhvaćena iznimka → 500 problem+json, bez stack tracea
app.UseStatusCodePages();    // prazni 401/403/404 dobivaju tijelo problem+json

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "SportskaLiga API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "SportskaLiga API";
    options.EnablePersistAuthorization();
    options.DisplayRequestDuration();
});
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.UseHttpsRedirection();

// wwwroot: skripta widgeta (liga-widget.js) i demo stranica kluba (widget.html).
app.UseStaticFiles();

app.UseCors(PolitikaWidget);

// Najprije autentikacija (tko je pozivatelj), zatim autorizacija (smije li to).
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
