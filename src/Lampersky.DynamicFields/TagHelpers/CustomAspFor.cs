using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DynamicFields.TagHelpers;

[HtmlTargetElement("dynamic-fields", Attributes = ForAttributeName)]
public class CustomAspForHelper : InputTagHelper {
    private const string ForAttributeName = "asp-for";

    public CustomAspForHelper(IHtmlGenerator generator) : base(generator)
    {
    }
}
