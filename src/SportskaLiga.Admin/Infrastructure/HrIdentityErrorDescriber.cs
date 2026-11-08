using Microsoft.AspNetCore.Identity;

namespace SportskaLiga.Admin.Infrastructure;

/// <summary>Hrvatski opisi pogrešaka ASP.NET Core Identityja (lozinke, duplikati) za forme administracije.</summary>
public class HrIdentityErrorDescriber : IdentityErrorDescriber
{
    // Identity po zadanom vraća engleske poruke; ovdje se zamjenjuju hrvatskima (registrirano u Program.cs preko AddErrorDescriber).
    public override IdentityError DefaultError()
        => new() { Code = nameof(DefaultError), Description = "Dogodila se nepoznata pogreška." };

    public override IdentityError PasswordTooShort(int length)
        => new() { Code = nameof(PasswordTooShort), Description = $"Lozinka mora imati najmanje {length} znakova." };

    public override IdentityError PasswordRequiresDigit()
        => new() { Code = nameof(PasswordRequiresDigit), Description = "Lozinka mora sadržavati barem jednu znamenku (0-9)." };

    public override IdentityError PasswordRequiresLower()
        => new() { Code = nameof(PasswordRequiresLower), Description = "Lozinka mora sadržavati barem jedno malo slovo (a-z)." };

    public override IdentityError PasswordRequiresUpper()
        => new() { Code = nameof(PasswordRequiresUpper), Description = "Lozinka mora sadržavati barem jedno veliko slovo (A-Z)." };

    public override IdentityError PasswordRequiresNonAlphanumeric()
        => new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Lozinka mora sadržavati barem jedan poseban znak (npr. ! ? # $)." };

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
        => new() { Code = nameof(PasswordRequiresUniqueChars), Description = $"Lozinka mora sadržavati barem {uniqueChars} različitih znakova." };

    public override IdentityError DuplicateUserName(string userName)
        => new() { Code = nameof(DuplicateUserName), Description = $"Korisnik s e-mailom '{userName}' već postoji." };

    public override IdentityError DuplicateEmail(string email)
        => new() { Code = nameof(DuplicateEmail), Description = $"Korisnik s e-mailom '{email}' već postoji." };

    public override IdentityError InvalidEmail(string? email)
        => new() { Code = nameof(InvalidEmail), Description = $"E-mail '{email}' nije ispravan." };

    public override IdentityError InvalidUserName(string? userName)
        => new() { Code = nameof(InvalidUserName), Description = $"Korisničko ime '{userName}' nije ispravno." };

    public override IdentityError PasswordMismatch()
        => new() { Code = nameof(PasswordMismatch), Description = "Trenutna lozinka nije točna." };
}
