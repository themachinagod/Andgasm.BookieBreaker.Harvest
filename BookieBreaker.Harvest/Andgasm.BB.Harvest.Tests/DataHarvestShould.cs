using Andgasm.BB.Harvest.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Andgasm.BB.Harvest.Tests
{
    [TestClass]
    public class DataHarvestShould
    {
        [TestMethod]
        public async Task ExtractLastModeKey_FromValidRawHtml()
        {
            Http.HttpRequestManager rm = new Http.HttpRequestManager();
            HarvestRequestManager hrm = new HarvestRequestManager(new Microsoft.Extensions.Logging.Abstractions.NullLogger<HarvestRequestManager>(), rm);
            var ctx = HarvestHelper.ConstructRequestContext(string.Empty, null, null,
                                                            "_cmpQcif3pcsupported=1; hstpconfig=eyJJRCI6IjMwNDM1MjU0dWk1ZTQyYjg2YTZmZDRlIiwiQ1RSIjoiR0IiLCJSZWdpb24iOm51bGwsIkJyb3dzZXIiOiJDaHJvbWUiLCJQbGF0Zm9ybSI6IldpbmRvd3MiLCJNb2JpbGUiOjAsIkJvdCI6MCwicmVtb3RlX2FkZHIiOjE0MDg1NDAzMDIsIkxhc3RVcGRhdGUiOjE1ODE0MzA4OTAsIm5vY2FjaGUiOnRydWUsImVycm9yIjpmYWxzZSwibGFzdFRyYWNrZXIiOjF9; eupubconsent=BOuZcSIOuZcSIAKAAAENAAAA6AAAAA; hstpcount37584=eyJDbGljayI6MCwiQ291bnRlciI6MX0%3D; pbjs-id5id_last=Tue%2C%2011%20Feb%202020%2013%3A33%3A51%20GMT; lasttrack37584=1; googlepersonalization=OuZcSIOuZcSIgA; pbjs-id5id=%7B%22ID5ID%22%3A%22ID5-ZHMOAWwzmBD3j5pC1qBkqBsmn3GFS0pOc2ba_iiGRQ%22%2C%22ID5ID_CREATED_AT%22%3A%222020-02-11T13%3A34%3A45.696Z%22%2C%22ID5_CONSENT%22%3Atrue%2C%22CASCADE_NEEDED%22%3Atrue%2C%22ID5ID_LOOKUP%22%3Afalse%2C%223PIDS%22%3A%5B%5D%7D; incap_ses_873_774904=91bbGB2nvUNBlQe164UdDE64Ql4AAAAAOw6kRaLnvdHzrqAf8SnO+g==; __gads=ID=51e29fa544efae39:T=1539698261:S=ALNI_MYExInRy-jX0XQtWrZ_o4BP7ex9hQ; _xpid=907651299; _gat_subdomainTracker=1; _ym_visorc_55667518=w; _gat=1; _ym_isad=2; _ym_d=1581080571; _gid=GA1.2.739353075.1581427371; cto_bundle=2nAzL19KSURobEE1elg3TjFrbXI1dGdWSTlQJTJCJTJCNGdOWW5YSWJIcjh2bmtPdXp5amo1MUJqWVRQZk1hZzBMTCUyQmNBNnJpUFI4M0cwOG5uT05aekh3Z0g2WVpPbk9TZUhoSzZ3Z2NxbnVic1FpaW82cUFEMXdMSUl2OUtQWURmRkQ3RER4MzJJSmNidm5QaW1oaSUyRnNZWWh3dWhQZyUzRCUzRA; _xpkey=GK0bjJRMNw4uqzDh-9OEVK2CC-DgnVm2; _ga=GA1.2.1643971251.1539698250; _ym_uid=1581080571839310288; _ym_visorc_52685938=w; ct=GB; visid_incap_774904=EwEiUMdJQQ62RSYIrsYv/BRgPV4AAAAARkIPAAAAAACAvS2SAVP0no6DWJ9jUhYV/lMQLebFEgCP",
                                                            null, false, false, false);


            var p1 = await hrm.MakeRequest("https://www.whoscored.com/", ctx);

            var p = await hrm.MakeRequest("https://www.whoscored.com/Regions/252/Tournaments/2/Seasons/7361", ctx);


        }

        [TestMethod]
        public void ExtractNullLastModeKey_FromInvalidRawHtml()
        {
            var baseclass = InitialiseSvc();
            var data = baseclass.Object.GetLastModeKey("");
            Assert.IsNull(data);
        }

        [TestMethod]
        public void CanExecute_WhenAllPropsPopulated()
        {
            var baseclass = InitialiseSvc();
            var canexe = baseclass.Object.CanExecute();
            Assert.IsTrue(canexe); 
        }

        [TestMethod]
        public void CannotExecute_WhenRequestManagerNotInstanciated()
        {
            var baseclass = InitialiseNonExecutableSvc();
            var canexe = baseclass.Object.CanExecute();
            Assert.IsFalse(canexe);
        }

        #region Setup
        private Mock<DataHarvest> InitialiseSvc()
        {
            var m = new Mock<IHarvestRequestManager>();
            var r = new Mock<DataHarvest>(m.Object, 1);
            r.CallBase = true;
            return r;
        }

        private Mock<DataHarvest> InitialiseNonExecutableSvc()
        {
            var r = new Mock<DataHarvest>(null, 1);
            r.CallBase = true;
            return r;
        }

        private string GetTestHtml()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Andgasm.BB.Harvest.Tests.FullResponse.txt";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
        #endregion
    }
}
