using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Findwise.Configuration
{
    internal static class Helpers
    {
        public static IEnumerable<Type> GetDerivedTypes(this Type baseType)
        {
            var types = new HashSet<Type>();
            foreach (var implementingType in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
            {
                try
                {
                    return a.GetExportedTypes();
                }
                catch //(Exception ex)
                {
                    return Enumerable.Empty<Type>();
                }
            }).Where(t => baseType.IsAssignableFrom(t)))
            {
                types.Add(implementingType);
            }
            return types;
        }

    }
}
