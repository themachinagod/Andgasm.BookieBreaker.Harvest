using Andgasm.BookieBreaker.Harvest.WhoScored;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Andgasm.BookieBreaker.Harvest
{
    // use as singleton to track requests and apply throttles
    // TODO: the _requesttimes collection will grow and grow unless we trim this every so often without losing data accuracy
    public class HarvestRequestManager 
    {
        #region Fields
        List<TimeSpan> _requestTimes = new List<TimeSpan>();
        Random random = new Random();
        #endregion

        #region Properties
        ILogger<HarvestRequestManager> _logger;
        public bool LastRequestFailed { get; set; }
        private double MaxRequestsPerMin { get; set; }
        public Stopwatch RunningTimer { get; set; }

        public double BenchmarkAvgRequestLength 
        {
            get
            {
                return (60000D / MaxRequestsPerMin); // milliseconds
            }
        }

        public double AvgRequestLength 
        {
            get
            {
                if (_requestTimes == null || _requestTimes.Count == 0) return BenchmarkAvgRequestLength;
                return _requestTimes.Average(x => x.TotalMilliseconds);  // milliseconds
            }
        }

        public TimeSpan CurrentThrottlePause 
        {
            get
            {
                var throttlesecs = (BenchmarkAvgRequestLength - AvgRequestLength);
                var throttlemsecs = (int)(throttlesecs);
                if (throttlemsecs > 0) return new TimeSpan(0, 0, 0, 0, throttlemsecs);
                else return new TimeSpan(0, 0, 0);
            }
        }
        #endregion

        #region Constructors
        public HarvestRequestManager(ILogger<HarvestRequestManager> logger, int maxrequestspermin = 30)
        {
            _logger = logger;
            MaxRequestsPerMin = maxrequestspermin;

            RunningTimer = new Stopwatch();
            RunningTimer.Start();
        }
        #endregion

        #region Request Execution
        public async Task<HtmlDocument> MakeRequest(string url, HarvestRequestContext ctx, bool isretry = false)
        {
            var requestTimer = new Stopwatch();
            requestTimer.Start();
            HtmlDocument doc = null;
            try
            {
                _logger.LogDebug(string.Format("Making web request: {0}", url));
                HttpWebRequest req = InitialiseRequestFromContext(ctx, url);
                InitialiseCertificates();
                using (WebResponse resp = await req.GetResponseAsync())
                {
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        doc = new HtmlDocument();
                        doc.LoadHtml((await sr.ReadToEndAsync()).Trim());
                        if (doc.DocumentNode.InnerText.Contains("Request unsuccessful"))
                        {
                            var realisedcookie = "";
                            if (!isretry)
                            {
                                _logger.LogDebug("Request did not fail but reurned an Incapsula request was unsuccessful - executing hard throttle of 2s!");
                                _logger.LogDebug($"Updating Incapsula cookies from response headers, new cookie is '{realisedcookie}'");
                                await CookieInitialiser.RefreshCookieForResponseContext(resp, ctx);
                                await Task.Delay(2000);
                                return await MakeRequest(url, ctx, true);
                            }
                            else
                            {
                                _logger.LogDebug("Incapsula request has been rejected even after a retry - executing hard throttle of 5s!");
                                await Task.Delay(5000);
                                throw new Exception($"Incapsula request has been rejected even after a retry for request url '{url}'!");
                            }
                        }
                        _logger.LogDebug(string.Format("Web request response successfully recieved & serialised to cache: {0}bytes", doc.DocumentNode.OuterLength));
                    }
                }
                LastRequestFailed = false;
            }
            catch(Exception ex)
            {
                LastRequestFailed = true;
                _logger.LogDebug(string.Format("Web request failed as follows: {0}", ex.Message));
                _logger.LogDebug(string.Format("Web request was CANCELLED & failed to complete: {0}", url));
                return null;
            }
            await ApplyRequestThrottle(requestTimer);
            return doc;
        }
        #endregion

        #region Helpers
        private HttpWebRequest InitialiseRequestFromContext(HarvestRequestContext ctx, string url)
        {
            HttpWebRequest req = WebRequest.CreateHttp(url);
            if (ctx != null)
            {
                req.Timeout = ctx.Timeout;
                req.Method = ctx.Method;
                req.Host = ctx.Host;
                req.Accept = ctx.Accept;
                req.UserAgent = SpoofUserAgent();
                req.Referer = ctx.Referer;
                req.Credentials = CredentialCache.DefaultCredentials;
                req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                foreach (var h in ctx.Headers) req.Headers.Add(h.Key, h.Value);
                foreach (var c in ctx.Cookies) req.Headers.Add(c.Key, c.Value);
            }
            return req;
        }

        private void InitialiseCertificates()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
        }

        private async Task ApplyRequestThrottle(Stopwatch requesttimer)
        {
            _requestTimes.Add(requesttimer.Elapsed);
            _logger.LogDebug($"Average Request Execution Time  (milliseconds): {AvgRequestLength}");
            _logger.LogDebug($"Current Request Execution Time  (milliseconds): {requesttimer.Elapsed.TotalMilliseconds}");
            _logger.LogDebug($"Executing request throttle for (milliseconds): {CurrentThrottlePause.TotalMilliseconds}");
            await Task.Delay(CurrentThrottlePause);
            requesttimer.Stop();
        }

        private bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, 
                                                           System.Security.Cryptography.X509Certificates.X509Chain chain, 
                                                           System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private string SpoofUserAgent()
        {
            List<string> agents = new List<string>();
            agents.Add("Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
            agents.Add("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2227.1 Safari/537.36");
            agents.Add("Mozilla/5.0 (Windows NT 4.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2049.0 Safari/537.36");
            agents.Add("Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 7.0; InfoPath.3; .NET CLR 3.1.40767; Trident/6.0; en-IN)");
            agents.Add("Mozilla/5.0 (Windows; U; MSIE 9.0; WIndows NT 9.0; en-US))");
            agents.Add("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246");
            agents.Add("Mozilla/5.0 (Windows NT 6.3; rv:36.0) Gecko/20100101 Firefox/36.0");
            agents.Add("Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.1");
            agents.Add("Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:15.0) Gecko/20100101 Firefox/15.0.1");


            agents.Add("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_2) AppleWebKit/601.3.9 (KHTML, like Gecko) Version/9.0.2 Safari/601.3.9");
            agents.Add("Mozilla/5.0 (Linux; Android 4.4.3; KFTHWI Build/KTU84M) AppleWebKit/537.36 (KHTML, like Gecko) Silk/47.1.79 like Chrome/47.0.2526.80 Safari/537.36");
            agents.Add("Mozilla/5.0 (Linux; Android 5.0.2; SAMSUNG SM-T550 Build/LRX22G) AppleWebKit/537.36 (KHTML, like Gecko) SamsungBrowser/3.3 Chrome/38.0.2125.102 Safari/537.36");
            agents.Add("Mozilla/5.0 (Linux; Android 7.0; Pixel C Build/NRD90M; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/52.0.2743.98 Safari/537.36");
            agents.Add("Mozilla/5.0 (Windows Phone 10.0; Android 6.0.1; Microsoft; RM-1152) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Mobile Safari/537.36 Edge/15.15254");
            agents.Add("Mozilla/5.0 (iPhone; CPU iPhone OS 11_0 like Mac OS X) AppleWebKit/604.1.38 (KHTML, like Gecko) Version/11.0 Mobile/15A372 Safari/604.1");

            lock (random) // synchronize
            {
                var rnd = random.Next(0, agents.Count - 1);
                return agents[rnd]; // hacked random
            }
        }
        #endregion
    }
}
