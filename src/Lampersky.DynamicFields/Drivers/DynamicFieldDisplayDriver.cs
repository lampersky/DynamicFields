using System.Dynamic;
using System.Text.Json;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.DynamicFields.Fields;
using OrchardCore.DynamicFields.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.DynamicFields.Drivers;

public sealed class DynamicFieldDisplayDriver : ContentFieldDisplayDriver<DynamicField>
{
    internal readonly IStringLocalizer S;

    public DynamicFieldDisplayDriver(IStringLocalizer<DynamicFieldDisplayDriver> localizer)
    {
        S = localizer;
    }

    public override IDisplayResult Display(DynamicField field, BuildFieldDisplayContext context)
    {
        return Initialize<DisplayDynamicFieldViewModel>(GetDisplayShapeType(context), model =>
        {
            model.RawValue = JsonSerializer.Serialize(field.Value);
            model.Field = field;
            model.Part = context.ContentPart;
            model.PartFieldDefinition = context.PartFieldDefinition;
        })
        .Location("Detail", "Content")
        .Location("Summary", "Content");
    }

    public override IDisplayResult Edit(DynamicField field, BuildFieldEditorContext context)
    {
        return Initialize<EditDynamicFieldViewModel>(GetEditorShapeType(context), model =>
        {
            model.Value = JsonSerializer.Serialize(field.Value);
            model.Field = field;
            model.Part = context.ContentPart;
            model.PartFieldDefinition = context.PartFieldDefinition;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(DynamicField field, UpdateFieldEditorContext context)
    {
        var model = new EditDynamicFieldViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Value);

        if (model.Value != null) {
            try
            {
                field.Value = JsonSerializer.Deserialize<ExpandoObject>(model.Value);
            }
            catch (JsonException)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(field.Value), S["The value provided is not valid JSON for {0}.", context.PartFieldDefinition.DisplayName()]);
            }
        }

        return Edit(field, context);
    }
}
