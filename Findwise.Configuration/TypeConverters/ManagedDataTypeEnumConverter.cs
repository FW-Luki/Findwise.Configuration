using Microsoft.Office.Server.Search.Administration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Findwise.Configuration.TypeConverters
{
    public class ManagedDataTypeEnumConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(ManagedDataType)) return true;
            return base.CanConvertFrom(context, sourceType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is ManagedDataType)
            {
                value.ToString();
            }
            return base.ConvertFrom(context, culture, value);
        }
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(ManagedDataType)) return true;
            return base.CanConvertTo(context, destinationType);
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(ManagedDataType) && value is string str)
            {
                return Enum.Parse(destinationType, str);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
