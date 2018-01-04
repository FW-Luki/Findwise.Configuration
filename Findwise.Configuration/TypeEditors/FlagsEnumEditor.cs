using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Findwise.Configuration.TypeEditors
{
    /// <summary>
    /// UI type editor that provides abilty to check/uncheck flags provided by enum with <see cref="FlagsAttribute"/> as well as array of enum values.
    /// </summary>
    /// <typeparam name="T">Type of enum providing the values</typeparam>
    public class FlagsEnumEditor<T> : EnumExEditor<T> where T : struct, IConvertible, IComparable
    {
        public /*override*/ object __EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (context != null && context.Instance != null && provider != null)
            {
                var svc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (svc != null)
                {
                    using (var listbox = new CheckedListBox()
                    {
                        BorderStyle = BorderStyle.None,
                        CheckOnClick = true
                    })
                    {
                        listbox.Items.AddRange(Enum.GetValues(typeof(T)).Cast<object>().ToArray());

                        if (context.PropertyDescriptor.PropertyType.IsEnum) //ToDo: Item of value = 0 check/uncheck support needs to be improved.
                        {
                            listbox.ItemCheck += Listbox_ItemCheck;
                            for (int i = 0; i < listbox.Items.Count; i++)
                            {
                                if (((Enum)value).HasFlag((Enum)listbox.Items[i]))
                                    listbox.SetItemChecked(i, true);
                            }
                        }
                        else if (context.PropertyDescriptor.PropertyType.IsArray)
                        {
                            for (int i = 0; i < listbox.Items.Count; i++)
                            {
                                if (value != null && Array.Exists((T[])value, m => m.CompareTo((T)listbox.Items[i]) == 0))
                                    listbox.SetItemChecked(i, true);
                            }
                        }

                        svc.DropDownControl(listbox);

                        if (context.PropertyDescriptor.PropertyType.IsEnum)
                            return GetFlagsValue(listbox);
                        else if (context.PropertyDescriptor.PropertyType.IsArray)
                            return GetCollectionValue(listbox).ToArray();
                    }
                }
            }
            return base.EditValue(context, provider, value);
        }


        protected override ListBox OnCreateListBox()
        {
            return new CheckedListBox()
            {
                CheckOnClick = true
            };
        }

        protected override void OnListBoxCreated(ITypeDescriptorContext context, object value, IWindowsFormsEditorService service, ListBox listbox)
        {
            var mylistbox = (CheckedListBox)listbox;
            if (context.PropertyDescriptor.PropertyType.IsEnum) //ToDo: Item of value = 0 check/uncheck support needs to be improved.
            {
                mylistbox.ItemCheck += Listbox_ItemCheck;
                for (int i = 0; i < mylistbox.Items.Count; i++)
                {
                    if (((Enum)value).HasFlag((Enum)(object)((KeyValuePair<T, string>)mylistbox.Items[i]).Key))
                        mylistbox.SetItemChecked(i, true);
                }
            }
            else if (context.PropertyDescriptor.PropertyType.IsArray)
            {
                for (int i = 0; i < mylistbox.Items.Count; i++)
                {
                    if (value != null && Array.Exists((T[])value, m => m.CompareTo(((KeyValuePair<T, string>)mylistbox.Items[i]).Key) == 0))
                        mylistbox.SetItemChecked(i, true);
                }
            }
        }

        protected override object OnReturnValue(ITypeDescriptorContext context, ListBox listbox)
        {
            var mylistbox = (CheckedListBox)listbox;
            if (context.PropertyDescriptor.PropertyType.IsEnum)
                return GetFlagsValue(mylistbox);
            else if (context.PropertyDescriptor.PropertyType.IsArray)
                return GetCollectionValue(mylistbox).ToArray();
            else
                throw new ApplicationException();
        }


        private static int GetFlagsValue(CheckedListBox listbox)
        {
            return listbox.CheckedItems.Cast<KeyValuePair<T, string>>().Aggregate(0, (tot, nxt) => tot |= Convert.ToInt32(nxt.Key));
        }

        private static IEnumerable<T> GetCollectionValue(CheckedListBox listbox)
        {
            return listbox.CheckedItems.Cast<KeyValuePair<T, string>>().Select(kvp => kvp.Key);
        }

        private bool _updating = false;
        private void Listbox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var listbox = sender as CheckedListBox;
            listbox.ItemCheck -= Listbox_ItemCheck;
            listbox.SetItemCheckState(e.Index, e.NewValue); //Hack to make ItemCheck act like "after checked" event.
            listbox.ItemCheck += Listbox_ItemCheck;

            if (_updating) return;
            _updating = true;
            foreach (var item in listbox.Items.Cast<KeyValuePair<T, string>>().Where(item => Convert.ToInt32(item.Key) == 0).ToArray()) //ToArray() used to avoid "collection changed" problem.
            {
                listbox.SetItemChecked(listbox.Items.IndexOf(item), GetFlagsValue(listbox) == 0);
            }
            _updating = false;
        }

    }
}
