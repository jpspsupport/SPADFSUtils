# SPADFSUtils

This is a client side code sample to get authenticated to SharePoint OnPremises with ADFS as Trusted Identity Token Issuer.

This sample is not expected to be used that frequently.
Generally, SharePoint On-Premises is supposed to hold Windows Authentication endpoint as well for internal access use, such as crawling. 
And it will have federation authentication endpoint by extending the Web Application to additional IIS Site.

Thus, please reconsider if this should be used in your scenario. Most of the time, it does not have to. However if you really want to use, please go ahead.

## Reference
The orginal sample code is from Japan SharePoint Support Team Blog as below.
https://blogs.technet.microsoft.com/sharepoint_support/2016/03/15/クライアント-アプリケーションから-sharepoint-オンプレ/
