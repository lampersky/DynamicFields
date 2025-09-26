using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DynamicFields.TagHelpers;

[HtmlTargetElement("script", Attributes = DeferAttributeName)]
[HtmlTargetElement("script", Attributes = AsyncAttributeName)]
[HtmlTargetElement("script", Attributes = NoModuleAttributeName)]
[HtmlTargetElement("script", Attributes = CrossOriginAttributeName)]
[HtmlTargetElement("script", Attributes = TypeAttributeName)]
public class ExtendedScriptTagHelper : TagHelper
{
    private const string DeferAttributeName = "defer";
    private const string AsyncAttributeName = "async";
    private const string NoModuleAttributeName = "nomodule";
    private const string CrossOriginAttributeName = "crossorigin";
    private const string TypeAttributeName = "type";

    public override int Order => -1;

    [HtmlAttributeName(DeferAttributeName)]
    public bool Defer { get; set; }
    [HtmlAttributeName(AsyncAttributeName)]
    public bool Async { get; set; }
    [HtmlAttributeName(NoModuleAttributeName)]
    public bool NoModule { get; set; }
    [HtmlAttributeName(CrossOriginAttributeName)]
    public bool CrossOrigin { get; set; }

    [HtmlAttributeName(TypeAttributeName)]
    public string Type { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var booleanMap = new Dictionary<string, bool>
        {
            { DeferAttributeName, Defer },
            { AsyncAttributeName, Async },
            { NoModuleAttributeName, NoModule },
            { CrossOriginAttributeName, CrossOrigin },
            { TypeAttributeName, !string.IsNullOrEmpty(Type) },
        };

        foreach (var kvp in booleanMap)
        {
            SetAttribute(output, kvp.Key, kvp.Value);
        }
    }

    private void SetAttribute(TagHelperOutput output, string name, bool enabled)
    {
        if (enabled)
        {
            // for boolean attributes, it would be better to pass null,
            // but original ScriptTagHelper implementation is calling ToString() on each attribute value
            // and we don't want to end up with NullReferenceException
            // output.Attributes.SetAttribute(name, (object)null);
            output.Attributes.SetAttribute(name, name == TypeAttributeName ? Type : "");
        }
        else
        {
            output.Attributes.RemoveAll(name);
        }
    }
}
