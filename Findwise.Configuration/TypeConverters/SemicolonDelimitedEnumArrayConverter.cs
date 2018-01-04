using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Findwise.Configuration.TypeConverters
{
    public class SemicolonDelimitedEnumArrayConverter<T> : ArrayConverter
    {
        public virtual string Delimiter { get { return ";"; } }
        protected virtual bool EnumParseIgnoreCase { get { return true; } }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return ((string)value).Split(new[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries).Select(s => (T)Enum.Parse(typeof(T), s, EnumParseIgnoreCase)).ToArray();
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value == null) return null;
            return string.Join(Delimiter, (IEnumerable<T>)value);
        }
    }


    public class SemicolonDelimitedEnumArrayConverterNonExpandable<T> : SemicolonDelimitedEnumArrayConverter<T>
    {
        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return false;
        }
    }
}
