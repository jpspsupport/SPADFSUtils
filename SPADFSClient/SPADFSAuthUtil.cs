/*
 This Sample Code is provided for the purpose of illustration only and is not intended to be used in a production environment. 
 THIS SAMPLE CODE AND ANY RELATED INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, 
 INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.  

 We grant you a nonexclusive, royalty-free right to use and modify the sample code and to reproduce and distribute the object 
 code form of the Sample Code, provided that you agree: 
    (i)   to not use our name, logo, or trademarks to market your software product in which the sample code is embedded; 
    (ii)  to include a valid copyright notice on your software product in which the sample code is embedded; and 
    (iii) to indemnify, hold harmless, and defend us and our suppliers from and against any claims or lawsuits, including 
          attorneys' fees, that arise or result from the use or distribution of the sample code.

Please note: None of the conditions outlined in the disclaimer above will supercede the terms and conditions contained within 
             the Premier Customer Services Description.
 */
using Microsoft.IdentityModel.Protocols.WSTrust;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Xml;

namespace SPADFSUtils
{
    static class SPADFSAuthUtil
    {
        public static string GetAuthenticationCookie(string IdProviderUrl, string RP_Uri, string RP_Realm, string UserName, string Password, string Domain)
        {
            //-----------------------------------------------------------------------------
            // Pass the Windows auth credential to the endpoint of the Identity Provider (ADFS) 
            //-----------------------------------------------------------------------------
            var binding = new WS2007HttpBinding(SecurityMode.TransportWithMessageCredential);
            binding.Security.Message.EstablishSecurityContext = false;
            var endpoint = new EndpointAddress(IdProviderUrl + "/adfs/services/trust/13/windowsmixed");
            RequestSecurityTokenResponse rstr;
            using (var factory = new Microsoft.IdentityModel.Protocols.WSTrust.WSTrustChannelFactory(binding, endpoint))
            {
                factory.Credentials.Windows.ClientCredential =
                    new NetworkCredential(UserName, Password, Domain);
                factory.TrustVersion = System.ServiceModel.Security.TrustVersion.WSTrust13;

                var rst = new RequestSecurityToken();

                rst.AppliesTo = new EndpointAddress(RP_Realm);
                rst.KeyType = WSTrust13Constants.KeyTypes.Bearer;
                rst.RequestType = WSTrust13Constants.RequestTypes.Issue;

                var channel = (Microsoft.IdentityModel.Protocols.WSTrust.WSTrustChannel)factory.CreateChannel();
                channel.Issue(rst, out rstr);
            }

            //-----------------------------------------------------------------------------
            // Send the security token acquired from the Identity Provider (ADFS)
            //  to Replying Party (SharePoint)
            // And acquire authorization cookie from SharePoint Server.
            //-----------------------------------------------------------------------------
            using (var output = new StringWriterUtf8(new StringBuilder()))
            {
                using (var xmlwr = XmlWriter.Create(output))
                {
                    WSTrust13ResponseSerializer rs = new WSTrust13ResponseSerializer();
                    rs.WriteXml(rstr, xmlwr, new WSTrustSerializationContext());
                }
                var str = string.Format("wa=wsignin1.0&wctx={0}&wresult={1}",
                    HttpUtility.UrlEncode(RP_Uri + "/_layouts/15/Authenticate.aspx?Source=%2F"),
                    HttpUtility.UrlEncode(output.ToString()));

                var req = (HttpWebRequest)HttpWebRequest.Create(RP_Uri + "/_trust/");

                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                req.CookieContainer = new CookieContainer();
                req.AllowAutoRedirect = false;

                using (var res = req.GetRequestStream())
                {
                    byte[] postData = Encoding.UTF8.GetBytes(str);
                    res.Write(postData, 0, postData.Length);
                }

                using (var res = (HttpWebResponse)req.GetResponse())
                {
                    return res.Cookies["FedAuth"].Value;
                }
            }
        }



        internal class StringWriterUtf8 : StringWriter
        {
            public StringWriterUtf8(StringBuilder sb)
                : base(sb)
            { }

            public override Encoding Encoding
            {
                get
                {
                    return Encoding.UTF8;
                }
            }
        }
    }
}