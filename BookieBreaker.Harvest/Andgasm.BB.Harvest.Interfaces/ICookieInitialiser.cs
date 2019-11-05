
using Andgasm.BB.Harvest.Interfaces;
using System.Net;
using System.Threading.Tasks;

namespace Andgasm.BB.Harvest
{
    public interface ICookieInitialiser
    {
        string GetCookieFromResponseDirectives(WebResponse resp);
        Task<string> GetCookieFromRootDirectives();
        Task RefreshCookieForResponseContext(WebResponse resp, IHarvestRequestContext ctx);
    }
}
