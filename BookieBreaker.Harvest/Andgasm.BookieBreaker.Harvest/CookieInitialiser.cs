
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Andgasm.BookieBreaker.Harvest.WhoScored
{
    public class CookieInitialiser
    {
        public static string GetCookieFromResponseDirectives(WebResponse resp)
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

        public static async Task<string> GetCookieFromRootDirectives()
        {
            var realisedcookie = "";
            var n = await Http.HttpRequestFactory.Get("https://www.whoscored.com/");
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

        public static async Task RefreshCookieForResponseContext(WebResponse resp, HarvestRequestContext ctx)
        {
            var realisedcookie = "";
            if (resp != null && resp.Headers["Set-Cookie"] != null)
            {
                realisedcookie = CookieInitialiser.GetCookieFromResponseDirectives(resp);
            }
            else
            {
                realisedcookie = await CookieInitialiser.GetCookieFromRootDirectives();
            }
            ctx.Cookies.Clear();
            ctx.AddCookie("Cookie", realisedcookie);
        }
    }
}
