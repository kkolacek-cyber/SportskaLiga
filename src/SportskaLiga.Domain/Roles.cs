namespace SportskaLiga.Domain;

/// <summary>Nazivi uloga (ASP.NET Core Identity) koje koriste Admin, Api i seed.</summary>
public static class Roles
{
    /// <summary>Vodi ligu: momčadi, igrači, sezone, suci, raspored, korisnici.</summary>
    public const string Administrator = "Administrator";

    /// <summary>Delegat utakmice: upisuje rezultat i strijelce utakmica koje su mu dodijeljene.</summary>
    public const string Delegat = "Delegat";

    /// <summary>Za <c>[Authorize(Roles = Roles.Osoblje)]</c> – obje uloge.</summary>
    public const string Osoblje = Administrator + "," + Delegat;
}
