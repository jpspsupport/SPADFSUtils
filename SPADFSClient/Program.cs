using SPADFSUtils;
using System;
using System.Net;

namespace SPADFSClient
{
    class Program
    {
        static void Main(string[] args)
        {
            // Please specify ADFS Server (Do not include trailing slash /)
            string IdProviderUrl = "https://adfsserver";
            // Specify the SharePoint Server (RP) (Do not include trailing slash /)
            string RP_Uri = "https://sharepointserver";
            // Specify the realm of the RP 
            string RP_Realm = "urn:seo:sharepoint";

            // User crednetial to pass to ADFS. 
            string UserName = "Administrator";
            string Password = "PASSWORD";
            string Domain = "DOMAIN";


            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(RP_Uri);

            // Retrieve Cookie using the sample utility classes.
            var cc = new CookieContainer();
            cc.Add(new Uri(RP_Uri), new Cookie("FedAuth", SPADFSAuthUtil.GetAuthenticationCookie(IdProviderUrl, RP_Uri, RP_Realm, UserName, Password, Domain)));
            req.CookieContainer = cc;

            // Access SharePoint Server
            using (var res = (HttpWebResponse)req.GetResponse())
            {
                Console.WriteLine("Status Code: " + res.StatusCode);
                Console.WriteLine("Press [ENTER] to finish...");
                Console.ReadLine();
            }
        }
    }
   
}
