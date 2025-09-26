using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.DynamicFields.Extensions;
public static class ContentTypeDefinitionBuilderExtensions
{
    public static ContentTypeDefinition BuildFakeContentTypeDefinition<TField, TSettings>(this ContentTypeDefinitionBuilder builder, string contentType, string contentPart, string contentField, TSettings settings)
    {
        var contentTypeDefinition = builder
            .Named(contentType)
            .WithPart(contentPart, new ContentPartDefinitionBuilder()
                .Named(contentPart)
                .WithField<TField>(contentField, field => field.WithSettings(settings))
                .Build(),
                c => { }
            )
            .Build();

        return contentTypeDefinition;
    }
}
