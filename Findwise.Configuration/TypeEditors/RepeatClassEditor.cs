using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Findwise.Configuration.TypeEditors
{
    /// <summary>
    /// <para>UI type editor that provides abilty to create an instance of one of the types derived from specified base type, excluding the base type.</para>
    /// <para>In order to include the base type or sort types alphabetically add <see cref="RepeatTypeEditor.OptionsAttribute"/>.</para>
    /// </summary>
    /// <typeparam name="T">Base class or implemented interface</typeparam>
    public class RepeatClassEditor<T> : RepeatTypeEditor<T>
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editedValue = base.EditValue(context, provider, value);
            if (editedValue is Type type)
            {
                return type != null ? Activator.CreateInstance(type) : null;
            }
            else
            {
                return editedValue;
            }
        }
    }

    /// <summary>
    /// <para>UI type editor that provides abilty to create an instance of one of the types derived from specified base type, excluding the base type.</para>
    /// <para>This version is more flexible than its generic equivalent due to its lack of type parameter but it must go with <see cref="RepeatTypeEditor.OptionsAttribute.BaseType"/> specified.</para>
    /// </summary>
    public class RepeatClassEditor : RepeatTypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editedValue = base.EditValue(context, provider, value);
            if (editedValue is Type type)
            {
                return type != null ? Activator.CreateInstance(type) : null;
            }
            else
            {
                return editedValue;
            }
        }
    }
}
