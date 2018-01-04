using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Findwise.Configuration
{
    public class NullValueSubstituteAttribute : Attribute
    {
        public object Value { get; }

        public NullValueSubstituteAttribute(object value)
        {
            Value = value;
        }
    }
}
