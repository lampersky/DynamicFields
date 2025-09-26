using System.Text.Json;
using System.Text.Json.Serialization;
using OrchardCore.DynamicFields.Settings;
using OrchardCore.ResourceManagement;

namespace OrchardCore.DynamicFields.Converters;

public class ResourceConverter : JsonConverter<Resource>
{
    public override Resource Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject");
        }

        var resource = new Resource();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return resource;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected PropertyName");
            }

            string propertyName = reader.GetString()!;
            reader.Read();

            switch (propertyName)
            {
                case nameof(resource.At):
                    resource.At = (ResourceLocation)reader.GetInt32();
                    break;

                case nameof(resource.Hash):
                    resource.Hash = reader.GetString()!;
                    break;

                case nameof(resource.IsInline):
                    resource.IsInline = reader.GetBoolean();
                    break;

                case nameof(resource.Src):
                    resource.Src = reader.GetString()!;
                    break;

                case nameof(resource.ScriptType):
                    resource.ScriptType = ScriptType.Parse(reader.GetString());
                    break;

                case nameof(resource.Type):
                    resource.Type = (ResourceType)reader.GetInt32();
                    break;

                case nameof(resource.IsAsync):
                    resource.IsAsync = reader.GetBoolean();
                    break;

                case nameof(resource.IsDeferred):
                    resource.IsDeferred = reader.GetBoolean();
                    break;

                default:
                    reader.Skip();
                    break;
            }
        }

        throw new JsonException("Invalid JSON for Resource");
    }

    public override void Write(Utf8JsonWriter writer, Resource value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber(nameof(value.At), (int)value.At);
        writer.WriteString(nameof(value.Hash), value.Hash);
        writer.WriteBoolean(nameof(value.IsInline), value.IsInline);
        writer.WriteString(nameof(value.Src), value.Src);
        writer.WriteNumber(nameof(value.Type), (int)value.Type);

        if (value.IsScript)
        {
            writer.WriteBoolean(nameof(value.IsAsync), value.IsAsync);
            writer.WriteBoolean(nameof(value.IsDeferred), value.IsDeferred);
            writer.WriteString(nameof(value.ScriptType), value.ScriptType);
        }

        writer.WriteEndObject();
    }
}
