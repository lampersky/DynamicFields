using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DynamicFields.Fields;

namespace OrchardCore.DynamicFields.ViewModels;

public class EditDynamicFieldViewModel
{
    public string Value { get; set; }
    public DynamicField Field { get; set; }
    public ContentPart Part { get; set; }
    public ContentPartFieldDefinition PartFieldDefinition { get; set; }
}
