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
    public class HarvestRequestManager // use as singleton to track requests and apply throttles
    {
        #region Properties
        public bool LastRequestFailed { get; set; }
        ILogger<HarvestRequestManager> _logger;

        double _maxRequestsPerMin = 60D;
        TimeSpan _avgRequestLength = new TimeSpan();
        List<TimeSpan> _requestTimes = new List<TimeSpan>();

        public double MaxAvgRequestLength //(seconds)
        {
            get
            {
                return (_maxRequestsPerMin / 60D);
            }
        }

        public double AvgRequestLength //(seconds)
        {
            get
            {
                return _requestTimes.Average(x => x.TotalSeconds);
            }
        }

        public double ForecastRequestsPerMin
        {
            get
            {
                return (60D / AvgRequestLength);
            }
        }

        public int CurrentThrottlePause //(seconds)
        {
            get
            {
                // need to work out how much over the max req
                return (int)(AvgRequestLength - MaxAvgRequestLength);
            }
        }
        #endregion

        public HarvestRequestManager(ILogger<HarvestRequestManager> logger)
        {
            _logger = logger;
        }

        public async Task<HtmlDocument> MakeRequest(string url, HarvestRequestContext ctx, bool isretry = false)
        {
            var requestTimer = new Stopwatch();
            requestTimer.Start();
            HtmlDocument doc = null;
            try
            {
                _logger.LogDebug(string.Format("Making web request: {0}", url));
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
                    foreach(var h in ctx.Headers) req.Headers.Add(h.Key, h.Value);
                    foreach (var c in ctx.Cookies) req.Headers.Add(c.Key, c.Value);
                }

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
                using (WebResponse resp = await req.GetResponseAsync())
                {
                    
                    using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                    {
                        doc = new HtmlDocument();
                        var data = await sr.ReadToEndAsync();
                        doc.LoadHtml(data.Trim());
                        if (doc.DocumentNode.InnerText.Contains("Request unsuccessful"))
                        {
                            throw new Exception("Request did not fail but reurned an Incapsula request was unsuccessful!");
                        }
                        _logger.LogDebug(string.Format("Web request response successfully recieved & serialised to cache: {0}bytes", data.Length));
                    }
                }
                LastRequestFailed = false;
            }
            catch(Exception ex)
            {
                // DBr: we need a significant pause here in case the failure is due to request throttling (403)
                LastRequestFailed = true;
                _logger.LogDebug(string.Format("Web request failed as follows: {0}", ex.Message));
                _logger.LogDebug("Pausing to avoid request throttle!");
                await Task.Delay(20000);
                _logger.LogDebug(string.Format("Web request was cancelled & failed to complete: {0}", url));
                throw ex;
            }
            await Task.Delay(CurrentThrottlePause);
            _requestTimes.Add(requestTimer.Elapsed);
            requestTimer.Stop();
            return doc;
        }

        public bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, 
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
            return agents.Random(); // TODO: random leanup!!
        }
    }

    public static class ListExtensions
    {
        public static bool AddIfNotExists<T>(this IList<T> collection, T item)
        {
            if (!collection.Contains(item))
            {
                collection.Add(item);
                return true;
            }
            return false;
        }

        public static class EnumerableHelper<E>
        {
            private static Random r;

            static EnumerableHelper()
            {
                r = new Random();
            }

            public static T Random<T>(IEnumerable<T> input)
            {
                return input.ElementAt(r.Next(input.Count()));
            }

        }

        public static T Random<T>(this IEnumerable<T> input)
        {
            return EnumerableHelper<T>.Random(input);
        }
    }
}
