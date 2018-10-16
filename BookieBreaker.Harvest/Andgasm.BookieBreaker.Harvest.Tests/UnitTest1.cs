using Microsoft.Extensions.Logging.Abstractions;
using System;
using Xunit;

namespace Andgasm.BookieBreaker.Harvest.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async void Test1()
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
