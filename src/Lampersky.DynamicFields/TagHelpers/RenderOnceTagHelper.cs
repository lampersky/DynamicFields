using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DynamicFields.TagHelpers;

[HtmlTargetElement("render-once", Attributes = "key")]
public class RenderOnceTagHelper : TagHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RenderOnceTagHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    [HtmlAttributeName("key")]
    public string Key { get; set; } = string.Empty;

    [HtmlAttributeName("disabled")]
    public bool IsDisabled { get; set; } = false;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            output.SuppressOutput();
            return;
        }

        output.TagName = null;

        if (IsDisabled || string.IsNullOrWhiteSpace(Key))
        {
            // always render when disabled or invalid key
            return;
        }

        if (httpContext.Items.ContainsKey(Key))
        {
            output.SuppressOutput();
            return;
        }

        httpContext.Items[Key] = true;

#if DEBUG
        output.PreElement.SetHtmlContent($"<!-- Rendered once: {Key} -->");
#endif
    }
}
