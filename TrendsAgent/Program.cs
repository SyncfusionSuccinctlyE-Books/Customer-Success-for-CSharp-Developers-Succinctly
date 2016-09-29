/*
Customer_Success_for_C_Sharp_Developers_Succinctly.
Eduardo Freitas - 2016 - http://edfreitas.me
*/

using System;

namespace TrendsAgent
{
    class Program
    {
        static void Main(string[] args)
        {
            LoadRunPlugin();
            Console.ReadLine();
        }

        public static void LoadRunPlugin()
        {
            string pluginName = "TrendsPlugin.PluginDemo, TrendsPlugin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

            Type pluginType = Type.GetType(pluginName);
            object pluginInstance = Activator.CreateInstance(pluginType);
            IPluginInterface plugin = pluginInstance as IPluginInterface;

            string[] l = plugin.NavigateToSite("http://google.com");
            Console.WriteLine("Lines fetched: " + l.Length.ToString());
        }
    }
}
