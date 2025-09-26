using OrchardCore.ContentManagement.Utilities;
using OrchardCore.DynamicFields.Fields;
using OrchardCore.DynamicFields.Settings;

namespace OrchardCore.DynamicFields.Extensions;
public static class DynamicFieldSettingsExtensions
{
    private const string scriptPrefix = "dynamicFields.";
    private const string stylePrefix = "#";

    public static DynamicFieldSettings Fix(this DynamicFieldSettings dynamicFieldSettings,
        string clonedContentType, string clonedContentField, string contentType, string contentField)
    {
        if (dynamicFieldSettings is null)
        {
            return dynamicFieldSettings;
        }

        if (string.IsNullOrWhiteSpace(clonedContentType) || string.IsNullOrWhiteSpace(clonedContentField))
        {
            return dynamicFieldSettings;
        }

        var clonedName = $"{clonedContentType.ToSafeName()}_{clonedContentField.ToSafeName()}_{nameof(DynamicField.Value)}";
        var newName = $"{contentType.ToSafeName()}_{contentField.ToSafeName()}_{nameof(DynamicField.Value)}";

        if (!string.IsNullOrEmpty(dynamicFieldSettings.Code))
        {
            dynamicFieldSettings.Code = dynamicFieldSettings.Code.Replace($"{scriptPrefix}{clonedName}", $"{scriptPrefix}{newName}");
            dynamicFieldSettings.Code = dynamicFieldSettings.Code.Replace($"{stylePrefix}{clonedName}", $"{stylePrefix}{newName}");
        }

        foreach (var resource in dynamicFieldSettings.Resources)
        {
            if (!resource.IsInline || string.IsNullOrEmpty(resource.Src))
            {
                continue;
            }

            if (resource.IsScript)
            {
                resource.Src = resource.Src.Replace($"{scriptPrefix}{clonedName}", $"{scriptPrefix}{newName}");
            }
            else if (resource.IsStyle)
            {
                resource.Src = resource.Src.Replace($"{stylePrefix}{clonedName}", $"{stylePrefix}{newName}");
            }
        }

        dynamicFieldSettings.ContentType = contentType;
        dynamicFieldSettings.ContentField = contentField;

        return dynamicFieldSettings;
    }
}
