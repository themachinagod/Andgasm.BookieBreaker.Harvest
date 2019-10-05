using Andgasm.BookieBreaker.Harvest;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Andgasm.BB.Harvest.Tests
{
    [TestClass]
    public class Scratch
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            var url = @"https://httpbin.org";
            HarvestRequestManager hrm = new HarvestRequestManager(new NullLogger<HarvestRequestManager>(), 2);

            for (int i = 0; i < 2; i++)
            {
                var r = await hrm.MakeRequest(url, null);
            }
            var ranfor = hrm.RunningTimer.Elapsed;
        }
    }
}
