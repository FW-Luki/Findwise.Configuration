using System;
using System.Collections.Generic;
using System.Drawing;
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

        public static Color ToColor(this ConsoleColor consoleColor)
        {
            var isBright = consoleColor.HasFlag((ConsoleColor)8);
            var hasRed = consoleColor.HasFlag((ConsoleColor)4);
            var hasGreen = consoleColor.HasFlag((ConsoleColor)2);
            var hasBlue = consoleColor.HasFlag((ConsoleColor)1);
            return Color.FromArgb((hasRed ? 128 : 0) + (isBright ? 127 : 0), (hasGreen ? 128 : 0) + (isBright ? 127 : 0), (hasBlue ? 128 : 0) + (isBright ? 127 : 0));
        }
    }
}
