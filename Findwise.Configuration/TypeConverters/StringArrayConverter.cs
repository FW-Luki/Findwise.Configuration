using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Findwise.Configuration.TypeConverters
{
    public class StringArrayConverter : TypeConverter
    {
        private char _delimiter = ',';
        public virtual char Delimiter { get { return _delimiter; } set { _delimiter = value; } }
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string[])) return true;
            return base.CanConvertFrom(context, sourceType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string[] stringArray)
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < stringArray.Count(); i++)
                {
                    stringBuilder.Append(i);
                    if (i < stringArray.Count() - 1)
                        stringBuilder.Append(Delimiter);
                }
                return stringBuilder.ToString();
            }
            return base.ConvertFrom(context, culture, value);
        }
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string[])) return true;
            return base.CanConvertTo(context, destinationType);
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if(destinationType == typeof(string[]) && value is string str)
            {
                return str.Split(Delimiter);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
