using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Findwise.Configuration
{
    /// <summary>
    /// Specifies the name of the category in which to group the property or event when displayed in a System.Windows.Forms.PropertyGrid control set to Categorized mode.
    /// Supports category ordering. Ordering is 1(one)-based, descending which means the higher the number, the higher the category will be displayed, ordering starts with 1.
    /// </summary>
    public class OrderedCategoryAttribute : CategoryAttribute
    {
        public int Order { get; }
        public OrderedCategoryAttribute(string category, int order) : base(category.PadLeft(category.Length + Math.Abs(order), '\t'))
        {
            Order = order;
        }
    }
}
