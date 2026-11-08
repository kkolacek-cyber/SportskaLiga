using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.Localization;
using SportskaLiga.Domain.Validation;

namespace SportskaLiga.Admin.Validation;

/// <summary>Dodaje adapter za vlastiti atribut iz Domain projekta; za sve ostale koristi zadani provider.</summary>
public class LigaValidationAttributeAdapterProvider : IValidationAttributeAdapterProvider
{
    private readonly ValidationAttributeAdapterProvider _zadani = new();

    // ASP.NET pita za adapter svakog validacijskog atributa; za naš atribut vraća vlastiti, za ostale zadani.
    public IAttributeAdapter? GetAttributeAdapter(ValidationAttribute attribute, IStringLocalizer? stringLocalizer)
        => attribute is NakonDatumaAttribute nakonDatuma
            ? new NakonDatumaAttributeAdapter(nakonDatuma, stringLocalizer)
            : _zadani.GetAttributeAdapter(attribute, stringLocalizer);
}
