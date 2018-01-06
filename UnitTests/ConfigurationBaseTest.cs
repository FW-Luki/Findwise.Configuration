using System;
using System.Collections.Generic;
using System.Linq;
using Findwise.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class ConfigurationBaseTest
    {
        [TestMethod]
        public void GetKnownTypesTest()
        {
            var expectedTypes = new[] { typeof(Ciasto),
                                        typeof(ISkładnik[]),
                                        typeof(ISkładnik),
                                        typeof(double),
                                        typeof(Mąka),
                                        typeof(Tłuszcz),
                                        typeof(Margaryna),
                                        typeof(Olej),
                                        typeof(Tort),
                                        typeof(IEnumerable<Dekoracja>),
                                        typeof(Dekoracja),
                                        typeof(Świeczka),
                                        typeof(ŚwieczkaWybuchowa),
                                        typeof(int),
                                        typeof(Int64),
                                        typeof(bool),
                                        typeof(object) };
            var discoveredTypes = ConfigurationBase.GetKnownTypes(typeof(Ciasto));
            Console.WriteLine(string.Join(Environment.NewLine, discoveredTypes.Select(t => t.Name)));
            if (!discoveredTypes.All(t => expectedTypes.Contains(t))) Assert.Fail(); 
        }
    }




    public abstract class Ciasto
    {
        public ISkładnik[] Składniki { get; set; }
    }

    public interface ISkładnik
    {
        double Ilość { get; set; }
        //Ciasto Ciasto { get; set; }
    }

    public class Mąka : ISkładnik
    {
        public double Ilość { get; set; }
        //public Ciasto Ciasto { get; set; }
    }

    public abstract class Tłuszcz : ISkładnik
    {
        public double Ilość { get; set; }
        //public Ciasto Ciasto { get; set; }
    }

    public class Margaryna : Tłuszcz
    {
    }

    public class Olej : Tłuszcz
    {
    }

    public class Tort : Ciasto
    {
        public IEnumerable<Dekoracja> Dekoracje { get; set; }
    }

    public abstract class Dekoracja
    {
    }

    public class Świeczka : Dekoracja
    {
    }

    public class ŚwieczkaWybuchowa : Świeczka
    {
    }

}
