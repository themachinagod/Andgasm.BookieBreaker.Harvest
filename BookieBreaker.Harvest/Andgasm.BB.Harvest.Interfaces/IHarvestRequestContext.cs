using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Andgasm.BB.Harvest.Interfaces
{
    public interface IHarvestRequestContext
    {
        int Timeout { get; set; }
        string Method { get; set; }
        string Host { get; set; }
        string Accept { get; set; }
        string UserAgent { get; set; }
        string Referer { get; set; }
        List<string> SetCookies { get; set; }
        Dictionary<string, string> Headers { get; set; }
        Dictionary<string, string> Cookies { get; set; }
    }
}
