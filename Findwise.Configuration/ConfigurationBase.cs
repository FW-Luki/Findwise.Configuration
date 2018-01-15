﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web.UI;
using System.Xml;
using System.Xml.Serialization;

namespace Findwise.Configuration
{
    /// <summary>
    /// Base class for configuration classes.
    /// Provides functionality of XML serialization/deserialization, setting default values and validating data.
    /// </summary>
    /// <remarks>If this class is to be used by other configuration class not related with connector this class as well as <see cref="Security.Cryptography"/> should be extracted to a separate assembly.</remarks>
    public abstract class ConfigurationBase
    {
        /// <summary>
        /// Creates new instance of the <see cref="ConfigurationBase"/> class.
        /// </summary>
        public ConfigurationBase()
        {
            SetDefaultPropertyValues();
        }

        /// <summary>
        /// Sets default values to all properties that have <see cref="DefaultValueAttribute"/> attribute defined.
        /// If for some reason value cannot be set, no action is taken.
        /// </summary>
        public virtual void SetDefaultPropertyValues()
        {
            foreach (var property in this.GetType().GetProperties())
            {
                var attributes = property.GetCustomAttributes(false);
                var defaultValueAttribute = attributes.OfType<DefaultValueAttribute>().FirstOrDefault();
                if (defaultValueAttribute != null)
                {
                    try
                    {
                        property.SetValue(this, defaultValueAttribute.Value, null);
                    }
                    catch
                    { }
                }
            }
        }

        /// <summary>
        /// Returns true if all properties have values satisfying all their validation conditions, otherwise false.
        /// </summary>
        /// <param name="errorMessages">Returns list of messages of errors occured during the validation.</param>
        /// <param name="warningMessages">Returns list of messages of warnings occured during the validation.</param>
        /// <returns><see cref="bool"/></returns>
        public bool ValidateData(out IEnumerable<string> errorMessages, out IEnumerable<string> warningMessages)
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            var errorsOccured = false;
            foreach (var property in this.GetType().GetProperties())
            {
                var attributes = property.GetCustomAttributes(false);
                foreach(var validateAttribute in attributes.OfType<ValidateAttribute>())
                {
                    try
                    {
                        var passFail = (bool)DataBinder.Eval(this, validateAttribute.Condition);
                        if (passFail ^ (validateAttribute.ConditionInterpretation == ValidateAttributeConditionInterpretation.Allow))
                        {
                            if (validateAttribute.Severity == ValidateAttributeSeverity.Error)
                            {
                                errors.Add(validateAttribute.Message);
                                errorsOccured = true;
                            }
                            else if (validateAttribute.Severity == ValidateAttributeSeverity.Warning)
                            {
                                warnings.Add(validateAttribute.Message);
                            } 
                        }
                    }
                    catch (Exception ex)
                    {
                        warnings.Add($"An error occured while validating value for property {property.Name}.{Environment.NewLine}{ex.Message}");
                    }
                }
            }
            errorMessages = errors.AsEnumerable();
            warningMessages = warnings.AsEnumerable();
            return errorsOccured;
        }

        /// <summary>
        /// Serializes this object to XML data structure and returns it as <see cref="string"/>.
        /// </summary>
        /// <returns>Returns <see cref="string"/> containing data of the serialized object.</returns>
        /// <exception cref="InvalidDataContractException"></exception>
        /// <exception cref="SerializationException"></exception>
        public string Serialize()
        {
            using (var output = new StringWriter())
            using (var writer = new XmlTextWriter(output) { Formatting = Formatting.Indented })
            {
                var myType = this.GetType();
                var serializer = new NetDataContractSerializer();
                serializer.WriteObject(writer, this);
                return output.GetStringBuilder().ToString();
            }
        }

        /// <summary>
        /// Deserializes passed XML structure data as <see cref="T"/> object and returns new instance of this object.
        /// If deserialization fails an exception is thrown.
        /// </summary>
        /// <typeparam name="T">Type of the object to which data should be deserialized</typeparam>
        /// <param name="data"><see cref="string"/> object containing serialized XML data structure</param>
        /// <returns>Returns new instance of the <see cref="T"/> class.</returns>
        /// <exception cref="ArgumentNullException">Occurs when passed data is null.</exception>
        /// <exception cref="SerializationException">Occurs when loaded data cannot be deserialized to desired type.</exception>
        public static T Deserialize<T>(string data, SerializationBinder binder = null) where T : ConfigurationBase
        {
            using (var input = new StringReader(data))
            using (var reader = new XmlTextReader(input))
            {
                var deserializer = new NetDataContractSerializer();
                if (binder != null) deserializer.Binder = binder;
                return (T)deserializer.ReadObject(reader);
            }
        }

        //protected static IEnumerable<Type> GetKnownTypes(object obj, ICollection<object> objects)
        //{
        //    if (objects == null) objects = new HashSet<object>(new ReferenceComparer());
        //    if (!objects.Contains(obj))
        //    {
        //        var type = obj.GetType();

        //        objects.Add(obj);
        //        yield return type;

        //        var properties = type.GetProperties();
        //        if (properties != null && properties.Any())
        //        {
        //            foreach (var prop in properties)
        //            {
        //                if (prop.GetMethod != null && prop.SetMethod != null)
        //                {
        //                    var value = prop.GetValue(obj).GetType();
        //                    yield return value;
        //                }
        //            }
        //        }
        //    }
        //}
        //private class ReferenceComparer : IEqualityComparer<object>
        //{
        //    public new bool Equals(object x, object y)
        //    {
        //        return ReferenceEquals(x, y);
        //    }
        //    public int GetHashCode(object obj)
        //    {
        //        return obj.GetHashCode();
        //    }
        //}
        //private static IEnumerable<Type> GetKnownTypes<T>()
        //{
        //    return GetKnownTypes(typeof(T));
        //}
        //public static IEnumerable<Type> GetKnownTypes(Type baseType, HashSet<Type> alreadyKnownTypes = null)
        //{
        //    //if (alreadyKnownTypes?.Contains(baseType) ?? false) return Enumerable.Empty<Type>();
        //    if (alreadyKnownTypes == null) alreadyKnownTypes = new HashSet<Type>();
        //    alreadyKnownTypes.Add(baseType);

        //    var types = new HashSet<Type>();
        //    if (!baseType.IsAbstract && !baseType.IsInterface) types.Add(baseType);

        //    AddRangeToSet(types, GetImplementingTypes(baseType));
        //    AddRangeToSet(types, GetContainedTypes(baseType));
        //    foreach (var type in types.ToArray())
        //    {
        //        if (!alreadyKnownTypes.Contains(type))
        //            AddRangeToSet(types, GetKnownTypes(type, alreadyKnownTypes));
        //    }
        //    return types;
        //}
        //private static void AddRangeToSet<T>(HashSet<T> set, IEnumerable<T> range)
        //{
        //    foreach (var item in range)
        //    {
        //        set.Add(item);
        //    }
        //}
        //private static IEnumerable<Type> GetImplementingTypes(Type baseType)
        //{
        //    if (baseType.IsInterface || baseType.IsAbstract)
        //    {
        //        return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
        //        {
        //            try
        //            {
        //                return a.GetExportedTypes();
        //            }
        //            catch //(Exception ex)
        //            {
        //                return Enumerable.Empty<Type>();
        //            }
        //        }).Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);
        //    }
        //    else
        //    {
        //        return Enumerable.Repeat(baseType, 1);
        //    }
        //}
        //private static IEnumerable<Type> GetContainedTypes(Type baseType)
        //{
        //    return baseType.GetProperties().Where(p => !p.GetCustomAttributes(false).OfType<XmlIgnoreAttribute>().Any()
        //                                            && p.GetMethod != null && p.SetMethod != null).SelectMany(p =>
        //    {
        //        var type = p.PropertyType;
        //        var types = Enumerable.Repeat(type, 1);

        //        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
        //        {
        //            if (type.IsArray)
        //                types = types.Concat(new[] { type.GetElementType() });
        //            else
        //                types = types.Concat(new[] { type.GetGenericArguments().FirstOrDefault() });
        //        }

        //        return types.Where(t => t != null);
        //    });
        //}

        #region Serialization helper
        /// <summary>
        /// Indicates if the object is currently being serialized.
        /// </summary>
        protected bool IsSerializing { get; private set; }
        /// <summary>
        /// Indicates if the object is currently being deserialized.
        /// </summary>
        protected bool IsDeserializing { get; private set; }

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            IsSerializing = true;
        }

        [OnSerialized]
        internal void OnSerializedMethod(StreamingContext context)
        {
            IsSerializing = false;
        }

        [OnDeserializing]
        internal void OnDeserializingMethod(StreamingContext context)
        {
            IsDeserializing = true;
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            IsDeserializing = false;
        }
        #endregion

        #region Property Binding
        [NonSerialized]
        protected Dictionary<System.Windows.Forms.Binding, System.Windows.Forms.Control> _propertyBindings = new Dictionary<System.Windows.Forms.Binding, System.Windows.Forms.Control>();

        /// <summary>
        /// Binds specified property of this configuration object to specified property of the passed control and adds created binding to the passed control's data bindings.
        /// This cause immediate write of this configuration property value to the control property.
        /// </summary>
        /// <param name="control"><see cref="System.Windows.Forms.Control"/> where binding is to be added.</param>
        /// <param name="controlPropertyName">Name of the control property that is to be bound.</param>
        /// <param name="objectPropertyName">Name of this configuration object property that is to be bound.</param>
        /// <remarks>Use nameof operator to get property names for best source code maintainability.</remarks>
        public void AddPropertyBinding(System.Windows.Forms.Control control, string controlPropertyName, string objectPropertyName)
        {
            var binding = new System.Windows.Forms.Binding(controlPropertyName, this, objectPropertyName, false, System.Windows.Forms.DataSourceUpdateMode.Never);
            _propertyBindings.Add(binding, control);
            control.DataBindings.Add(binding);
            binding.ControlUpdateMode = System.Windows.Forms.ControlUpdateMode.Never; //It is important to set this property after adding data binding to control, otherwise the control property won't get correct value.
        }

        /// <summary>
        /// Removes binding with specified property names from specified control data bindings collection.
        /// </summary>
        /// <param name="control"><see cref="System.Windows.Forms.Control"/> whose data binding is to be removed.</param>
        /// <param name="controlPropertyName">Name of the control property to find the binding that is to be removed.</param>
        /// <param name="objectPropertyName">Name of this configuration object property to find the binding that is to be removed.</param>
        /// <returns>Returns true if the binding is successfully found and removed, otherwise false.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="InvalidOperationException">Occurs when no binding or more than one binding is found with the specified conditions.</exception>
        /// <remarks>Use nameof operator to get property names for best source code maintainability.</remarks>
        public bool RemovePropertyBinding(System.Windows.Forms.Control control, string controlPropertyName, string objectPropertyName)
        {
            var binding = _propertyBindings.Single(b => b.Key.PropertyName == controlPropertyName && b.Key.BindingMemberInfo.BindingField == objectPropertyName).Key;
            control.DataBindings.Remove(binding);
            return _propertyBindings.Remove(binding);
        }

        /// <summary>
        /// Reads the current values from all the control bound properties and writes them to their respective properties in this configuration object.
        /// </summary>
        public void ApplyValuesFromControl()
        {
            foreach (var binding in _propertyBindings.Keys)
            {
                binding.WriteValue();
            }
        }
        #endregion
    }
}
