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
using System.Collections;
using System.Reflection;

namespace Findwise.Configuration.TypeEditors
{
    public class CsvLoaderEditor : ArrayEditor
    {
        public CsvLoaderEditor(Type type) : base(type)
        {
        }

        public static string FieldSeparator { get => ";"; }
        public static string ItemSeparator { get => ","; }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        protected override Type[] CreateNewItemTypes()
        {
            var types = base.CreateNewItemTypes();
            return types.Concat(new[] { typeof(SuperSpecialInstanceCreator) }).ToArray();
        }

        protected override object CreateInstance(Type itemType)
        {
            if (itemType == typeof(SuperSpecialInstanceCreator))
                return new SuperSpecialInstanceCreator().CreateInstance(CreateNewItemTypes().First());
            else
                return base.CreateInstance(itemType);
        }

        protected override IList GetObjectsFromInstance(object instance)
        {
            if (instance is IList list)
                return list;
            else
                return base.GetObjectsFromInstance(instance);
        }


        private class SuperSpecialInstanceCreator
        {
            public object CreateInstance(Type propertyType)
            {
                //if (propertyType.IsArray)
                //{
                var type = propertyType; //.GetElementType();
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
                    //}
                }
                return null;
            }

            public override string ToString()
            {
                return "Create from CSV file...";
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
                                var safeValue = ConvertValue(p, item);
                                p.SetValue(obj, safeValue, null);
                            }
                            catch { }
                        }
                    }
                }
                return obj;
            }
            private static object ConvertValue(PropertyInfo propertyInfo, object value)
            {
                foreach (var converter in GetTypeConverters(propertyInfo))
                {
                    if (converter.CanConvertFrom(value.GetType()))
                        return converter.ConvertFrom(value);
                }
                var type = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                return (value == null) ? null : Convert.ChangeType(value, type);
            }

            private static IEnumerable<TypeConverter> GetTypeConverters(PropertyInfo propertyInfo)
            {
                foreach (TypeConverterAttribute a in propertyInfo.GetCustomAttributes(typeof(TypeConverterAttribute), true))
                {
                    if (Activator.CreateInstance(Type.GetType(a.ConverterTypeName)) is TypeConverter converter)
                        yield return converter;
                }
                yield return GetDefaultTypeConverter(propertyInfo.PropertyType);
            }
            private static TypeConverter GetDefaultTypeConverter(Type type)
            {
                return TypeDescriptor.GetConverter(type);
            }
        }
    }
}
