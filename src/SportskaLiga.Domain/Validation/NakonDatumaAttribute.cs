using System.ComponentModel.DataAnnotations;

namespace SportskaLiga.Domain.Validation;

/// <summary>
/// Vlastiti validacijski atribut: datum ovog svojstva ne smije biti prije datuma drugog svojstva istog objekta.
/// Primjer: <c>[NakonDatuma(nameof(DatumPocetka))] public DateOnly DatumZavrsetka</c>.
/// Radi s <see cref="DateOnly"/> i <see cref="DateTime"/>. (Klijentsku stranu dodaje web aplikacija – adapter i JS.)
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class NakonDatumaAttribute : ValidationAttribute
{
    public NakonDatumaAttribute(string drugoSvojstvo)
        : base("Polje {0} ne smije biti prije polja {1}.")
    {
        DrugoSvojstvo = drugoSvojstvo;
    }

    /// <summary>Naziv svojstva s kojim se uspoređuje (npr. "DatumPocetka").</summary>
    public string DrugoSvojstvo { get; }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var svojstvo = validationContext.ObjectType.GetProperty(DrugoSvojstvo);
        if (svojstvo is null)
        {
            return new ValidationResult($"Nepoznato svojstvo '{DrugoSvojstvo}'.");
        }

        var drugo = svojstvo.GetValue(validationContext.ObjectInstance);

        // Prazne vrijednosti provjerava [Required]; ovdje uspoređujemo samo kad su oba datuma zadana.
        var prije = (value, drugo) switch
        {
            (DateOnly ovaj, DateOnly onaj) => ovaj < onaj,
            (DateTime ovaj, DateTime onaj) => ovaj < onaj,
            _ => false
        };

        if (!prije)
        {
            return ValidationResult.Success;
        }

        var clanovi = validationContext.MemberName is { } ime ? new[] { ime } : null;
        return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), clanovi);
    }

    // {0} = naziv ovog polja (iz [Display]), {1} = naziv drugog svojstva.
    public override string FormatErrorMessage(string name)
        => string.Format(ErrorMessageString, name, DrugoSvojstvo);
}
