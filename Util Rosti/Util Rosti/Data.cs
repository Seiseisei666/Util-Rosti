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
            _a = a - (s*100 + d*10);
            this.anno_string = a.ToString();
            setData(g, m, s, d, _a);
		}

        public Data (int g, int m, string anno)
        {
            try
            {
                if (anno.Length != 4) throw new Exception(string.Format("lunghezza stringa anno errata. Anno.Length: {0}", anno.Length));
                string s, d, a; int S, D, A;
                s = anno.Substring(0, 2); if (!int.TryParse(s, out S)) { throw new ArgumentException(string.Format("Prime due cifre dell'anno non valide. Anno:{0}", anno)); }
                d = anno.Substring(2, 1); if (!int.TryParse(d, out D)) { d = "x"; D = -1; }
                a = anno.Substring(3, 1); if (!int.TryParse(d, out A)) { a = "x"; A = -1; }
                this.anno_string = s + d + a;
                setData(g, m, S, D, A);
            }
            catch (Exception)
            {
                //HACK: fare il parse di un intervallo anni?
                System.Diagnostics.Debug.WriteLine(anno);
                var match = Regex.Match(anno, @"\d\d\d\d");
                this.anno_string = match.Value;
                setData(g, m, int.Parse(anno_string.Substring(0, 2)), int.Parse(anno_string.Substring(2, 1)), int.Parse( anno_string.Substring(3, 1)));
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", g, ((Mesi)m).ToString(), anno_string);
		}

        public string GG_MM_AAAA { get
            {
                return string.Format("{0}/{1}/{2}", g, m, anno_string);
            } }

		/// <summary>
		/// Formato della data in uso nel DB
		/// </summary>
		public string FormatoDB()
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
            string g, a, m = "";
			Regex [] Formati = 
			{
				new Regex (GG + _P_OR_S + XX + _P_OR_S + AAAA, RegexOptions.Compiled), // GG.XX.AAAA
				new Regex (GG + _P_OR_S + MESE + _P_OR_S + AAAA , RegexOptions.Compiled), // GG Mese AAAA
				new Regex (_S + MESE + @"[\s\.](?:del(?:\s|l'anno\s))?" + AAAA , RegexOptions.Compiled), // Mese [del] AAAA
			};

			foreach (Regex formato in Formati) {

				match = formato.Match (txt);
				if (match.Success) {
					break;
				}
			}

			if (!match.Success) {
				data = null;
				return false;
			}
            g = match.Groups["gg"].Value;
            m = match.Groups["xx"].Success 
                ? match.Groups["xx"].Value 
                : match.Groups["mese"].Value;
            a = match.Groups["aaaa"].Value;
            return (Data.TryParse(g, m, a, out data));
		}
           
        public static bool TryParse (string gg, string mese, string anno, out Data data)
        {
            //Giorno
            int g = 0; int m = 0; int a = 0;
            int.TryParse(gg, out g);
            //Mese
            int risultato; int cambioAnno = 0;
            if (int.TryParse(mese, out risultato))
            {
                if (risultato > 12) { risultato = risultato - 12; cambioAnno = 1; }
                else if (risultato <= 0) { risultato = 12 + risultato; cambioAnno = -1; }
            }
            else m = Data.MeseToInt(mese);
            
            if (int.TryParse(anno, out a))
                data = new Data(g, m, a+cambioAnno);
            else try
                {
                    data = new Data(g, m, anno);
                }
                catch (ArgumentException)
                {
                    data = null;
                    return false;
                }
            return true;

        }

			public static int MeseToInt (string mese)
        {
            if (string.IsNullOrEmpty(mese))
                return 0;
            
            else
            {
                if (mese.Length <= 4)
                    return mese.ToArabic();
                else
                {
                    foreach (Mesi nome_mese in Enum.GetValues(typeof(Mesi)))
                        if (mese.ToCapitalCase() == nome_mese.ToString())
                        {
                            return (int)nome_mese;
                        }
                }
            }
            return 0;
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
