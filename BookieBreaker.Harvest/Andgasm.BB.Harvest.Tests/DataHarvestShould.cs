using Andgasm.BB.Harvest.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using System.Reflection;

namespace Andgasm.BB.Harvest.Tests
{
    [TestClass]
    public class DataHarvestShould
    {
        [TestMethod]
        public void ExtractLastModeKey_FromValidRawHtml()
        {
            var baseclass = InitialiseSvc();
            var data = baseclass.Object.GetLastModeKey(GetTestHtml());
            Assert.AreEqual("4TtT/8/gkxRvcHRZK/3O3FqFV8kGqeyWySfI3k6LnKQ=", data);
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
