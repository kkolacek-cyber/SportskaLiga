using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SportskaLiga.Admin.TagHelpers;

/// <summary>
/// <c>&lt;forma niz="@red.Forma" /&gt;</c> – zadnjih 5 rezultata momčadi kao obojeni kružići:
/// P (pobjeda, zeleno), N (neriješeno, sivo), I (izgubljeno, crveno).
/// </summary>
[HtmlTargetElement("forma", TagStructure = TagStructure.WithoutEndTag)]
public class FormaTagHelper : TagHelper
{
    public IEnumerable<char>? Niz { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", "forma");

        foreach (var ishod in Niz ?? [])
        {
            var (klasa, naslov) = ishod switch
            {
                'P' => ("forma-p", "Pobjeda"),
                'N' => ("forma-n", "Neriješeno"),
                _ => ("forma-i", "Poraz")
            };

            // AppendHtml s fiksnim tekstom; ishod je uvijek jedan od tri poznata znaka, pa nema rizika umetanja HTML-a.
            output.Content.AppendHtml($"<span class=\"{klasa}\" title=\"{naslov}\">{ishod}</span>");
        }
    }
}
