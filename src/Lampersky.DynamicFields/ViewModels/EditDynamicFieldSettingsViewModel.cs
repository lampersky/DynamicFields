using OrchardCore.DynamicFields.Settings;

namespace OrchardCore.DynamicFields.ViewModels;

public class EditDynamicFieldSettingsViewModel
{
    public bool IndexRawValue { get; set; } = true;
    public string Code { get; set; }
    public Dictionary<int, Resource> Resources { get; set; } = new();
    public string ContentType { get; internal set; }
    public string ContentField { get; internal set; }
    public bool FetchOrLoadFailed { get; set; }
}
