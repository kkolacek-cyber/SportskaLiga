using Microsoft.AspNetCore.Razor.TagHelpers;
using SportskaLiga.Domain.Enums;

namespace SportskaLiga.Admin.TagHelpers;

/// <summary>
/// <c>&lt;status-badge value="@utakmica.Status" /&gt;</c> – obojena oznaka statusa utakmice ili pozicije igrača.
/// S <c>ceka-rezultat="true"</c> zakazana utakmica kojoj je termin prošao prikazuje se kao "Čeka rezultat".
/// </summary>
[HtmlTargetElement("status-badge", TagStructure = TagStructure.WithoutEndTag)]
public class StatusBadgeTagHelper : TagHelper
{
    // Vrijednost koja se prikazuje: StatusUtakmice ili Pozicija.
    public Enum? Value { get; set; }

    // Zakazana utakmica kojoj je termin prošao, a rezultat još nije upisan.
    public bool CekaRezultat { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (Value is null)
        {
            output.SuppressOutput();
            return;
        }

        // Boja i tekst dolaze iz vrijednosti enuma (tekst iz [Display(Name)]), osim za utakmicu koja čeka rezultat.
        var (boja, tekst) = CekaRezultat && Value is StatusUtakmice.Zakazana
            ? ("warning", "Čeka rezultat")
            : (Boja(Value), Value.NazivZaPrikaz());

        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        var klase = $"badge text-bg-{boja}";
        // Zadržava klase navedene u pogledu (npr. fs-6) uz klase same oznake.
        if (output.Attributes.TryGetAttribute("class", out var postojece))
        {
            klase += " " + postojece.Value;
        }

        output.Attributes.SetAttribute("class", klase);
        output.Content.SetContent(tekst);
    }

    // Bootstrap boja oznake: zeleno = gotovo, plavo = u planu, crveno = problem, sivo = neutralno.
    private static string Boja(Enum vrijednost) => vrijednost switch
    {
        StatusUtakmice.Zakazana => "info",
        StatusUtakmice.Odigrana => "success",
        StatusUtakmice.Odgodena => "danger",

        StatusPrijave.NaCekanju => "warning",
        StatusPrijave.Odobrena => "success",
        StatusPrijave.Odbijena => "danger",

        Pozicija.Vratar => "warning",
        Pozicija.Obrana => "primary",
        Pozicija.Vezni => "success",
        Pozicija.Napad => "danger",

        _ => "secondary"
    };
}
