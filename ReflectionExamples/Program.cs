using System.Diagnostics.Tracing;
/*
Customer_Success_for_C_Sharp_Developers_Succinctly.
Eduardo Freitas - 2016 - http://edfreitas.me
*/

#region using
using System.Diagnostics;
using System;
using System.Collections.Generic;

using ReflexionVendorSampleAssembly;
using System.Reflection;
#endregion

#region ns
namespace ReflectionExamples
{
    #region program
    class Program
    {
        static void Main(string[] args)
        {
            //CreateListNormally();
            //CreateListReflection();

            VendorPublicItems();
            //VendorPrivateItems();
            VendorPrivateItemsAdjusted();

            Console.ReadLine();
        }

        public static void CreateListNormally()
        {
            Stopwatch sw = Stopwatch.StartNew();

            for (int i = 0; i <= 1000; i++)
            {
                List<string> l = new List<string>();
            }

            sw.Stop();
            Console.WriteLine("CreateListNormally -> Time taken: {0}ms", sw.Elapsed.TotalMilliseconds);
        }

        public static void CreateListReflection()
        {
            Stopwatch sw = Stopwatch.StartNew();

            Type lType = typeof(List<int>);

            for (int i = 0; i <= 1000; i++)
            {
                var l = Activator.CreateInstance(lType);
            }

            sw.Stop();
            Console.WriteLine("CreateListReflection -> Time taken: {0}ms", sw.Elapsed.TotalMilliseconds);
        }

        public static void VendorPublicItems()
        {
            Console.WriteLine("VendorPublicItems");

            VendorAssembly publicVendor = new VendorAssembly();
            publicVendor.AddNewItem();
        }

        public static void VendorPrivateItemsAdjusted()
        {
            Console.WriteLine("VendorPrivateItems");

            Type vendorType = typeof(VendorAssembly);

            var act = Activator.CreateInstance(vendorType);
            VendorAssembly pv = (VendorAssembly)act;

            MethodInfo dMet = vendorType.GetMethod("AddNewItem", BindingFlags.Public | BindingFlags.Instance);

            dMet.Invoke(pv, null);
        }

        public static void VendorPrivateItems()
        {
            Console.WriteLine("VendorPrivateItems");

            Type vendorType = typeof(VendorAssembly);
            
            var act = Activator.CreateInstance(vendorType);
            VendorAssembly pv = (VendorAssembly)act;

            MethodInfo dMet = vendorType.GetMethod("AddNewItemPriv", BindingFlags.NonPublic | BindingFlags.Instance);

            dMet.Invoke(pv, null);
        }
    }
    #endregion
}
#endregion