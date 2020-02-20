using Andgasm.Http;
using Andgasm.Http.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace Andgasm.BB.Harvest
{
    public class CookieInitialiser : ICookieInitialiser
    {
        IHttpRequestManager _httpmanager;

        public CookieInitialiser(IHttpRequestManager httpmanager)
        {
            _httpmanager = httpmanager;
        }

        public async Task<string> GetCookieFromRootDirectives()
        {
            var realisedcookie = ""; 
            var pctx = ConstructRequestContext(realisedcookie);
            var n = await  _httpmanager.Get("https://www.whoscored.com/", pctx);
            foreach (var sc in n.Headers.Where(x => x.Key == "Set-Cookie"))
            {
                foreach (var scv in sc.Value)
                {
                    var v = scv.Split(';')[0];
                    realisedcookie = $"{realisedcookie}; {v}";
                }
            }
            return realisedcookie.Substring(2);
        }

        private HttpRequestContext ConstructRequestContext(string cookieinit)
        {
            var pctx = new HttpRequestContext();
            pctx.Method = "GET";
            pctx.Accept = "";
            pctx.AddHeader("Accept", "text/html, application/xhtml+xml, image/jxr, */*");
            pctx.AddHeader("Accept-Language", "en-GB,en-US;q=0.7,en;q=0.3");
            pctx.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko");
            pctx.AddHeader("Accept-Encoding", "gzip, deflate");
            pctx.AddHeader("Host", "www.whoscored.com");
            //pctx.AddCookie("Cookie", cookieinit);
            return pctx;
        }
    }
}
