using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace Utility_Promus
{
	public class Pattern
	{

        Regex regex;
		Pattern[] children;
        TipoData tipo;
		Match match;


        static bool running;
        public static Data Data;
        public static Match Result;

		private Pattern (string pattern, TipoData tipo= TipoData.NA, bool caseSens = true, params Pattern[] children)
		{
			regex = new Regex (pattern, RegexOptions.Compiled| (caseSens? 0:RegexOptions.IgnoreCase));
			this.children = children;
            this.tipo = tipo;
            if (tipo != TipoData.NA)
                foreach (var p in children)
                    p.setTipo(tipo);
		}

        void setTipo(TipoData tipo)
        {
            this.tipo = tipo;
        }


		static Pattern [] _patterns ()
        {

            var data_txt_fine = new Pattern(@".*?(?<gg>\d\d?)°?[\s\.]{1,2}(?<xx>[IVX]{1,4})[\s\.]{1,2}(?<aaaa>\d\d[\d\.?]{2})");
            var gg_mese_anno_txt_fine = new Pattern(@".*?(?<gg>\d\d ?)°?[\s\.]{1,2}(?<mese>(?i:[gfmalsond]\w{2,6}(?:(?:[rl]e)|(?:[iznt]o))))\b[\s\.]{1,2}(?<aaaa>\d\d[\d\.?]{2})");
            var mese_anno_txt_fine = new Pattern(@".*?(?<mese>(?i:[gfmalsond]\w{2,6}(?:(?:[rl]e)|(?:[iznt]o))))\b[\s\.]{1,2}(?<aaaa>\d\d[\d\.?]{2})");
            var chldrn = new Pattern[] { data_txt_fine, gg_mese_anno_txt_fine, mese_anno_txt_fine };


            var prima_di = new Pattern(@"\b?(?<pos>prima\sd(?:i|el)\s|entro\s(?:il\s\s|la\s|l'))", TipoData.prima_di,false, chldrn);
            var fino_al = new Pattern(@"\b?(?<pos>[sf]ino\sal?|entro\s(?:il\s|la\s|l'))", TipoData.sino_a, false, chldrn);
            var a_partire_da = new Pattern(@"\b?(?<pos>a\spartire\sda(?:l\s|\s|ll')|dopo\s(?:il\s|la\s|l'))", TipoData.a_partire_da,false, chldrn);
            var dopo_il = new Pattern(@"\b?(?<pos>dopo\s(?:il|la\s|l'))", TipoData.dopo_il,false, chldrn);
            var il = new Pattern(@"\b?(?<pos>il|nel)\s", TipoData.il, false, chldrn);
            var intorno_a = new Pattern(@"\b?(?<pos>intorno\sa(?:l?\s|ll')|[n\b]circa\b)", TipoData.intorno_a,false, chldrn);
            var tra = new Pattern(@"(?:[tf]ra\s(?<pos>il\s|l').+?)(?=e\s(?<pos2>il\s|l'))", TipoData.tra, false, chldrn);
            //                      
            var ingresso = new Pattern(@"\W?(?<pos>a\.\s?)", TipoData.il, true, chldrn);
            var uscita = new Pattern(@"\W?(?<pos>d\.\s?)", TipoData.il,true, chldrn);

            Pattern[] result = { ingresso, uscita, prima_di, fino_al, a_partire_da, dopo_il, il, intorno_a, tra };
            return result;
        }

        static string getCodTab (string s)
        { string helper = "";
            if (Program.Tabelle_Helper.TryGetValue(new Tuple<string, string>("attiv", s), out helper))
                return helper;
            else return "presenza";
        }

 
		void _tryMatch (string stringa, Pattern riferimento = null, int startPos = 0)
		{

            Pattern padre = 
				riferimento == null 
				? this 
				: riferimento;

			match = this.regex.Match (stringa, startPos);

			if (match.Success)
            {
				if (children.Any () && running)
                {
						int offset = //match.Groups ["pos"].Index + 
                        match.Groups ["pos"].Length;
						//match = null;
						foreach (Pattern p in children)
							p._tryMatch (stringa, padre, startPos = offset);					
                    
				} else if (!children.Any () && running) {
                    //MatchFound.Invoke (this, new MatchFoundEvntArgs (match, this.tipo));
					running = false;
                    Pattern.Data = null;
                    string g, m, a;
                    g = match.Groups["gg"].Value;
                    m = match.Groups["xx"].Success
                        ? match.Groups["xx"].Value
                        : match.Groups["mese"].Value;
                    a = match.Groups["aaaa"].Value;
                    Data.TryParse(g, m, a, out Data);
                    return;
                }
            }
            }


		public static bool TryMatch (string stringa)
		{
			running = true;
			foreach (Pattern p in _patterns()) 
			{
                if (running) p._tryMatch(stringa);
                else break;
			}
            return (!running);
		}

        public static event EventHandler<MatchFoundEvntArgs> MatchFound;
	}


}

