using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Modules;

namespace OrchardCore.DynamicFields.Services;
public class PreviewDynamicFieldService(IShapeFactory shapeFactory,
    IContentFieldDisplayDriverResolver contentFieldDisplayDriverResolver,
    IContentPartDisplayDriverResolver contentPartDisplayDriverResolver,
    ITypeActivatorFactory<ContentPart> contentPartFactory,
    IEnumerable<IContentDisplayDriver> contentDisplayDrivers,
    IUpdateModelAccessor updateModelAccessor,
    ILayoutAccessor layoutAccessor,
    ILogger<PreviewDynamicFieldService> logger)
{
    public async Task<IShape> BuildFieldEditorAsync(ContentTypeDefinition contentTypeDefinition, bool skip = true)
    {
        var contentItem = new ContentItem();

        var itemShape = await shapeFactory.CreateAsync("Content_Edit", () => ValueTask.FromResult<IShape>(new ZoneHolding(() => shapeFactory.CreateAsync("ContentZone"))));
        itemShape.Properties["ContentItem"] = contentItem;
        var context = new BuildEditorContext(itemShape, "", false, "", shapeFactory, await layoutAccessor.GetLayoutAsync(), updateModelAccessor.ModelUpdater);

        var contentShape = context.Shape as IZoneHolding;
        var partsShape = await context.ShapeFactory.CreateAsync("ContentZone",
            Arguments.From(new
            {
                Identifier = contentItem.ContentItemId,
            }));

        contentShape.Zones["Parts"] = partsShape;

        /* todo: do we need this for single field preview? */
        if (!skip)
        {
            foreach (var displayDriver in contentDisplayDrivers)
            {
                try
                {
                    var result = await displayDriver.BuildEditorAsync(contentItem, context);
                    if (result != null)
                    {
                        await result.ApplyAsync(context);
                    }
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                    logger.LogError(ex, "{Exception} thrown from {Type} by {Method}", ex.GetType().Name, displayDriver.GetType(), nameof(BuildFieldEditorAsync));
                }
            }
        }

        foreach (var typePartDefinition in contentTypeDefinition.Parts)
        {
            var partTypeName = typePartDefinition.PartDefinition.Name;
            var partName = typePartDefinition.Name;
            var contentType = typePartDefinition.ContentTypeDefinition.Name;
            var activator = contentPartFactory.GetTypeActivator(partTypeName);
            var part = (ContentPart)contentItem.Get(activator.Type, partName) ?? activator.CreateInstance();
            var partPosition = typePartDefinition.GetSettings<ContentTypePartSettings>().Position ?? "before";
            part.ContentItem = contentItem;

            // Create a custom shape to render all the part shapes into it
            var typePartShapeResult = CreateShapeResult(context.GroupId, partTypeName, contentType, typePartDefinition, partPosition);

            await typePartShapeResult.ApplyAsync(context);
            var typePartShape = typePartShapeResult.Shape;

            if (typePartShape == null)
            {
                // Part is explicitly noop in placement then stop rendering execution
                continue;
            }

            typePartShape.Properties["ContentPart"] = part;
            typePartShape.Properties["ContentTypePartDefinition"] = typePartDefinition;
            partsShape.Properties[partName] = typePartShape;

            context.DefaultZone = $"Parts.{partName}";
            context.DefaultPosition = partPosition;

            /* todo: do we need this for single field preview? */
            if (!skip)
            {
                var partDisplayDrivers = contentPartDisplayDriverResolver.GetEditorDrivers(partTypeName, typePartDefinition.Editor());
                await partDisplayDrivers.InvokeAsync(async (driver, part, typePartDefinition, context) =>
                {
                    var result = await driver.BuildEditorAsync(part, typePartDefinition, context);
                    if (result != null)
                    {
                        await result.ApplyAsync(context);
                    }
                }, part, typePartDefinition, context, logger);
            }

            foreach (var partFieldDefinition in typePartDefinition.PartDefinition.Fields)
            {
                var fieldName = partFieldDefinition.Name;
                var fieldPosition = partFieldDefinition.GetSettings<ContentPartFieldSettings>().Position ?? "before";

                context.DefaultZone = $"Parts.{partName}:{fieldPosition}";
                var fieldDisplayDrivers = contentFieldDisplayDriverResolver.GetEditorDrivers(partFieldDefinition.FieldDefinition.Name, partFieldDefinition.Editor());
                await fieldDisplayDrivers.InvokeAsync(async (driver, part, partFieldDefinition, typePartDefinition, context) =>
                {
                    var result = await driver.BuildEditorAsync(part, partFieldDefinition, typePartDefinition, context);
                    if (result != null)
                    {
                        await result.ApplyAsync(context);
                    }
                }, part, partFieldDefinition, typePartDefinition, context, logger);
            }
        }

        return context.Shape;
    }

    private static ShapeResult CreateShapeResult(string groupId, string partTypeName, string contentType, ContentTypePartDefinition typePartDefinition, string partPosition)
    {
        var shapeType = "ContentPart_Edit";
        var partName = typePartDefinition.Name;

        var typePartShapeResult = new ShapeResult(shapeType, ctx => ctx.ShapeFactory.CreateAsync(shapeType));
        typePartShapeResult.Differentiator($"{contentType}-{partName}");
        typePartShapeResult.Name(partName);
        typePartShapeResult.Location($"Parts:{partPosition}");
        typePartShapeResult.OnGroup(groupId);
        typePartShapeResult.Displaying(ctx =>
        {
            // ContentPart_Edit__[PartType]
            // eg ContentPart-ServicePart.Edit
            ctx.Shape.Metadata.Alternates.Add($"{shapeType}__{partTypeName}");

            // ContentPart_Edit__[ContentType]__[PartType]
            // e.g. ContentPart-LandingPage-ServicePart.Edit
            ctx.Shape.Metadata.Alternates.Add($"{shapeType}__{contentType}__{partTypeName}");

            var isNamedPart = typePartDefinition.PartDefinition.IsReusable() && partName != partTypeName;

            if (isNamedPart)
            {
                // ContentPart_Edit__[ContentType]__[PartName]
                // e.g. ContentPart-LandingPage-BillingService.Edit ContentPart-LandingPage-HelplineService.Edit
                ctx.Shape.Metadata.Alternates.Add($"{shapeType}__{contentType}__{partName}");
            }
        });

        return typePartShapeResult;
    }
}
