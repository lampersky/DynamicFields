using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.DynamicFields.Extensions;
using OrchardCore.DynamicFields.Fields;
using OrchardCore.DynamicFields.Services;
using OrchardCore.DynamicFields.ViewModels;

namespace OrchardCore.DynamicFields.Settings;

public sealed class DynamicFieldSettingsDisplayDriver(
    IHttpContextAccessor httpContextAccessor,
    IContentDefinitionManager contentDefinitionManager,
    DynamicFieldsHttpClient dynamicFieldsHttpClient
    ) : ContentPartFieldDefinitionDisplayDriver<DynamicField>
{
    public override async Task<IDisplayResult> EditAsync(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        DynamicFieldSettings templateSettings;
        var fetchOrLoadFailed = false;
        if (httpContextAccessor.HttpContext.Request.Query.TryGetValue("url", out var url))
        {
            (var success, templateSettings) = await dynamicFieldsHttpClient.TryFetchSettingsOrDefaultAsync(url);
            templateSettings = templateSettings.Fix(templateSettings?.ContentType, templateSettings?.ContentField,
                partFieldDefinition.PartDefinition.Name,
                partFieldDefinition.Name);
            fetchOrLoadFailed = !success;
        }
        else
        {
            httpContextAccessor.HttpContext.Request.Query.TryGetValue("contentType", out var contentType);
            httpContextAccessor.HttpContext.Request.Query.TryGetValue("contentField", out var contentField);
            (var success, templateSettings) = await contentDefinitionManager.TryGetFieldSettingsOrDefaultAsync<DynamicField, DynamicFieldSettings>(contentType, contentField);
            templateSettings = templateSettings.Fix(contentType, contentField,
                partFieldDefinition.PartDefinition.Name,
                partFieldDefinition.Name);
            fetchOrLoadFailed = !success;
        }

        return Initialize<EditDynamicFieldSettingsViewModel>("DynamicFieldSettings_Edit", model =>
        {
            var settings = templateSettings ?? partFieldDefinition.GetSettings<DynamicFieldSettings>();
            model.ContentType = partFieldDefinition.PartDefinition.Name;
            model.ContentField = partFieldDefinition.Name;
            model.Code = settings.Code;
            model.IndexRawValue = settings.IndexRawValue;
            model.Resources = settings.Resources.Select((item, index) => new { item, index }).ToDictionary(x => x.index, x => x.item);
            model.FetchOrLoadFailed = fetchOrLoadFailed;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        var viewModel = new EditDynamicFieldSettingsViewModel();
        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);
        var settings = ConvertViewModelToModel(viewModel);
        settings.ContentField = partFieldDefinition.Name;
        settings.ContentType = partFieldDefinition.PartDefinition.Name;
        context.Builder.WithSettings(settings);
        //var str = JsonSerializer.Serialize(settings);

        return Edit(partFieldDefinition, context);
    }

    public static DynamicFieldSettings ConvertViewModelToModel(EditDynamicFieldSettingsViewModel viewModel)
    {
        var settings = new DynamicFieldSettings()
        {
            Code = viewModel.Code,
            IndexRawValue = viewModel.IndexRawValue,
            Resources = viewModel.Resources
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => {
                    kvp.Value.Hash = ComputeHash(kvp.Value.Src);
                    return kvp.Value;
                })
                .ToList(),
        };

        return settings;
    }

    private static string ComputeHash(string input)
    {
        if (input is null) {
            return "undefined";
        }
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
    }
}
