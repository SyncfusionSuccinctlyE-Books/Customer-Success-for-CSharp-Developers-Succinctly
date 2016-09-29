/*
Customer_Success_for_C_Sharp_Developers_Succinctly.
Eduardo Freitas - 2016 - http://edfreitas.me
*/

#region using
using System;
using System.Collections.Generic;
#endregion

#region ns
namespace ReflexionVendorSampleAssembly
{
    #region VendorAssembly
    public class VendorAssembly
    {
        private const string cStr = "dd-MMM-yyyy hh:mm:ss UTC";

        private DateTime dtDate;
        private List<string> pItems;

        public string Info
        {
            get { return dtDate.ToString(cStr); }
        }

        public List<string> Items
        {
            get { return pItems; }
            set { pItems = value; }
        }

        public VendorAssembly()
        {
            pItems = new List<string>();
            AddNewItem();
        }

        public void AddNewItem()
        {
            dtDate = DateTime.UtcNow;
            pItems.Add(dtDate.ToString(cStr));

            Console.WriteLine("AddNewItem -> " + pItems.Count.ToString() + " - " + dtDate.ToString(cStr));
        }

        private void AddNewItemPriv()
        {
            Console.WriteLine("AddNewItemPriv -> " + pItems.Count.ToString() + " - " + dtDate.ToString(cStr));
        }
    }
    #endregion
}
#endregion