using Andgasm.Http.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Andgasm.BB.Harvest.Interfaces
{
    public interface IHarvestRequestManager
    {
        Task<IHarvestRequestResult> MakeRequest(string url, IHttpRequestContext ctx, bool isretry = false);
    }
}
