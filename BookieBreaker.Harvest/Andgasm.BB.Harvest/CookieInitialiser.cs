using Andgasm.BB.Harvest.Interfaces;
using Andgasm.Http.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public string GetCookieFromResponseDirectives(WebResponse resp)
        {
            var realisedcookie = "";
            foreach (var sc in resp.Headers["Set-Cookie"].Split(';'))
            {
                foreach (var scv in sc.Split(','))
                {
                    if (scv.Contains("incap_"))
                    {
                        realisedcookie = $"{scv}; {realisedcookie}";
                    }
                }
            }
            return realisedcookie;
        }

        public async Task<string> GetCookieFromRootDirectives()
        {
            var realisedcookie = "";
            var n = await  _httpmanager.Get("https://www.whoscored.com/");
            foreach (var sc in n.Headers.Where(x => x.Key == "Set-Cookie"))
            {
                foreach (var scv in sc.Value)
                {
                    var v = scv.Split(';')[0];
                    realisedcookie = $"{v}; {realisedcookie}";
                }
            }
            return realisedcookie;
        }

        public async Task RefreshCookieForResponseContext(WebResponse resp, IHarvestRequestContext ctx)
        {
            var realisedcookie = "";
            if (resp != null && resp.Headers["Set-Cookie"] != null)
            {
                realisedcookie = GetCookieFromResponseDirectives(resp);
            }
            else
            {
                realisedcookie = await GetCookieFromRootDirectives();
            }
            ctx.Cookies = new Dictionary<string, string>();
            ctx.AddCookie("Cookie", realisedcookie);
        }
    }
}
