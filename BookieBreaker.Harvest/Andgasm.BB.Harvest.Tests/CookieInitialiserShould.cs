using Andgasm.BB.Harvest.Interfaces;
using Andgasm.Http.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Andgasm.BB.Harvest.Tests
{
    [TestClass]
    public class CookieInitialiserShould
    {
        [TestMethod]
        public async Task ExtractFullCookie_WhenMultipleApplicableCookiesArePresent_FromRootDirectives()
        {
            var testresp = InitialiseValidMultiCookieResponse();
            var svc = InitialiseSvc(testresp);
            var data = await svc.GetCookieFromRootDirectives();
            Assert.AreEqual("incap_ses_872_774904=46iKdofQZlKhBwfGR/gZDIbqw10AAAAAyPf1oDR3j6bGbbEneFiAjA==; visid_incap_774904=I+ZGY3kRR2+LnMmqpeQhMYbqw10AAAAAQUIPAAAAAACcjeiay2I6uFNEvic7xI8D; ", data);
        }

        [TestMethod]
        public void ExtractFullCookie_WhenMultipleApplicableCookiesArePresent_FromResponseDirectives()
        {
            var resp = InitialiseTestWebResponse();
            var svc = InitialiseSvc(null);
            var data = svc.GetCookieFromResponseDirectives(resp);
            Assert.AreEqual("incap_ses_872_774904=46iKdofQZlKhBwfGR/gZDIbqw10AAAAAyPf1oDR3j6bGbbEneFiAjA==; visid_incap_774904=I+ZGY3kRR2+LnMmqpeQhMYbqw10AAAAAQUIPAAAAAACcjeiay2I6uFNEvic7xI8D; ", data);
        }

        [TestMethod]
        public async Task RefreshCookie_WhenMultipleApplicableCookiesArePresent_FromResponseContext()
        {
            var resp = InitialiseTestWebResponse();
            var ctx = InitialiseRequestContext();
            var svc = InitialiseSvc(null);
            await svc.RefreshCookieForResponseContext(resp, ctx);
            Assert.AreEqual("incap_ses_872_774904=46iKdofQZlKhBwfGR/gZDIbqw10AAAAAyPf1oDR3j6bGbbEneFiAjA==; visid_incap_774904=I+ZGY3kRR2+LnMmqpeQhMYbqw10AAAAAQUIPAAAAAACcjeiay2I6uFNEvic7xI8D; ", ctx.Cookies["Cookie"]);
        }

        #region Setup
        private CookieInitialiser InitialiseSvc(HttpResponseMessage testresp)
        {
            var r = new Mock<IHttpRequestManager>();
            r.Setup(x => x.Get(It.IsAny<string>())).ReturnsAsync(testresp);
            var svc = new CookieInitialiser(r.Object);
            return svc;
        }

        private HttpResponseMessage InitialiseValidMultiCookieResponse()
        {
            var testresp = new HttpResponseMessage(HttpStatusCode.OK);
            testresp.Headers.Add("Dead-Cookie", "testvalue; testvalue2");
            testresp.Headers.Add("Set-Cookie", "visid_incap_774904=I+ZGY3kRR2+LnMmqpeQhMYbqw10AAAAAQUIPAAAAAACcjeiay2I6uFNEvic7xI8D;");
            testresp.Headers.Add("Set-Cookie", "incap_ses_872_774904=46iKdofQZlKhBwfGR/gZDIbqw10AAAAAyPf1oDR3j6bGbbEneFiAjA==;");
            return testresp;
        }

        private WebResponse InitialiseTestWebResponse()
        {
            var hc = new WebHeaderCollection();
            hc.Add("Set-Cookie", "visid_incap_774904=I+ZGY3kRR2+LnMmqpeQhMYbqw10AAAAAQUIPAAAAAACcjeiay2I6uFNEvic7xI8D;");
            hc.Add("Set-Cookie", "incap_ses_872_774904=46iKdofQZlKhBwfGR/gZDIbqw10AAAAAyPf1oDR3j6bGbbEneFiAjA==;");

            var r = new Mock<WebResponse>();
            r.Setup(x => x.Headers).Returns(hc);
            return r.Object;
        }

        private IHarvestRequestContext InitialiseRequestContext()
        {
            var r = new HarvestRequestContext();
            r.Cookies = new Dictionary<string, string>();
            return r;
        }
        #endregion
    }
}
