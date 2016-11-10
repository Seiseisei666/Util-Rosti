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
        int g, m;
        int secolo, decennio, anno;
        string anno_string;

		public int G { get { return g; } }
		public int M { get { return m; } }
		public int A { get { return secolo*100 + (decennio == -1? 0: 10 * decennio) + (anno == -1? 0: anno); } }


		void setData (int g, int m, int s, int d, int a)
        {
            this.g = g;
            this.m = m;
            this.secolo = s;
            this.decennio = d;
            this.anno = a;
        }

		public Data(int g, int m, int a)
		{
            int s, d, _a;
            s = a / 100;
            d = (a - s * 100) / 10;
            _a = a - s + d;
            this.anno_string = _a.ToString();
            setData(g, m, s, d, _a);
		}

        public Data (int g, int m, string anno)
        {
            if (anno.Length != 4) throw new Exception(string.Format ("lunghezza stringa anno errata. Anno.Length: {0}",anno.Length));
            string s, d, a; int S, D, A;
            s = anno.Substring(0, 2); if (!int.TryParse(s, out S)) { throw new ArgumentException (string.Format("Prime due cifre dell'anno non valide. Anno:{0}", anno)); }
            d = anno.Substring(2, 1); if (!int.TryParse(d,out D)) { d = "x"; D = -1; }
            a = anno.Substring(3, 1); if (!int.TryParse(d, out A)) { a = "x"; A = -1; }
            this.anno_string = s + d + a;
            setData(g, m, S, D, A);
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", g, Enum.GetNames(typeof (Mesi))[m], anno_string);
		}

        public string GG_MM_AAAA { get
            {
                return string.Format("{0}/{1}/{2}", g, m, anno_string);
            } }
		public string AAAA_MM_GG()
		{
			string formato = anno_string+"/";
			formato += ((m > 0 ? m.ToString() : "xx") + "/" + (g > 0 ? g.ToString() : "xx"));
			return formato;
		}

        public Data Inizio { get
            {
                int giorno = g == 0 ? 1 : g;
                int mese = m == 0 ? 1 : g;
                return new Data(g, m, A);
            } }

        public Data Fine { get
            {
                int mese = m == 0 ? 12 : g;
                int _anno = secolo * 100 + decennio == -1 ? 9 : decennio + anno == -1 ? 0 : anno; 
                int giorno = g == 0 ? DateTime.DaysInMonth(_anno,mese) : g;
                return new Data(g, m, _anno);
            } }

		/// <summary>
		/// Cerca di leggere una data
		/// </summary>
		/// <returns><c>true</c>, if parse was tryed, <c>false</c> otherwise.</returns>
		/// <param name="txt">stringa che contiene la data</param>
		/// <param name="data">Data restituita</param>
		public static bool TryParse (string txt, out Data data)
		{
			Match match = null;
			int g, a, m = 0;
			Regex [] Formati = 
			{
				new Regex (GG + _P_OR_S + XX + _P_OR_S + AAAA, RegexOptions.Compiled), // GG.XX.AAAA
				new Regex (GG + _P_OR_S + MESE + _P_OR_S + AAAA , RegexOptions.Compiled), // GG Mese AAAA
				new Regex (_S + MESE + @"[\s\.](?:del(?:\s|l'anno\s))?" + AAAA , RegexOptions.Compiled), // Mese [del] AAAA
			};

			foreach (Regex formato in Formati) {

				match = formato.Match (txt);
				if (match.Success) {
					Console.WriteLine ("************************"+match.Value);
					break;
				}
			}

			if (!match.Success) {
				data = null;
				return false;
			}

			g = match.Groups ["gg"].Success 
				? int.Parse (match.Groups ["gg"].Value) 
				: 0;

			if (match.Groups ["xx"].Success)
				m = match.Groups ["xx"].Value.ToArabic ();
			else if (match.Groups ["mese"].Success) {
				foreach (Mesi mese in Enum.GetValues(typeof(Mesi)))
					if (match.Groups ["mese"].Value.ToCapitalCase () == mese.ToString ()) {
						m = (int)mese;
						break;
					}
			} else {
				data = null;
				return false;
			}

            if (int.TryParse (match.Groups["aaaa"].Value, out a))
                data = new Data (g, m, a);
            else try
                {
                    data = new Data(g, m, match.Groups["aaaa"].Value);
                }
                catch (ArgumentException)
                {
                    data = null;
                    return false;
                }
			return true;
		}
           

//		public static readonly Regex [] Formati = 
//		{
//			new Regex (GG + _P_OR_S + XX + _P_OR_S + AAAA, RegexOptions.Compiled), // GG.XX.AAAA
//			new Regex (GG + _P_OR_S + MESE + _P_OR_S + AAAA , RegexOptions.Compiled), // GG Mese AAAA
//			new Regex (_S + MESE + @"[\s\.](?:del(?:\s|l'anno\s))?" + AAAA , RegexOptions.Compiled), // Mese [del] AAAA
//		};


		public static readonly string _S = @"\s";
		public static readonly string _P = @"\.";
        public static readonly string _P_OR_S = @"[\s\.]{1,2}";
		public static readonly string GG = @"(?<gg>\d\d?)°?";
		public static readonly string MESE = @"(?i:(?<mese>[gfmalsond]\w{2,6}(?:(?:[rl]e)|(?:[iznt]o))))\b";
		public static readonly string AAAA = @"(?<aaaa>\d\d[\d\.?]{2})\b";
		public static readonly string XX = @"(?<xx>[IVX]{1,4})";
	
    }

	public enum formatoData
	{
		GG_mese_AAAA,
		GGpXXpAAAA,
		ORDpXXpAAAA,
	}

	
}
