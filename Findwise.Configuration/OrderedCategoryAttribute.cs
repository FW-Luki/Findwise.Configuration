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
    /// Supports category ordering. Ordering is 1(one)-based, descending which means the higher the number, the higher the category will be displayed, ordering starts from 1.
    /// </summary>
    public class OrderedCategoryAttribute : CategoryAttribute
    {
        /// <summary>
        /// Gets the attribute order.
        /// </summary>
        public int Order { get; }


        /// <summary>
        /// Creates new instance of <see cref="OrderedCategoryAttribute"/> using specified name with no order.
        /// </summary>
        /// <param name="category">The name of the category.</param>
        public OrderedCategoryAttribute(string category) : base(category)
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="OrderedCategoryAttribute"/> using specified name with the order specified.
        /// The order number also indicates how many tabs are inserted at the left side of category name string.
        /// </summary>
        /// <param name="category">The name of the category.</param>
        /// <param name="order">Order value. Property with <see cref="OrderedCategoryAttribute"/> with the highest value will be displayed topmost.</param>
        public OrderedCategoryAttribute(string category, int order) : base(category.PadLeft(category.Length + Math.Abs(order), '\t'))
        {
            Order = order;
        }

        /// <summary>
        /// Creates new instance of <see cref="OrderedCategoryAttribute"/> using specified name with the order specified.
        /// The order number also indicates how many tabs are inserted at the left side of category name string.
        /// It uses <see cref="KeyValuePair&lt;string, int&gt;"/> to increase category order consistency.
        /// </summary>
        /// <param name="orderedCategory">A  <see cref="KeyValuePair&lt;string, int&gt;"/> containing the name of the category in the Key and the order in Value.</param>
        public OrderedCategoryAttribute(KeyValuePair<string, int> orderedCategory) : base(orderedCategory.Key.PadLeft(orderedCategory.Key.Length + Math.Abs(orderedCategory.Value), '\t'))
        {
            Order = orderedCategory.Value;
        }

    }
}
