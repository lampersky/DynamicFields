using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DynamicFields.Settings;
using OrchardCore.DynamicFields.ViewModels;

namespace OrchardCore.DynamicFields.Controllers;

[Authorize]
public class AdminController(IUpdateModelAccessor UpdateModelAccessor) : Controller
{
    [HttpPost]
    public async Task<IActionResult> DynamicFieldPreviewAsync(string contentType, string contentField)
    {
        var model = new EditDynamicFieldSettingsViewModel();
        var prefix = $"{contentType}.{contentField}.{nameof(DynamicFieldSettingsDisplayDriver)}";
        await UpdateModelAccessor.ModelUpdater.TryUpdateModelAsync(model, prefix);
        var settings = DynamicFieldSettingsDisplayDriver.ConvertViewModelToModel(model);

        return View(new DynamicFieldPreviewViewModel {
            Settings = settings,
            ContentField = contentField,
            ContentPart = contentType,
        });
    }
}
