/*
Customer_Success_for_C_Sharp_Developers_Succinctly.
Eduardo Freitas - 2016 - http://edfreitas.me
*/

using System.Net;
using TrendsAgent;

namespace TrendsPlugin
{
    public class PluginDemo : IPluginInterface
    {
        public string[] NavigateToSite(string url)
        {
            string[] p = null;

            using (var client = new WebClient())
            {
                client.Headers["User-Agent"] =
                    "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 6.0) " +
                    "(compatible; MSIE 6.0; Windows NT 5.1; " +
                    ".NET CLR 1.1.4322; .NET CLR 2.0.50727)";

                using (var stream = client.OpenRead(url))
                {
                    string value = client.DownloadString(url);
                    p = value.Split('\r');
                }
            }

            return p;
        }
    }
}
