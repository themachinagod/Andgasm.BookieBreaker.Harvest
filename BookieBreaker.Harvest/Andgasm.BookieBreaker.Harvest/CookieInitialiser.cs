
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Andgasm.BookieBreaker.Harvest.WhoScored
{
    public enum FiddlerVersion
    {
        Fiddler2,
        Fiddler4
    }

    public class CookieInitialiser : IDisposable
    {
        const string FiddlerStartCommand = "start";
        const string FiddlerStopCommand = "stop";
        const string FiddlerDumpCommand = "dump";

        string _zipPath = @"C:\Users\{0}\Documents\Fiddler2\Captures\dump.saz";
        string _fiddlerPath = @"C:\Users\machi\AppData\Local\Programs\Fiddler\Fiddler.exe";
        string _fiddlerExecActionPath = @"C:\Users\machi\AppData\Local\Programs\Fiddler\ExecAction.exe";
        string _siteRoot = @"www.whoscored.com";
        Process _fiddlerproc = null;
        Process _rootbrowserproc = null;

        public string RealisedCookie { get; set; }

        public CookieInitialiser(FiddlerVersion fv)
        {
            _fiddlerExecActionPath = string.Format(_fiddlerExecActionPath, fv.ToString());
            _fiddlerPath = string.Format(_fiddlerPath, fv.ToString());
            _zipPath = string.Format(_zipPath, Environment.UserName);
        }

        public void Execute()
        {

            StartFiddlerClient();
            NavigateToSiteRoot();
            StartCapturingHttpSessions();
            InitialiseCookieData();

        }

        private void StartFiddlerClient()
        {

            // TODO: i would like this method to wait until complete without need for sleep
            // launch fiddler client 
            Console.WriteLine("Starting Fiddler client.");
            _fiddlerproc = Process.Start(_fiddlerPath);
            _fiddlerproc.WaitForInputIdle();
            Thread.Sleep(10000);
            Console.WriteLine("Successfully started Fiddler client.");

        }

        private void StartCapturingHttpSessions()
        {

            // TODO: i would like this method to wait until complete without need for sleep
            // start capture of http events
            Console.WriteLine("Starting Fiddler monitoring of HTTP sessions on port 8888.");
            using (var startproc = Process.Start(_fiddlerExecActionPath, FiddlerStartCommand))
            {
                startproc.WaitForExit();
                Thread.Sleep(1000);
                Console.WriteLine("Successfully started Fiddler monitoring HTTP sessions on port 8888.");
            }

        }

        private void InitialiseCookieData()
        {

            // if old dump exists - delete
            if (File.Exists(_zipPath)) File.Delete(_zipPath);

            // dump current fiddler session data & attempt to read
            Console.WriteLine("Starting dump of captured HTTP sessions to SAZ file.");
            using (var dumpproc = Process.Start(_fiddlerExecActionPath, FiddlerDumpCommand))
            {
                dumpproc.WaitForExit();
                ReadFromDump();
                if (RealisedCookie == null)
                {
                    InitialiseCookieData();
                }
            }

        }

        private void ReadFromDump()
        {

            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(_zipPath))
                {
                    Console.WriteLine("Successfully dumped captured HTTP sessions to file.");
                    GetCookieDataFromSAZArchive(archive);
                }
            }
            catch
            {
                // file is locked - wait and try again
                Console.WriteLine("Dump file still in use, awaiting completion of data dump before retry!");
                Thread.Sleep(1000);
                ReadFromDump();
            }

        }

        private void NavigateToSiteRoot()
        {

            // navigate to url
            _rootbrowserproc = Process.Start(@"C:\Program Files\internet explorer\iexplore.exe", _siteRoot);
            _rootbrowserproc.WaitForInputIdle();
            Thread.Sleep(10000);
            Console.WriteLine("Successfully navigated to site root.");

        }

        private void GetCookieDataFromSAZArchive(ZipArchive archive)
        {

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    using (StreamReader sr = new StreamReader(entry.Open()))
                    {
                        var data = sr.ReadToEnd();
                        if (data.Contains(@"GET https://www.whoscored.com/Accounts/Info HTTP/1.1"))
                        {
                            // get basic cookie data
                            int cookiestartindex = data.IndexOf("Cookie:");
                            string cookiedata = data.Substring(cookiestartindex);

                            // ensure no data after cookie propper - if so filter
                            int cookieendindex = data.IndexOf("If-Modified-Since:", cookiestartindex);
                            if (cookieendindex > 0)
                            {
                                cookiedata = data.Substring(cookiestartindex, (cookieendindex - cookiestartindex));
                            }

                            // clean cookie data
                            RealisedCookie = cookiedata.Replace("Cookie: ", "");
                            RealisedCookie = RealisedCookie.Replace("\n", " ");
                            RealisedCookie = RealisedCookie.Replace("\r", "");
                            Console.WriteLine(string.Format("Successfully realised session cookie: {0}.", RealisedCookie));
                        }
                    }
                }
            }
            if (RealisedCookie == null) throw new Exception("Failed to identify session cookie!");

        }

        public void Dispose()
        {
            //_fiddlerproc.CloseMainWindow();
            //_rootbrowserproc.Kill();
            //_fiddlerproc.Close();
            //_fiddlerproc.Dispose();
            //_rootbrowserproc.Dispose();
        }
    }
}
