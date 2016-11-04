using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Utility_Rostirolla
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

		public override string ToString ()
		{
			return string.Format ("[Data: G={0}, M={1}, A={2}]", G, M, A);
		}

		/// <summary>
		/// Cerca di leggere una data
		/// </summary>
		/// <returns><c>true</c>, if parse was tryed, <c>false</c> otherwise.</returns>
		/// <param name="txt">stringa che contiene la data</param>
		/// <param name="data">Data restituita</param>
		public bool TryParse (string txt, out Data data)
		{
			Match match;
			int g, m, a;
			foreach (string f in formati) {
			
				match = Regex.Match (txt, f);
				if (match.Success) {

					g = int.Parse (match.Groups [1].Value);
					m = 0;
					a = int.Parse (match.Groups [2].Value);

					data = new Data (g, m, a);
					return true;
				}

			}
			data = null;
			return false;

		}

		static readonly string[] formati = {
			GG + " " + mese + " " + AAAA,
			GG + "." + XX + "." + AAAA,
			ORD + "." + XX + "." + AAAA
		};

		static readonly string GG = @"(\d{1,2})";
		static readonly string mese = @"";
		static readonly string AAAA = @"(\d{4})";
		static readonly string XX = @"([IVX]{1,4})";
		static readonly string ORD = @"(\d)°";
	
    }

	public enum formatoData
	{
		GG_mese_AAAA,
		GGpXXpAAAA,
		ORDpXXpAAAA,
	}

	
}
