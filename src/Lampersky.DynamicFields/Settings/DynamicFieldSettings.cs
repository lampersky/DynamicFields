using System.ComponentModel;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DynamicFields.Converters;
using OrchardCore.ResourceManagement;

namespace OrchardCore.DynamicFields.Settings;

public class DynamicFieldSettings
{
    public string ContentType { get; set; }
    public string ContentField { get; set; }
    public bool IndexRawValue { get; set; } = true;
    public string Code { get; set; }
    public List<Resource> Resources { get; set; } = new();
}

public enum ResourceType
{
    Script,
    Style,
}

[TypeConverter(typeof(ScriptTypeConverter))]
public sealed record ScriptType(string Value)
{
    public static readonly ScriptType Default = new("");
    public static readonly ScriptType Module = new("module");
    public static readonly ScriptType Babel = new("text/babel");

    public static implicit operator string(ScriptType scriptType) => scriptType is null ? Default.Value : scriptType.Value;
    public override string ToString() => Value;
    public static ScriptType Parse(string value)
    {
        return value switch
        {
            null => Default,
            "" => Default,
            "module" => Module,
            "text/babel" => Babel,
            _ => new(value)
        };
    }
}

[JsonConverter(typeof(ResourceConverter))]
public class Resource
{
    private ScriptType _scriptType;

    public Resource()
    {
        IsInline = false;
        IsDeferred = false;
        IsAsync = false;
        ScriptType = ScriptType.Default;
    }

    public ResourceType Type { get; set; } = ResourceType.Script;
    public string Src { get; set; }
    public string Hash { get; set; }
    public ResourceLocation At { get; set; } = ResourceLocation.Foot;
    public ScriptType ScriptType
    {
        get => _scriptType;
        set => _scriptType = value ?? ScriptType.Default;
    }

    [BindingBehavior(BindingBehavior.Optional)]
    [DefaultValue(false)]
    public bool IsInline { get; set; }

    [BindingBehavior(BindingBehavior.Optional)]
    [DefaultValue(false)]
    public bool IsDeferred { get; set; }

    [BindingBehavior(BindingBehavior.Optional)]
    [DefaultValue(false)]
    public bool IsAsync { get; set; }

    [JsonIgnore]
    public bool IsScript => Type == ResourceType.Script;
    [JsonIgnore]
    public bool IsStyle => Type == ResourceType.Style;
    [JsonIgnore]
    public bool IsModule => ScriptType == ScriptType.Module;
}
