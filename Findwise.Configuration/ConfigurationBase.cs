using System;
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
                    catch { }
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
                var serializer = GetFormatter(GetType());
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
                var deserializer = GetFormatter(typeof(T));
                if (binder != null) deserializer.Binder = binder;
                return (T)deserializer.ReadObject(reader);
            }
        }

        private static NetDataContractSerializer GetFormatter(Type type)
        {
            var formatter = new NetDataContractSerializer();
            var serializationSurrogate = type.GetCustomAttributes(false).OfType<SerializationSurrogateAttribute>().FirstOrDefault()?.SerializationSurrogateType is Type sst ?
                Activator.CreateInstance(sst) as ISerializationSurrogate : null;
            if (serializationSurrogate != null)
            {
                var surrogateSelector = new SurrogateSelector();
                surrogateSelector.AddSurrogate(type, new StreamingContext(StreamingContextStates.All), serializationSurrogate);
                formatter.SurrogateSelector = surrogateSelector;
            }
            return formatter;
        }

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
