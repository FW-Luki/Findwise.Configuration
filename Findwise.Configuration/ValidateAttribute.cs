using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Findwise.Configuration
{
    /// <summary>
    /// Defines validation condition for property.
    /// Property can have multiple validation conditions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ValidateAttribute : Attribute
    {
        /// <summary>
        /// Specifies if validation fail should be treated as an error or as a warning.
        /// Default value is error.
        /// </summary>
        public ValidateAttributeSeverity Severity { get; set; } = ValidateAttributeSeverity.Error;

        /// <summary>
        /// Name of the property which returns boolean value indicating validation result.
        /// </summary>
        public string Condition { get; /*set;*/ }

        /// <summary>
        /// Defines if specified condition is used to pass or fail the validation.
        /// Default value is for fail.
        /// </summary>
        public ValidateAttributeConditionInterpretation ConditionInterpretation { get; set; } = ValidateAttributeConditionInterpretation.Forbid;

        /// <summary>
        /// A message that is displayed when validation fails.
        /// </summary>
        public string Message { get; set; }

        public ValidateAttribute(string condition)
        {
            Condition = condition;
        }
    }



    public enum ValidateAttributeSeverity
    {
        /// <summary>
        /// Indicates that validation fail is treated as an error.
        /// </summary>
        Error,
        /// <summary>
        /// Indicates that validation fail is treated as a warning.
        /// </summary>
        Warning
    }

    public enum ValidateAttributeConditionInterpretation
    {
        /// <summary>
        /// Indicates that validation is failed if condition is met.
        /// </summary>
        Forbid,
        /// <summary>
        /// Indicates that validation is passed if condition is met.
        /// </summary>
        Allow
    }
}
