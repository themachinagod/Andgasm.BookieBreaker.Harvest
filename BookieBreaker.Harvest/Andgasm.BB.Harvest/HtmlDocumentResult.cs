using Andgasm.BB.Harvest.Interfaces;

namespace Andgasm.BB.Harvest
{
    public class HarvestRequestResult : IHarvestRequestResult
    {
        public string InnerHtml { get; set; }
        public string InnerText { get; set; }
    }
}
