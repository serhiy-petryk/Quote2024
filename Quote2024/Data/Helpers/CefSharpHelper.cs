using System;
using CefSharp;

namespace Data.Helpers
{
    public class CefSharpHelper
    {
        public class PostRequestHandler : CefSharp.Handler.RequestHandler
        {
            private readonly string Url;
            internal readonly string PostData;
            public PostRequestHandler(string url, string postData) : base()
            {
                Url = url;
                PostData = postData;
            }
            protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
            {
                //Where possible only intercept specific Url's
                //Load http://httpbin.org/post in the browser and you'll
                //see the post data
                if (request.Url == Url)
                {
                    return new PostResourceRequestHandler();
                }

                //Default behaviour, url will be loaded normally.
                return null;
            }
        }

        private class PostResourceRequestHandler : CefSharp.Handler.ResourceRequestHandler
        {

            protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
            {
                //Modify the request to add post data
                //Make sure to read https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods/POST
                var uri = new Uri(((CefSharp.Core.Request)request).Url);
                var origin = uri.GetLeftPart(UriPartial.Authority);

                var postData = new PostData();
                postData.AddData(((PostRequestHandler)chromiumWebBrowser.RequestHandler).PostData);

                request.Method = "POST";
                request.PostData = postData;
                //Set additional Request headers as required.
                //Set the Content-Type header to whatever suites your requirement
                request.SetHeaderByName("Content-Type", "application/x-www-form-urlencoded", true);
                request.SetHeaderByName("X-Requested-With", "XMLHttpRequest", true);

                request.SetHeaderByName("Accept", "*/*", true);
                request.SetHeaderByName("Origin", origin, true);
                request.SetHeaderByName("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36", true);
                // request.SetReferrer("https://www.investing.com/stock-split-calendar/", ReferrerPolicy.Default);

                return CefReturnValue.Continue;
            }
        }
    }
}
