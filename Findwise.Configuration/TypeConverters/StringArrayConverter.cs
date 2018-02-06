using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Findwise.Configuration.TypeConverters
{
    public class StringArrayConverter : TypeConverter //ToDo: Rename it to ArrayDelimitedValuesConverter and make it generic?
    {
        public string Delimiter { get; set; } = ","; //ToDo: Or separator?

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string str)
            {
                return str.Split(Delimiter.ToCharArray());
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string)) return true;
            return base.CanConvertTo(context, destinationType);
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if(destinationType == typeof(string) && value is string[] stringArray)
            {
                return string.Join(Delimiter, stringArray);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
