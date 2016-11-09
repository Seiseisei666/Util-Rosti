using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using System.Reflection;

namespace Utility_Promus
{
    class TabellaDB
    {
        static int count;
        public string XUTEN { get { return "Ema"; } }
        public string XDTAGG { get { return string.Format("{0}-{1}-{2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);} }

        int id;
        
        public void CsvExport()
        {
            CsvWriter writer = new CsvWriter(new System.IO.StreamWriter(""));
            writer.Configuration.Delimiter = "#";


                }
    }
}
