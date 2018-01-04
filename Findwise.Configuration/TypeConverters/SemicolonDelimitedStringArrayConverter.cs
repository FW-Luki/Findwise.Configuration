using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Findwise.Configuration.TypeConverters
{
    public class SemicolonDelimitedStringArrayConverter : ArrayConverter
    {
        public virtual string Delimiter { get { return ";"; } }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return ((string)value).Split(new[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value == null) return null;
            return string.Join(Delimiter, (IEnumerable<string>)value);
        }
    }
}
