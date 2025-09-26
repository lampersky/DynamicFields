using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentFields.Indexing.SQL;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DynamicFields.Extensions;
using OrchardCore.DynamicFields.Fields;
using OrchardCore.DynamicFields.Settings;
using YesSql.Indexes;

namespace OrchardCore.DynamicFields.Indexing.SQL;

public class DynamicFieldIndex : TextFieldIndex
{
    public string Path { get; set; }
    public string Type { get; set; }
}

public class DynamicFieldIndexProvider : ContentFieldIndexProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly HashSet<string> _ignoredTypes = [];
    private IContentDefinitionManager _contentDefinitionManager;

    public DynamicFieldIndexProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override void Describe(DescribeContext<ContentItem> context)
    {
        context.For<DynamicFieldIndex>()
            .Map(async contentItem =>
            {
                // Remove index records of soft deleted items.
                if (!contentItem.Published && !contentItem.Latest)
                {
                    return null;
                }

                // Can we safely ignore this content item?
                if (_ignoredTypes.Contains(contentItem.ContentType))
                {
                    return null;
                }

                // Lazy initialization because of ISession cyclic dependency
                _contentDefinitionManager ??= _serviceProvider.GetRequiredService<IContentDefinitionManager>();

                // Search for DynamicField
                var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentItem.ContentType);

                // This can occur when content items become orphaned, particularly layer widgets when a layer is removed, before its widgets have been unpublished.
                if (contentTypeDefinition == null)
                {
                    _ignoredTypes.Add(contentItem.ContentType);
                    return null;
                }

                var fieldDefinitions = contentTypeDefinition
                    .Parts.SelectMany(x => x.PartDefinition.Fields.Where(f => f.FieldDefinition.Name == nameof(DynamicField)))
                    .ToArray();

                // This type doesn't have any DynamicField, ignore it
                if (fieldDefinitions.Length == 0)
                {
                    _ignoredTypes.Add(contentItem.ContentType);
                    return null;
                }

                return fieldDefinitions
                    .GetContentFields<DynamicField>(contentItem)
                    .SelectMany(pair => {
                        var result = new List<DynamicFieldIndex> { };
                        var settings = pair.Definition.GetSettings<DynamicFieldSettings>();

                        if (settings.IndexRawValue)
                        {
                            var value = JsonSerializer.Serialize(pair.Field.Value);

                            result.Add(
                                new DynamicFieldIndex
                                {
                                    Latest = contentItem.Latest,
                                    Published = contentItem.Published,
                                    ContentItemId = contentItem.ContentItemId,
                                    ContentItemVersionId = contentItem.ContentItemVersionId,
                                    ContentType = contentItem.ContentType,
                                    ContentPart = pair.Definition.ContentTypePartDefinition.Name,
                                    ContentField = pair.Definition.Name,
                                    Type = value.GetType().ToString(),
                                    Path = "raw",
                                    Text = value?.ToString()?[..Math.Min(value?.ToString()?.Length ?? 0, TextFieldIndex.MaxTextSize)],
                                    BigText = value?.ToString(),
                                });
                        }
                        else
                        {
                            var flattened = pair.Field.Value.Flatten();
                            var grouped = flattened.GroupBy(item => item.Value?.GetType() ?? typeof(object));

                            foreach (var group in grouped)
                            {
                                var type = group.Key;
                                foreach (var item in group)
                                {
                                    var path = item.Key;
                                    var value = item.Value;
                                    result.Add(
                                        new DynamicFieldIndex
                                        {
                                            Latest = contentItem.Latest,
                                            Published = contentItem.Published,
                                            ContentItemId = contentItem.ContentItemId,
                                            ContentItemVersionId = contentItem.ContentItemVersionId,
                                            ContentType = contentItem.ContentType,
                                            ContentPart = pair.Definition.ContentTypePartDefinition.Name,
                                            ContentField = pair.Definition.Name,
                                            Type = type.ToString(),
                                            Path = path,
                                            Text = value?.ToString()?[..Math.Min(value?.ToString()?.Length ?? 0, TextFieldIndex.MaxTextSize)],
                                            BigText = value?.ToString(),
                                        });
                                }
                            }
                        }

                        return result;
                    });
            });
    }
}
