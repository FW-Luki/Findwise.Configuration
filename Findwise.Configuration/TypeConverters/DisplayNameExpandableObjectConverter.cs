using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Findwise.Configuration.TypeConverters
{
    public class DisplayNameExpandableObjectConverter : ExpandableObjectConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var displayName = value?.GetType().GetCustomAttributes(false).OfType<DisplayNameAttribute>().FirstOrDefault()?.DisplayName;
            return displayName ?? base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
