using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.DynamicFields.Services;

public class DynamicFieldsConfiguration(IShellConfiguration shellConfiguration) : IConfigureOptions<DynamicFieldsOptions>
{
    public void Configure(DynamicFieldsOptions options)
    {
        var section = shellConfiguration.GetSection("OrchardCore_DynamicFields");
        var repoistoryUrls = section?.GetSection("RepositoryUrls")?.Get<string[]>() ?? [];
        options.RepositoryUrls = new HashSet<string>(repoistoryUrls, StringComparer.OrdinalIgnoreCase);
    }
}

public class DynamicFieldsOptions
{
    public HashSet<string> RepositoryUrls { get; set; }
}
