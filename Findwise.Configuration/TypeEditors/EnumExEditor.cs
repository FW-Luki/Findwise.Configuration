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
    //ToDo: Improve behavior on Esc key press.
    /// <summary>
    /// UI type editor for enums with values described by <see cref="DescriptionAttribute"/>.
    /// </summary>
    /// <typeparam name="T">Type of enum providing the values</typeparam>
    public class EnumExEditor<T> : UITypeEditor where T : struct, IConvertible, IComparable
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (context != null && context.Instance != null && provider != null)
            {
                var svc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (svc != null)
                {
                    using (var listbox = OnCreateListBox())
                    {
                        listbox.BorderStyle = BorderStyle.None;
                        listbox.IntegralHeight = true;
                        listbox.Items.AddRange(Enum.GetValues(typeof(T)).Cast<T>().Select(v => GetDescribedItem(v)).Cast<object>().ToArray());
                        listbox.DisplayMember = nameof(KeyValuePair<T, string>.Value);
                        listbox.ValueMember = nameof(KeyValuePair<T, string>.Key);
                        listbox.Height = listbox.ItemHeight * Math.Min(listbox.Items.Count, 8);

                        OnListBoxCreated(context, value, svc, listbox);

                        svc.DropDownControl(listbox);
                        
                        try { return OnReturnValue(context, listbox); } catch { }
                    }
                }
            }
            return base.EditValue(context, provider, value);
        }


        protected virtual ListBox OnCreateListBox()
        {
            return new ListBox()
            {
            };
        }

        protected virtual void OnListBoxCreated(ITypeDescriptorContext context, object value, IWindowsFormsEditorService service, ListBox listbox)
        {
            for (int i = 0; i < listbox.Items.Count; i++)
            {
                listbox.SetSelected(i, Convert.ToInt32(((KeyValuePair<T, string>)listbox.Items[i]).Key) == (int)value);
            }
            listbox.Click += (s_, e_) => { service.CloseDropDown(); };
        }

        protected virtual object OnReturnValue(ITypeDescriptorContext context, ListBox listbox)
        {
            return ((KeyValuePair<T, string>)listbox.SelectedItem).Key;
        }


        private KeyValuePair<T, string> GetDescribedItem(T value)
        {
            string description;
            var fi = typeof(T).GetField(Enum.GetName(typeof(T), value));
            var dna = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));
            if (dna != null)
                description= dna.Description;
            else
                description=value.ToString();
            return new KeyValuePair<T, string>(value, description);
        }
    }
}
