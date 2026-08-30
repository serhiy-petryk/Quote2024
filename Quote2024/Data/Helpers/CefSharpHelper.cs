using CefSharp;

namespace Data.Helpers
{
    public class CefSharpHelper
    {
        public class CustomRequestHandler : CefSharp.Handler.RequestHandler
        {
            private readonly string Url;
            internal readonly string PostData;
            public CustomRequestHandler(string url, string postData) : base()
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
                    return new CustomResourceRequestHandler();
                }

                //Default behaviour, url will be loaded normally.
                return null;
            }
        }

        private class CustomResourceRequestHandler : CefSharp.Handler.ResourceRequestHandler
        {

            protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
            {
                //Modify the request to add post data
                //Make sure to read https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods/POST

                var postData = new PostData();
                postData.AddData(((CustomRequestHandler)chromiumWebBrowser.RequestHandler).PostData);

                request.Method = "POST";
                request.PostData = postData;
                //Set additional Request headers as required.
                //Set the Content-Type header to whatever suites your requirement
                request.SetHeaderByName("Content-Type", "application/x-www-form-urlencoded", true);
                request.SetHeaderByName("X-Requested-With", "XMLHttpRequest", true);

                return CefReturnValue.Continue;
            }
        }
    }
}
