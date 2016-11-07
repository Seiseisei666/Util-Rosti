using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Utility_Promus
{
	public class Data
    {
		int g,m,a;

		public int G { get { return g; } }
		public int M { get { return m; } }
		public int A { get { return a; } }


		public Data ()
		{
		}

		public Data(int g, int m, int a)
		{
			this.g = g;
			this.m = m;
			this.a = a;
		}

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", g, Enum.GetNames(typeof (Mesi))[m], a);
		}

        public string GG_MM_AAAA { get
            {
                return string.Format("{0}/{1}/{2}", g, m, a);
            } }
		public string Formato()
		{
			string formato = a+"/";
			formato += ((m > 0 ? m.ToString() : "xx") + "/" + (g > 0 ? g.ToString() : "xx"));
			return formato;
		}


		/// <summary>
		/// Cerca di leggere una data
		/// </summary>
		/// <returns><c>true</c>, if parse was tryed, <c>false</c> otherwise.</returns>
		/// <param name="txt">stringa che contiene la data</param>
		/// <param name="data">Data restituita</param>
		public static bool TryParse (string txt, out Data data)
		{
			Match match;
			int g, a, m = 0;

			foreach (Regex formato in Formati) {

				match = formato.Match (txt);
				if (match.Success)
					break;
			}

			if (!match.Success)
				return false;

			a = match.Groups ["aaaa"].Success
				? int.Parse (match.Groups ["aaaa"].Value)
				: 0;
			g = match.Groups ["gg"].Success 
				? int.Parse (match.Groups ["gg"].Value) 
				: 0;

			if (match.Groups ["xx"].Success)
				m = match.Groups ["xx"].Value.ToArabic ();
			else if (match.Groups["mese"].Success)
			{
				foreach (Mesi mese in Enum.GetValues(typeof(Mesi)))
                if (match.Groups["m"].Value == mese.ToString())
					{ m = (int)mese;  break; }
            }

			data = new Data (g, m, a);
		}
           

		public static readonly Regex [] Formati = 
		{
			new Regex (GG + _P + XX + _P + AAAA, RegexOptions.Compiled), // GG.XX.AAAA
			new Regex (GG + _S + MESE + _S + AAAA , RegexOptions.Compiled), // GG Mese AAAA
			new Regex (_S + MESE + @"\s(?:del(?:\s|l'))?" + AAAA , RegexOptions.Compiled), // Mese [del] AAAA
		};


		public static readonly string _S = @"\s";
		public static readonly string _P = @"\.";
		public static readonly string GG = @"(?<gg>\d{1,2})°?";
		public static readonly string MESE = @"(?i:(?<mese>[gfmalsond]\w+(?:(?:[rl]e)|(?:[iznt]o))))\b";
		public static readonly string AAAA = @"(?<aaaa>\d{4})";
		public static readonly string XX = @"(?<xx>[IVX]{1,4})";
	
    }

	public enum formatoData
	{
		GG_mese_AAAA,
		GGpXXpAAAA,
		ORDpXXpAAAA,
	}

	
}
