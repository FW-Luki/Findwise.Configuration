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
    /// UI type editor that provides abilty to choose one of the types derived from specified base type, excluding the base type.
    /// </summary>
    /// <typeparam name="T">Base class or implemented interface</typeparam>
    public class DerivedTypeEditor<T> : UITypeEditor
    {
        private IWindowsFormsEditorService _editorService = null;

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            _editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

            using (var listBox = new ListBox())
            {
                listBox.SelectionMode = SelectionMode.One;
                listBox.Click += ListBox_SelectedValueChanged;

                var dataSource = typeof(T).GetDerivedTypes().Except(new[] { typeof(T) }).ToList();
                listBox.Items.Clear(); listBox.Items.AddRange(dataSource.ToArray()); //listBox.DataSource = dataSource;
                listBox.DisplayMember = nameof(Type.Name);
                listBox.ValueMember = nameof(Type.FullName);
                listBox.SelectedItem = value as Type;

                _editorService.DropDownControl(listBox);
                if (listBox.SelectedItem == null) return value;
                return ((Type)listBox.SelectedItem);
            }
        }

        private void ListBox_SelectedValueChanged(object sender, EventArgs e)
        {
            _editorService?.CloseDropDown();
            _editorService = null;
        }
    }
}
