
using Andgasm.BB.Harvest.Interfaces;
using Andgasm.Http.Interfaces;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Andgasm.BB.Harvest
{
    public interface ICookieInitialiser
    {
        Task<string> GetCookieFromRootDirectives();
    }
}
