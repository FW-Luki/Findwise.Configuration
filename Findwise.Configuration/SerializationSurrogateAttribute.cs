using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Findwise.Configuration
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SerializationSurrogateAttribute : Attribute
    {
        public Type SerializationSurrogateType { get; }
        public SerializationSurrogateAttribute(Type type)
        {
            if (typeof(ISerializationSurrogate).IsAssignableFrom(type))
                SerializationSurrogateType = type;
            else
                SerializationSurrogateType = null;
        }
    }
}
