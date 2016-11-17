using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility_Promus
{
    public static class Extension_Methods

    { 
        public static string ToCapitalCase (this string str)
        {
            if (str.Length < 2) return str;

            string helper = "";
            string[] allWrds = str.Split(' ');

            foreach (string s in allWrds)
                if (s.Length > 1)
                    helper = helper + (s.Substring(0, 1).ToUpper() + s.Substring(1).ToLower() + " ");
                else helper = helper + s.ToUpper() + " ";

            return helper;
        }

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


        }

    }

