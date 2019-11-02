using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Andgasm.BB.Harvest.Interfaces
{
    public interface IHarvestRequestResult
    {
        string InnerHtml { get; set; }
        string InnerText { get; set; }
    }
}
