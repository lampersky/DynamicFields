using System.Dynamic;
using OrchardCore.ContentManagement;

namespace OrchardCore.DynamicFields.Fields;

public class DynamicField : ContentField
{
    public ExpandoObject Value { get; set; }
}
