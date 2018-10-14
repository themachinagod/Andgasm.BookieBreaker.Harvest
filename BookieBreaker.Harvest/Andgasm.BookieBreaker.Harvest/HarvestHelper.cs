using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Andgasm.BookieBreaker.Harvest.WhoScored
{
    public class HarvestHelper
    {
        public static HarvestRequestContext ConstructRequestContext(string lastmodekey, string accept, string referer, string cookiestring, string acceptlang, bool isxhr, bool upgradeinsecure, bool hascachecontrol)
        {
            // this is for direct data retrieval (player data fixtures)
            HarvestRequestContext ctx = new HarvestRequestContext();
            ctx.Method = "GET";
            ctx.Host = "www.whoscored.com";
            ctx.Accept = accept;
            ctx.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";
            ctx.Referer = referer;
            
            ctx.AddHeader("Accept-Encoding", "gzip, deflate, br");
            ctx.AddHeader("Accept-Language", acceptlang == null ? "en-GB,en;q=0.5" : acceptlang);
            if (isxhr)
            {
                ctx.AddHeader("X-Requested-With", "XMLHttpRequest");
            }
            if (upgradeinsecure)
            {
                ctx.AddHeader("Upgrade-Insecure-Requests", "1");
            }
            if (hascachecontrol)
            {
                ctx.AddHeader("Cache-Control", "max-age=0");
            }
            if (!string.IsNullOrWhiteSpace(lastmodekey))
            {
                ctx.AddHeader("Model-last-Mode", lastmodekey);
            }
            ctx.AddCookie("Cookie", cookiestring);
            ctx.Timeout = 120000;
            return ctx;
        }

        public async static Task<HtmlAgilityPack.HtmlDocument> AttemptRequest(string url, string accept, string referer, string lastmodekey, string cookiestring, string acceptlang, bool isxhr, HarvestRequestManager rm, string exitstring = "'Model-Last-Mode': '", bool upgradeinsecure = false, bool cachecontrol = false)
        {
            HtmlAgilityPack.HtmlDocument p = null;
            int attempts = 0;
            //do
            //{
                if (attempts > 0)
                {
                    Thread.Sleep(10000);
                }
                HarvestRequestContext ctx = HarvestHelper.ConstructRequestContext(lastmodekey, accept, referer, cookiestring, acceptlang, isxhr, upgradeinsecure, cachecontrol);
                p = await rm.MakeRequest(url, ctx);
                attempts++;
                //if (rm.LastRequestFailed) continue;
                if (attempts > 3) return null;
            //} while (!p.DocumentNode.InnerText.Contains(exitstring));
            return p;
        }

        public static string GetLastModeKey(string rootdoc)
        {
            var rawdata = rootdoc;
            int startindex = rawdata.IndexOf("'Model-Last-Mode': '") + 20;
            int endindex = rawdata.IndexOf("' }", startindex);
            var lastmodekey = rawdata.Substring(startindex, endindex - startindex);
            return lastmodekey;
        }

        public static void FinaliseTimer(Stopwatch timer)
        {
                timer.Stop();
                Console.WriteLine(string.Format("Completed execution of harvest in {0} seconds.", (timer.Elapsed.TotalSeconds)));
        }
    }
}
