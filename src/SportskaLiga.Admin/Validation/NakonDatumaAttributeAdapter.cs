using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;
using SportskaLiga.Domain.Validation;

namespace SportskaLiga.Admin.Validation;

/// <summary>
/// Prevodi <see cref="NakonDatumaAttribute"/> u <c>data-val-*</c> HTML atribute za jQuery Unobtrusive Validation;
/// pravilo u pregledniku provodi <c>wwwroot/js/nakondatuma.js</c>. Na poslužitelju atribut radi i bez ovog adaptera.
/// </summary>
public class NakonDatumaAttributeAdapter : AttributeAdapterBase<NakonDatumaAttribute>
{
    public NakonDatumaAttributeAdapter(NakonDatumaAttribute attribute, IStringLocalizer? stringLocalizer)
        : base(attribute, stringLocalizer)
    {
    }

    public override void AddValidation(ClientModelValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Uključena validacija, poruka i ime polja s kojim se uspoređuje (npr. "DatumPocetka").
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-nakondatuma", GetErrorMessage(context));
        MergeAttribute(context.Attributes, "data-val-nakondatuma-drugo", Attribute.DrugoSvojstvo);
    }

    public override string GetErrorMessage(ModelValidationContextBase validationContext)
    {
        ArgumentNullException.ThrowIfNull(validationContext);

        return GetErrorMessage(validationContext.ModelMetadata, validationContext.ModelMetadata.GetDisplayName());
    }
}
