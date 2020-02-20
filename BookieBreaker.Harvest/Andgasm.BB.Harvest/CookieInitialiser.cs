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
            var realisedcookie = "incap_ses_873_774904=EhSmQCeluBiOhwgB7oUdDFhcTl4AAAAAmMbOzvUcDK2F8K4stCSdrQ=="; 
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
            return realisedcookie;
        }

        private HttpRequestContext ConstructRequestContext(string cookieinit)
        {
            var pctx = new HttpRequestContext();
            pctx.Method = "GET";
            pctx.Accept = "";
            pctx.AddHeader("Accept", "text/html, application/xhtml+xml, image/jxr, */*");
            pctx.AddHeader("Accept-Language", "en-GB,en-US;q=0.7,en;q=0.3");
            pctx.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko");
            pctx.AddHeader("Accept-Encoding", "");
            pctx.AddHeader("Host", "www.whoscored.com");
            pctx.AddCookie("Cookie", cookieinit);
            return pctx;
        }
    }
}
