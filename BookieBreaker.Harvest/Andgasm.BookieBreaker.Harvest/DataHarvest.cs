using Andgasm.BookieBreaker.RequestManager.Core;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Andgasm.BookieBreaker.Harvest
{
    public abstract class DataHarvest : IDataHarvest
    {
        #region Fields
        protected Stopwatch _timer = new Stopwatch();
        #endregion

        #region Properties
        public IRequestManager _requestmanager { get; set; }
        //public SystemConfig _config { get; set; }
        public ParallelOptions _po { get; set; }
        public TimeSpan ElapsedTime
        {
            get
            {
                return _timer.Elapsed;
            }
        }
        public string CookieString { get; set; }
        public string LastModeKey { get; set; }
        #endregion

        public DataHarvest(int? maxthreads = null)
        {
            //CookieString = "_gat_subdomainTracker=1; _gat=1; _gid=GA1.2.1241310394.1532547615; _ga=GA1.2.638389433.1532114109; visid_incap_774904=2VHWR4OrQJ6JcpSUOUIzzJ40UlsAAAAAVUIPAAAAAACAaMiFAU1lv1DEH1MajB012/jYOqYUG7V9; incap_ses_151_774904=hVSpROahlXZQlHG+J3cYAjEoW1sAAAAAXSx3Py7vJ+wXtAkT3UU/yQ==";
            _po = new ParallelOptions();
            _po.MaxDegreeOfParallelism = maxthreads.HasValue ? maxthreads.Value : Environment.ProcessorCount;
        }

        public virtual bool CanExecute()
        {
            if (_requestmanager == null) return false;
            if (_po == null) return false;
            //if (_config == null) return false;
            return true;
        }

        public abstract Task Execute();

        public string GetLastModeKey(string rootdoc)
        {
            try
            {
                var rawdata = rootdoc;
                int startindex = rawdata.IndexOf("'Model-last-Mode': '") + 20;
                int endindex = rawdata.IndexOf("' }", startindex);
                var lastmodekey = rawdata.Substring(startindex, endindex - startindex);
                return lastmodekey;
            }
            catch(Exception ex)
            {
                return null;
            }
        }
    }
}
