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
			int g, a, m = -1;
            for (int i = 0; i < Formati.Length; i++)
            {
                match = Regex.Match(txt, Formati[i]);
                if (match.Success)
                {
                    g = int.Parse(match.Groups["g"].Value);
                    a = int.Parse(match.Groups["a"].Value);

                    if (i == 0)
                        m = match.Groups["m"].Value.ToArabic();
                    else
                    {
                        
                        foreach (Mesi mese in Enum.GetValues(typeof(Mesi)))
                            if (match.Groups["m"].Value == mese.ToString())
                            { m = (int)mese;  break; }
                    }
                    if (m == -1) throw new Exception("Data matchata correttamente, ma mese non assegnato");
                    data = new Data(g, m, a);
                    return true;
                }
           

			}
			data = null;
			return false;

		}


		public static readonly string[] Formati = {
			@"(?<g>\d{1,2})°?\.(?<m>[IVX]{1,4})\.(?<a>\d{4})",

			GG + @"\." + XX + @"\." + AAAA,
            GG + @"\s" + mese + @"\s" + AAAA,
        };

		public static readonly string GG = @"(?<g>\d{1,2})°?";
		public static readonly string mese = @"(?<m>(Gennaio)|(Febbraio)|(Marzo)|(Aprile)|(Maggio)|(Giugno)|(Luglio)|(Agosto)|(Settembre)|(Ottobre)|(Novembre)|(Dicembre))";
		public static readonly string AAAA = @"(?<a>\d{4})";
		public static readonly string XX = @"(?<m>[IVX]{1,4})";
	
    }

	public enum formatoData
	{
		GG_mese_AAAA,
		GGpXXpAAAA,
		ORDpXXpAAAA,
	}

	
}
