using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Findwise.Configuration.TypeEditors
{
    public abstract class ListBoxEditor : UITypeEditor
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

                listBox.Items.Clear(); listBox.Items.AddRange(GetItems(context)); //listBox.DataSource = GetItems(context);
                listBox.DisplayMember = GetDisplayMember();
                listBox.ValueMember = GetValueMember();
                listBox.SelectedItem = GetPreviousItem(value);

                _editorService.DropDownControl(listBox);
                if (listBox.SelectedItem == null) return value;
                return GetCurrentItem(listBox.SelectedItem);
            }
        }

        private void ListBox_SelectedValueChanged(object sender, EventArgs e)
        {
            _editorService?.CloseDropDown();
            _editorService = null;
        }


        protected abstract object[] GetItems(ITypeDescriptorContext context);

        protected abstract string GetDisplayMember();
        protected abstract string GetValueMember();

        protected abstract object GetPreviousItem(object value);
        protected abstract object GetCurrentItem(object value);
    }
}
