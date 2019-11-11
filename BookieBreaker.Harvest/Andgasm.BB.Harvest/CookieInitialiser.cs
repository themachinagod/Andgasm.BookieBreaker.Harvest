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
    }
}
