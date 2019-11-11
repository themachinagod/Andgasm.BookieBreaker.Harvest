using Andgasm.Http.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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

        #region Setup
        private CookieInitialiser InitialiseSvc(HttpResponseMessage testresp)
        {
            var r = new Mock<IHttpRequestManager>();
            r.Setup(x => x.Get(It.IsAny<string>(), null)).ReturnsAsync(testresp);
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
        #endregion
    }
}
