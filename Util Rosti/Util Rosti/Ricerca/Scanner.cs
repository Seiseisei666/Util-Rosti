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
    class Scanner: IRetriever
    {

		public bool Success {get; private set;}

        public event EventHandler InfoRetrieved;

        /// <summary>
        /// Dizionario che mantiene in memoria tutti i gruppi nominativi di ogni singola ricerca
        /// </summary>
        Dictionary<string, string> retrieved = new Dictionary<string, string>();

        /// <summary>
        /// Cache del dizionario
        /// </summary>
        Dictionary<string, string> retrieved_cache;

        Dictionary<string, string> retrieved_temp;

        string _frase;

        bool _isRunning = false;

        bool continuazione_info = false;

        /// <summary>
        /// Puntatore all'ultimo match e all'ultimo regex usati
        /// </summary>
        Match _ma;
        Regex _re;

        /// <summary>
        /// Tutti i pattern più generali, che non sono figli di nessun altro
        /// </summary>
        Regex[] filtri0;

        List<Regex> traccia, traccia_temp;

        /// <summary>
        /// Associa ad ogni pattern i suoi sottopattern, se ne ha
        /// </summary>
        Dictionary<Regex, Tuple<List<Regex>,List<Tuple<Action<string>,string>>>> relazioni = 
            new Dictionary
            <Regex, 
                Tuple
                    <List<Regex>
                    ,List<Tuple
                        <Action<string>, string>>>>();

		public Scanner(string tipoFiltro)
        {
            CsvReader reader = new CsvReader(new System.IO.StringReader(Properties.Resources.filtri));
            reader.Configuration.Delimiter = ";";

            var valid_entries = new Dictionary
                <int,                       // KEY
                Tuple                       //Value
                <string,                        //ITEM1 non in uso
                string,                         //ITEM2 REGEX pattern
                int[],                          //ITEM3 Catena dei regex figli
                List<Tuple<Action<string>,       //ITEM4 Azioni da eseguire in caso di match
                string>>>>();                //Parametro dell'Action
                

                while (reader.Read())
                {
                    

                    string tipo = reader.GetField("TIPO");
					if (tipo != tipoFiltro) continue;

                    string pattern = reader.GetField("REGEX");
                    string figli = reader.GetField("CHILDREN");
                string script_azioni = reader.GetField("SCRIPT");
                    int id = reader.GetField<int>("ID");

					//Trasformo il formato N,N,... del campo children in un array di int
					var idsFigli = 
						string.IsNullOrEmpty(figli)
						?	new int[0]
						:   figli.Split(',')
                        	.Select(s => int.Parse(s))
                        	.ToArray<int>();

                //Parsing dello script
                #region SCRIPT

                //script_azioni = script_azioni.RemoveSpaces();            //Tolgo gli spazi bianchi
                MatchCollection cmds = Regex.Matches(script_azioni, @"(?<cmd>[A-Z_]+)(?:\((?<params>[\w\,_]+)\))?\,?");
                var comandi = new List<Tuple<Action<string>,string>>(cmds.Count);
                foreach (Match m in cmds)
                {
                    //TODO: check sintassi per evitare errori runtime
                    string metodo = m.Groups["cmd"].Value;
                    string argomenti = m.Groups["params"].Value;
                    var met = this.GetType().GetMethod(metodo);
                    var action = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), this, met);
                    comandi.Add(new Tuple<Action<string>, string>(action, argomenti));
                }


                #endregion


                //Memorizzo in locale i campi validi
                valid_entries.Add(id,
                    new Tuple
                    <string,
                    string,
                    int[],
                    List<Tuple<Action<string>, string>>>("", pattern, idsFigli, comandi));
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

			//Estraggo tutti i pattern che hanno almeno un sottopattern o un comando di script
			var padri = valid_entries.Where (
				            entry => entry.Value.Item3.Any ()
                            ||       entry.Value.Item4.Any ());

            foreach (var p in padri)
            {
                var figli = new List<Regex>(p.Value.Item3.Count());
                foreach (int i in p.Value.Item3)
                    figli.Add(regex_generati[i]);
                relazioni.Add(regex_generati[p.Key],                            //Key
                    new Tuple<List<Regex>, List<Tuple<Action<string>, string>>> //Values
                    (figli,             //Value 1
                   p.Value.Item4));     //Value 2
            }

			//Estraggo i filtri0 (regex generati dai pattern generali
			filtri0 = pattern_generali.Select(
				p => regex_generati [p.Key])
				.ToArray();

            _reset();
			this.retrieved_cache = retrieved;
			this.retrieved = new Dictionary<string, string>();
			retrieved["testo"] = "";
        }
			

        /// <summary>
        /// Comincia la scansione del testo
        /// </summary>
        public void Scan (string frase)
        {
            initialize(frase);

            foreach (var re in filtri0)
            {
				if (_isRunning)
                {
                    traccia_temp.Clear();
                    tryMatch(re);
                }

				else break;
            }

            if (continuazione_info)
                retrieved_temp["testo"] += _frase;
        }
        
        /// <summary>
        /// Metodo interno che si chiama ricorsivamente
        /// </summary>
        /// <param name="re"></param>
        void tryMatch (Regex re, int offset = 0)
        {
			//Eseguo il match
            Match match = re.Match(_frase, offset);

			//Locali
			_ma = match;
			_re = re;
			Tuple<List<Regex>,List<Tuple<Action<string>,string>>> relazioni_re;

            if (match.Success)
            {

                relazioni.TryGetValue(re, out relazioni_re);
                bool fine_catena = relazioni_re == null || !relazioni_re.Item1.Any();

                if (fine_catena && continuazione_info) Flush();

                saveInfos();

                if (fine_catena)
                {
                    copyInfos();
                    matchFound();
                }

                continuazione_info = true;

                if (relazioni_re != null)
                {
                    int _offset = match.Groups["p"].Index;
                    //Chiamate ricorsive
                    foreach (var filtro in relazioni_re.Item1)
                    {
                        if (_isRunning) tryMatch(filtro, offset);
                        else break;
					}
                }
                if (!Success) traccia_temp.RemoveLast();
                
            }
        }
        #region Funzioni Helper

        /// <summary>
        /// Chiamato dal costruttore. Inizializza l'oggetto
        /// </summary>
        void _reset()
        {
            this._isRunning = false;
            this.Success = false;
            continuazione_info = false;
            traccia_temp = new List<Regex>();
            retrieved_temp = new Dictionary<string, string>();
        }

        /// <summary>
        /// Chiamato all'inizio di ogni singola scansione
        /// </summary>
        /// <param name="frase"></param>
        void initialize(string frase)
        {
            this._isRunning = true;
            this.Success = false;
            this._frase = frase;
        }
        /// <summary>
        /// Legge eventuali info raccolte e resetta l'istanza
        /// </summary>
        public void Flush()
        {

            if (this.Success)
                onInfoRetrieved();
            this.retrieved_cache = retrieved;
            this.traccia = new List<Regex>();
            this.retrieved_temp = new Dictionary<string, string>();
            retrieved_temp["testo"] = "";
            this._isRunning = false;
            this.continuazione_info = false;
        }

        void saveInfos()
        {
            //Copio temporaneamente le info
            foreach (string nome in _re.GetNamedGroupsNames())
                retrieved_temp[nome] = _ma.Groups[nome].Value;

            traccia_temp.Add(_re);
        }

        void copyInfos()
        {
            retrieved = retrieved_temp;
            traccia = traccia_temp;
            
        }

        void matchFound()
        {
            this._isRunning = false;
            this.Success = true;
        }

        void executeScript()
        {
            Tuple<List<Regex>, List<Tuple<Action<string>, string>>> relazioni_re;

            foreach (var re in traccia)
            {
                if (relazioni.TryGetValue(re, out relazioni_re))
                    foreach (var cmd in relazioni_re.Item2)
                        cmd.Item1.Invoke(cmd.Item2);
            }
            traccia.Clear();
        }


        void onInfoRetrieved ()
        {
            executeScript();
            if (InfoRetrieved != null) InfoRetrieved.Invoke(this, null);
        }
        
        #endregion

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

        public string[] getInfos(params string[] groups)
        {
            string[] res = new string[groups.Count()];
            int c = 0;
            foreach (var gr in groups)
                res[c++] =
                    getInfo(gr);
            return res;
        }

        #region ACTIONS


        /// <summary>
        /// Incrementa di uno il parametro (se numerico)
        /// Nel caso si parli di giorno, mese o anno successivo
        /// </summary>
        /// <param name="param"></param>
        public void INCREM (string param)
        {
            int i;
            try
            {
                string valore = retrieved[param];

                if (int.TryParse(valore, out i))
                    i += 1;
                else
                {
                    i = Data.MeseToInt(valore);
                    if (i > 0) i += 1;
                }

                retrieved[param] = i.ToString();
            }
            catch (KeyNotFoundException)
            {
                return;
            }
        }
        
        /// <summary>
        /// Trova il prossimo match
        /// </summary>
        public void NEXT (string param)
        {
            _ma = _ma.NextMatch();
            if (_ma.Success)
            {
                foreach (var nome in _re.GetNamedGroupsNames())
                {
                    string key = nome + "-inizio";
                    retrieved[key] = retrieved[nome];
                    key = nome + "-fine";
                    retrieved[key] = _ma.Groups[nome].Value;
                }
            }
        }

        public void SET_VAL (string param)
        {
            string[] split = param.Split(',');
            if (split.Count() < 2) return;
            string gr, val;
            gr = split[0];
            val = split[1];
            retrieved[gr] = val;
        }

        public void LAST_VAL (string param)
        {
            retrieved[param] = retrieved_cache[param];
        }

        #endregion
    }

    public interface IRetriever
    {
        string getInfo(string par);
        string[] getInfos(params string [] gr);
    }
}
