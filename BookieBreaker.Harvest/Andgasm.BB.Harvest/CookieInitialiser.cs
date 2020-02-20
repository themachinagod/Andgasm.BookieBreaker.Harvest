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
            var realisedcookie = "pbjs-id5id_last=Thu%2C%2020%20Feb%202020%2009%3A57%3A45%20GMT; pbjs-id5id=%7B%22ID5ID%22%3A%22ID5-ZHMOYn-NvL-hjpd7I6jgN7a1vuGsXcCAMsqaZFiudw%22%2C%22ID5ID_CREATED_AT%22%3A%222020-02-20T09%3A58%3A24.429Z%22%2C%22ID5_CONSENT%22%3Afalse%7D; __gads=ID=fbda49da08a16d23:T=1582026258:S=ALNI_MbKMvfDqfpJxZrHWVaDYDl6eiwmYw; _xpid=932220843; _ym_visorc_55667518=w; _ym_isad=2; _ym_d=1582026258; _gid=GA1.2.1044782333.1582192645; _xpkey=zKZ-UiElInrjmAwSyLID_i0GgHXXs9Px; _ga=GA1.2.1822880755.1582026257; _ym_uid=158202625888574105; _ym_visorc_52685938=w; ct=GB;";
            var pctx = new HttpRequestContext();
            pctx.Method = "GET";
            pctx.AddHeader("Accept", "text/html, application/xhtml+xml, image/jxr, */*");
            pctx.AddHeader("Accept-Language", "en-GB,en-US;q=0.7,en;q=0.3");
            pctx.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko");
            pctx.AddHeader("Accept-Encoding", "gzip, deflate");
            pctx.AddHeader("Host", "www.whoscored.com");
            pctx.AddCookie("Cookie", realisedcookie);

            var n = await  _httpmanager.Get("https://www.whoscored.com/", pctx);
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
    }
}
