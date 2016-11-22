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
        public string XUTEN { get { return "Ema"; } }
        public string XDTAGG { get { return string.Format("{0}-{1}-{2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);} }
		public string X0VERN {get {return "0000";}}
		public int Id { get; protected set;}

		public abstract string GetNote();
    }
}
