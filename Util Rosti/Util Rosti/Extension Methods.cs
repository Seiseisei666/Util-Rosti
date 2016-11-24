using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility_Promus
{
    public static class Extension_Methods

    { 
        /// <summary>
        /// Restituisce la stringa con le iniziali maiuscole (Es. In Questo Modo)
        /// </summary>
        public static string ToCapitalCase (this string str)
        {
            if (str.Length < 2) return str;

            string helper = "";
            string[] allWrds = str.Split(' ');

            foreach (string s in allWrds)
                if (s.Length > 1)
                    helper = helper + (s.Substring(0, 1).ToUpper() + s.Substring(1).ToLower() + " ");
                else helper = helper + s.ToUpper() + " ";

			return helper.Trim();
        }

        /// <summary>
        /// Rimuove tutte le ricorrenze di un dato char nella stringa
        /// </summary>
        /// <param name="str"></param>
        /// <param name="c">predefinito: spazio</param>
        public static string RemoveSpaces(this string str)
        {
            return str.Where (c => !char.IsWhiteSpace(c)).ToString();
        }

        public static string RemoveNewLineChars (this string str)
        {
            return str.TrimEnd(new char[] { '\n', '\r' });
        }

        /// <summary>
        /// Converte i numeri romani in cifre arabe
        /// </summary>
        /// <returns>restituisce 0 se l'input non è valido</returns>
        public static int ToArabic (this string str)
        {
            if (str.Equals(string.Empty)) return 0;

            int n0 = 0, n1=0, tot = 0;

            for (int i = 0; i < str.Length; i++)
            {
                switch (str.ToUpper().ElementAt(i))
                {
                    case 'I':
                        n1 = 1;
                        break;
                    case 'V':
                        n1 = 5;
                        break;
                    case 'X':
                        n1 = 10;
                        break;
                    case 'L':
                        n1 = 50;
                        break;
                    case 'C':
                        n1 = 100;
                        break;
                    case 'D':
                        n1 = 200;
                        break;
                    case 'M':
                        n1 = 1000;
                        break;
                    default:
                        return 0;
                }

                if (n0 >= n1) tot += n1;
                else tot = tot - n0 * 2 + n1;
                n0 = n1;
            }

            return tot;

            }
        
        /// <summary>
        /// Restituisce tutti i gruppi nominativi di una regex (cioè quelli che non sono numerici)
        /// </summary>
		public static List<string> GetNamedGroupsNames (this System.Text.RegularExpressions.Regex re)
		{
			
			string[] names = re.GetGroupNames ();
			if (!names.Any ())
				return null;
			
			List<string> res = new List<string> (names.Length);

			foreach (string name in re.GetGroupNames())
			{
				int i;
				if (!int.TryParse(name, out i)) res.Add (name);
			}

			return res;
		}

        /// <summary>
        /// Restituisce la posizione finale di un dato match
        /// </summary>
        public static int EndPosition (this System.Text.RegularExpressions.Match match)
        {
            return match.Length + match.Index - 1;
        }

		/// <summary>
		/// associa ai valori T/F di un bool una stringa arbitraria
		/// </summary>
		public static string Chars (this bool b, string t = "s", string f = "n")
		{
			return b
					? t
					: f;
		}

        public static List<T> RemoveLast<T> (this List<T> list )
        {
            return list.Take(list.Count - 1).ToList();
        }


        }

    }

