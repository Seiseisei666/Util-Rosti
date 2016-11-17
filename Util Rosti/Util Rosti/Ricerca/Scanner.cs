using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using System.Threading.Tasks;
using Utility_Promus;

namespace Utility_Promus.Ricerca
{
    class Scanner
    {

        bool isRunning;

		public bool Success {get; private set;}

        Dictionary<string, string> retrieved = new Dictionary<string, string>();

        string testo;

		Match risultato;

		/// <summary>
		/// Tutti i pattern più generali, che non sono figli di nessun altro
		/// </summary>
		Regex[] filtri0;

        /// <summary>
        /// Associa ad ogni pattern i suoi sottopattern, se ne ha
        /// </summary>
        Dictionary<Regex, List<Regex>> relazioni = new Dictionary<Regex, List<Regex>>();

		public Scanner(string tipoFiltro)
        {
            CsvReader reader = new CsvReader(new System.IO.StringReader(Properties.Resources.filtri));
            reader.Configuration.Delimiter = ";";

            var valid_entries = new Dictionary<int,Tuple<string, string, int[]>>();


                while (reader.Read())
                {
                    int id;

                    string tipo = reader.GetField("TIPO");
					if (tipo != tipoFiltro) continue;

                    string pattern = reader.GetField("REGEX");
                    string figli = reader.GetField("CHILDREN");
                    id = reader.GetField<int>("ID");

					//Trasformo il formato N,N,... del campo children in un array di int
					var idsFigli = 
						string.IsNullOrEmpty(figli)
						?	new int[0]
						:   figli.Split(',')
                        	.Select(s => int.Parse(s))
                        	.ToArray<int>();
                    
					//Memorizzo in locale i campi validi
					//Key = ID
					//Item1: descrizione ***NON USATO ANCORA***
					//Item2: pattern del regex
					//Item3: array degli id dei figli
					valid_entries.Add(id, new Tuple<string, string, int[]>("", pattern, idsFigli));
                }

            //Controllo che non ci siano riferimenti circolari

            var loop = valid_entries.Any(
                entry => entry.Value.Item3.Any(
                    figlio => valid_entries[figlio].Item3.Contains(entry.Key)
                    || figlio == entry.Key));

            if (loop) throw new Exception("Errore nei filtri - presente riferimento circolare");

            //Genero uno ad uno tutti i regex letti
            var regex_generati = new Regex[valid_entries.Max(e => e.Key)+1];
            foreach (var e in valid_entries)
                regex_generati[e.Key] = new Regex(e.Value.Item2, RegexOptions.Compiled);

			//Estraggo i pattern generali (= quelli che non sono figli di nessuno)
			var pattern_generali = valid_entries.Where(
                entry => !valid_entries.Values.Any(
                    tuple => tuple.Item3.Contains(entry.Key)));

			if (!pattern_generali.Any ())
				throw new EntryPointNotFoundException ("Errore nel file dei filtri - filtri0 non presenti");

			//Estraggo tutti i pattern che hanno almeno un sottopattern
			var padri = valid_entries.Where (
				            entry => entry.Value.Item3.Any ());

            foreach (var p in padri)
            {
                var figli = new List<Regex>(p.Value.Item3.Count());
                foreach (int i in p.Value.Item3)
                    figli.Add(regex_generati[i]);
                relazioni.Add(regex_generati[p.Key], figli);
            }

			//Estraggo i filtri0 (regex generati dai pattern generali
			filtri0 = pattern_generali.Select(
				p => regex_generati [p.Key])
				.ToArray();
            
        }

        public void Scan (string testo)
        {
            //Reset dello stato dell'oggetto
            this.retrieved.Clear();
			this.testo = testo;
			this.isRunning = true;
			this.Success = false;
			this.risultato = null;

			foreach (var re in filtri0)
            {
				if (isRunning)
					tryMatch (re);
				else
					return;
            }
        }

		/// <summary>
		/// Metodo pubblico per leggere i risultati del match
		/// </summary>
		/// <param name="parametro">stringa identificativa del gruppo letto</param>
		public string getInfo(string gr)
		{
            string res;
            retrieved.TryGetValue(gr, out res);
            return res;
		}

		public string [] getInfos (params string[] groups)
		{
			string[] res = new string[groups.Count()];
			int c = 0;
			foreach (var gr in groups)
				res [c++] =
					getInfo(gr);
			return res;
		}


        void tryMatch (Regex re)
        {
            Match match = re.Match(testo);
			List<Regex> figli;
            if (match.Success)
            {
                //Salvo tutti i gruppi nominativi nel dizionario retrieved
				foreach (string nome in re.GetNamedGroupsNames()) 
					retrieved.Add(nome, match.Groups[nome].Value);

				if (relazioni.TryGetValue (re, out figli)) {
					foreach (var f in figli) {
						if (isRunning) tryMatch (f);
					}
				} 
				else {
					isRunning = false;
					Success = true;
					risultato = match;
					return;
				}
            }

        }

    }
}
