using OrchardCore.DynamicFields.Settings;

namespace OrchardCore.DynamicFields.ViewModels;
public class DynamicFieldPreviewViewModel
{
    public DynamicFieldSettings Settings { get; set; }
    public string ContentPart { get; set; }
    public string ContentField { get; set; }
}
