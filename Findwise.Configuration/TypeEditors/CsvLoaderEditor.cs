using Findwise.Configuration.TypeConverters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Findwise.Configuration.TypeEditors
{
    [TypeConverter(typeof(StringArrayConverter))]
    public class CsvLoaderEditor : ArrayEditor
    {
        public CsvLoaderEditor(Type type) : base(type)
        {
        }

        public string FieldSeparator { get => ";"; }
        public string ItemSeparator { get => ","; }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (context.PropertyDescriptor.PropertyType.IsArray)
            {
                var type = context.PropertyDescriptor.PropertyType.GetElementType();
                using (var dialog = new OpenFileDialog() { Filter = "Csv Files|*.csv|All Files|*.*" })
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        var dataTable = new DataTable();
                        var csvLines = File.ReadAllLines(dialog.FileName);
                        dataTable.Columns.AddRange(csvLines.First().Split(FieldSeparator.ToCharArray()).Select(x => new DataColumn(x)).ToArray());

                        var objectInstances = GetObjectInstances(csvLines, dataTable, type).ToArray();
                        var array = Array.CreateInstance(type, objectInstances.Count());
                        for (int i = 0; i < objectInstances.Count(); i++)
                        {
                            array.SetValue(objectInstances[i], i);
                        }
                        return array;
                    }
                }
            }
            return base.EditValue(context, provider, value);
        }
        private IEnumerable<object> GetObjectInstances(string[] csvLines, DataTable dataTable, Type type)
        {
            for (int i = 1; i < csvLines.Count(); i++)
            {
                var dataRow = dataTable.Rows.Add(csvLines.ElementAt(i).Split(FieldSeparator.ToCharArray()));
                yield return GetObjectInstance(type, dataRow);
            }
        }
        private static object GetObjectInstance(Type type, DataRow dataRow)
        {
            var obj = Activator.CreateInstance(type);

            var props = new Dictionary<string, System.Reflection.PropertyInfo>();
            foreach (var p in obj.GetType().GetProperties())
            {
                props.Add(p.Name, p);
            }
            foreach (DataColumn col in dataRow.Table.Columns)
            {
                string name = col.ColumnName;
                if (dataRow[name] != DBNull.Value && props.ContainsKey(name))
                {
                    object item = dataRow[name];
                    var p = props[name];
                    if (p != null)
                    {
                        try
                        {
                            StringArrayConverter stringArrayConverter = new StringArrayConverter();
                            Type t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                            object safeValue = null;
                            if (stringArrayConverter.CanConvertTo(t))
                            {
                                safeValue = (item == null) ? null : stringArrayConverter.ConvertTo(item, t);
                            }
                            else
                                safeValue = (item == null) ? null : Convert.ChangeType(item, t);
                            p.SetValue(obj, safeValue, null);
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                    }
                }
            }
            return obj;
        }
    }
}
