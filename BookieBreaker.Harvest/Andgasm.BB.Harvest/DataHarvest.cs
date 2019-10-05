using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Andgasm.BB.Harvest
{
    public abstract class DataHarvest : IDataHarvest
    {
        #region Fields
        protected Stopwatch _timer = new Stopwatch();
        #endregion

        #region Properties
        public HarvestRequestManager _requestmanager { get; set; }
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
            _po = new ParallelOptions();
            _po.MaxDegreeOfParallelism = maxthreads.HasValue ? maxthreads.Value : Environment.ProcessorCount;
        }

        public virtual bool CanExecute()
        {
            if (_requestmanager == null) return false;
            if (_po == null) return false;
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
