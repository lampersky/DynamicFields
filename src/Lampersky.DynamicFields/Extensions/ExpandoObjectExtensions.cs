using System.Dynamic;

namespace OrchardCore.DynamicFields.Extensions;

public static class ExpandoObjectExtensions
{
    public static Dictionary<string, object> Flatten(this ExpandoObject expando)
    {
        var result = new Dictionary<string, object>();
        if (expando != null)
        {
            FlattenExpando(expando, result, null);
        }
        return result;
    }

    private static void FlattenExpando(object value, Dictionary<string, object> result, string parentKey)
    {
        switch (value)
        {
            case IDictionary<string, object> dict:
                foreach (var kvp in dict)
                {
                    string key = parentKey == null ? kvp.Key : $"{parentKey}.{kvp.Key}";
                    FlattenExpando(kvp.Value, result, key);
                }
                break;

            case IEnumerable<object> list:
                int index = 0;
                foreach (var item in list)
                {
                    string key = $"{parentKey}[{index}]";
                    FlattenExpando(item, result, key);
                    index++;
                }
                break;

            default:
                if (parentKey != null)
                {
                    result[parentKey] = value;
                }
                break;
        }
    }

    public static ExpandoObject ToExpando(this object anonymousObject)
    {
        var expando = new ExpandoObject();
        var dict = (IDictionary<string, object>)expando;

        foreach (var prop in anonymousObject.GetType().GetProperties())
        {
            dict[prop.Name] = prop.GetValue(anonymousObject);
        }

        return expando;
    }
}
