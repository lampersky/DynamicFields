using System.ComponentModel;
using OrchardCore.DynamicFields.Settings;

namespace OrchardCore.DynamicFields.Converters;

public class ScriptTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
        if (value is string s)
        {
            return ScriptType.Parse(s);
        }

        return base.ConvertFrom(context, culture, value);
    }
}
