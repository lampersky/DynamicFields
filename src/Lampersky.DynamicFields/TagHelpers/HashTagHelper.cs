using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DynamicFields.TagHelpers;

[HtmlTargetElement("script", Attributes = DataHashAttributeName)]
[HtmlTargetElement("style", Attributes = DataHashAttributeName)]
public class HashTagHelper : TagHelper
{
    private const string DataHashAttributeName = "hash";

    public override int Order => -1;

    [HtmlAttributeName(DataHashAttributeName)]
    public string Hash { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (!string.IsNullOrWhiteSpace(Hash))
        {
            output.Attributes.SetAttribute(DataHashAttributeName, Hash);
        }
        else
        {
            output.Attributes.RemoveAll(DataHashAttributeName);
        }
    }
}
