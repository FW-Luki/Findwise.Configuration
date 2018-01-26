using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Findwise.Configuration.TypeEditors
{
    /// <summary>
    /// <para>UI type editor that provides abilty to choose one of the types derived from specified base type, excluding the base type.</para>
    /// <para>In order to include the base type or sort types alphabetically add <see cref="RepeatTypeEditor.OptionsAttribute"/>.</para>
    /// </summary>
    /// <typeparam name="T">Base class or implemented interface</typeparam>
    public class RepeatTypeEditor<T> : ListBoxEditor
    {
        protected override object[] GetItems(ITypeDescriptorContext context)
        {
            var dataSource = typeof(T).GetDerivedTypes();
            var options = context.PropertyDescriptor.Attributes.OfType<RepeatTypeEditor.OptionsAttribute>().FirstOrDefault();
            if (!(options?.IncludeBaseType ?? false)) dataSource = dataSource.Except(new[] { typeof(T) });
            if (options?.AlphabeticOrder ?? false) dataSource = dataSource.OrderBy(t => t.Name);
            return dataSource.ToArray();
        }

        protected override object GetPreviousItem(object value)
        {
            return value as Type;
        }

        protected override object GetCurrentItem(object value)
        {
            return (Type)value;
        }

        protected override string GetDisplayMember()
        {
            return nameof(Type.Name);
        }
        protected override string GetValueMember()
        {
            return nameof(Type.FullName);
        }
    }

    /// <summary>
    /// <para>UI type editor that provides abilty to choose one of the types derived from specified base type, excluding the base type.</para>
    /// <para>This version is more flexible than its generic equivalent due to its lack of type parameter but it must go with <see cref="RepeatTypeEditor.OptionsAttribute.BaseType"/> specified.</para>
    /// </summary>
    public class RepeatTypeEditor : RepeatTypeEditor<object>
    {
        protected Type BaseType;

        protected override object[] GetItems(ITypeDescriptorContext context)
        {
            var options = context.PropertyDescriptor.Attributes.OfType<RepeatTypeEditor.OptionsAttribute>().First();
            BaseType = options.BaseType;
            var dataSource = BaseType.GetDerivedTypes();
            if (!(options?.IncludeBaseType ?? false)) dataSource = dataSource.Except(new[] { BaseType });
            if (options?.AlphabeticOrder ?? false) dataSource = dataSource.OrderBy(t => t.Name);
            return dataSource.ToArray();
        }


        public class OptionsAttribute : Attribute
        {
            public bool IncludeBaseType { get; set; }
            public bool AlphabeticOrder { get; set; }
            public Type BaseType { get; set; }
        }
    }
}
