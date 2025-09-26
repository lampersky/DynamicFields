using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DynamicFields.Indexing.SQL;
using OrchardCore.Modules;
using OrchardCore.Data;
using OrchardCore.Data.Migration;

namespace OrchardCore.DynamicFields.Indexing;

[Feature("OrchardCore.DynamicFields.Indexing.SQL")]
[RequireFeatures("OrchardCore.ContentFields.Indexing.SQL")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataMigration<Migrations>();
        services.AddScopedIndexProvider<DynamicFieldIndexProvider>();
    }
}
