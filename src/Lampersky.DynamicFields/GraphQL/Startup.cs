using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.DynamicFields.Fields;
using OrchardCore.Modules;

namespace OrchardCore.DynamicFields.GraphQL;

[RequireFeatures("OrchardCore.Apis.GraphQL")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddObjectGraphType<DynamicField, DynamicFieldQueryObjectType>();
    }
}
