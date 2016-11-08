using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace Utility_Promus
{
	public class Pattern
	{
		Regex regex;
		List<Pattern> children;


		private Pattern (string pattern, Pattern padre = null)
		{
			regex = new Regex (pattern);
			children = new List<Pattern> ();
			padre?.iscriviFiglio (this);
		}

		void iscriviFiglio(Pattern figlio)
		{
			this.children.Add (figlio);
		}

		/// <summary>
		/// Istanzia i regex Pattern leggendo le righe di un foglio di risorse
		/// </summary>
		static Pattern [] _patterns ()
		{
			Pattern padre = null;
			int ordine = 0;
			string [] righe = Utility_Promus.Resources.Patterns.ReadAllLines();

			foreach (string riga in righe) {
				
				int o = 0;
				while (riga.First().Equals('#'))
					{
						riga = riga.Substring(1);
						o++;
					}

			}


		}

		bool _tryMatch (string stringa, out Match match)
		{
			match = this.regex.Match (stringa);
			if (match.Success)
				foreach (Pattern p in children)
					p._tryMatch (stringa, match);

			return (match.Success);
		}

		public static bool TryMatch (string stringa, out Match match)
		{
			match = null;

			foreach (Pattern p in Patterns) 
			{
				p._tryMatch(stringa, match);
			}

			return (match?.Success);

		}
	}


}

