using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Utility_Promus
{
    class Parser
    {
        /// <summary>
        /// Il testo completo del dizionario da analizzare
        /// </summary>
        string testo;

        int entries, entriesOk = 0;

        static int conteggioErrori = 0;

        /// <summary>
        /// Elenco degli individui trovati nel dizionario
        /// </summary>
        List<Individuo> individui;

        List<string> nomiAlt;

        Individuo individuo;
        string paragrafo, nome, cognome, floruit;
        Data dataNascita, dataMorte;
        string[] luoghiDiOrigine;

        // Filtri Paragrafo ---------------------------------------
        //HACK: controllare eccezioni ("COGNOME o VARIANTE", 
        // in un secondo momento anche i NOMI GENERICI tipo "Bernardino [I]
        // e quelli fra parentesi quadre "[Nome]"
        static readonly string COGNOME = @"^(?<cgn>[A-Z]+)\,?\s";
        static readonly string NOME = @"(?<nom>[\w\'\s]+)";
        static readonly string PARENTESI = @"\((?<contenuto>.*)\)[\s\.\;]*\r?\n";
        static readonly string CORPO = @"(?<corpo>[.+(\r\n){1}]+)";
        static readonly string FONTI = @"(F(onti|ONTI)\:\s)(?<fonti>[.+(\r\n){1}]+)";
        static readonly string BIBLIOGRAFIA = @"(B(ibliografia|IBLIOGRAFIA)\:\s)(?<biblio>[.+(\r\n){1}]+)";

        readonly string [] FILTRI_PARAGRAFI = {
                @"^\*[A-Z]", //Ricerca di luogo geografico (iniziante con *)
				@"^[^=^\n^\r]+\=", //Ricerca di '='
				@"^[^\[^\n^\r]+\[", //Ricerca di 'Nome ['
				@"^[A-Z][a-z]+[\,\s]{1,2}[A-Z]+", //Ricerca di nome COGNOME oppure nome, COGNOME
                @"^\(" //Ricerca di "(in ordine cronologico)"
            };


        readonly string[] MATCH_FLORUIT =
        {
           // @"\d{4}-\d{4})", //AAAA-AAAA
            @"\d{4}(ca\.)?-\d{4})", //AAAA{ca.}-AAAA
            @"\d{4}ca\.)", //AAAAca.
            @"\d{2,3}\?+)", //AAA? o AA??
            @"\d{4})", //AAAA
        };

        readonly string MATCH_NASCITA = @"\bn\.";
        readonly string MATCH_DATA_NASCITA = @"\b[Nn]\..+?(?<data>\d{1,2}°?\..+?\d{4})";
        readonly string MATCH_DATA_MORTE = @"(?<data>\d{1,2}°?\.[IVX]+\.\d{4})†";
        readonly string MATCH_DATA_MORTE_NO_CROCE = @"\b[Nn]\..+?(\.|([eo]\s))\d{4}\s?-\s?ivi(\s-)?(?<data>\d{1,2}°?\.[IVX]+\.\d{4})";

        readonly Regex fineParagrafo = new Regex(@"\r\n\r\n", RegexOptions.Compiled);
        readonly Regex cognome_e_nome = new Regex(COGNOME + NOME, RegexOptions.Compiled);
        readonly Regex regexParentesi = new Regex(PARENTESI, RegexOptions.Compiled);
        readonly Regex regexFonti = new Regex(@"F(onti|ONTI)\:", RegexOptions.Compiled);
        readonly Regex regexBiblio = new Regex(@"B(ibliografia|IBLIOGRAFIA)\:", RegexOptions.Compiled);


        List<Tuple<int, int>> indici;


        public Parser (string testo)
        {
            this.testo = testo;
			fineParagrafo = new Regex(@"\r\n\r\n", RegexOptions.Compiled);
            individui = new List<Individuo>();
            indici = new List<Tuple<int, int>>();

            defineParagraphs();


        }

        void defineParagraphs()
        {
            //Variabili locali
            int progress = 0;
            int inizioPar = 0, lunghezzaPar;
            string paragrafo;
            Match match;

            //Stampo a console - inizio
            Console.WriteLine("***Inizio divisione in paragrafi...***\n");

            //Inizio Ricerca
            match = fineParagrafo.Match(testo);

            while (match.Success)
            {
                lunghezzaPar = match.Index - inizioPar;

                paragrafo = testo.Substring(inizioPar, lunghezzaPar);

                if (isParagraphValid(paragrafo))
                    indici.Add(new Tuple<int, int>(inizioPar, lunghezzaPar));

                inizioPar = match.Index + 4;

                // Progress bar..........
                if (progress < (match.Index * 100f / testo.Length))
                {
                    Console.Write('*');
                    progress += 5;
                }
                //Loop
                match = match.NextMatch();
                entries++;
            }
            //HACK Aggiungo l'ultima entry
            paragrafo = testo.Substring(inizioPar, testo.Length - inizioPar);

            if (isParagraphValid(paragrafo))
                indici.Add(new Tuple<int, int>(inizioPar, testo.Length - inizioPar));

            entriesOk = indici.Count;
            Console.WriteLine("\nTrovati {0} Paragrafi validi su un totale di {1} paragrafi analizzati.", entriesOk, entries);

        }

        /// <summary>
        /// Mette a confronto il paragrafo input con vari esempi di paragrafi NON VALIDI = che non fanno riferimento a individui ma sono rimandi o altro
        /// </summary>
        /// <returns><c>true</c>se tutti i check falliscono, <c>false</c>in caso contrario.</returns>
        /// <param name="txt">stringa che rappresenta il paragrafo</param>
        bool isParagraphValid(string txt)
        {
            //Variabili locali
            Regex regex;


            //Ricerca
            foreach (string s in FILTRI_PARAGRAFI)
            {
                regex = new Regex(s);
                if (regex.Match(txt).Success)
                    return false;
            }

            return true;
        }


        public void Start ()
        {
            Console.WriteLine(@"
********************************
***INIZIO ESTRAPOLAZIONE DATI***
********************************

");


            //Init
            entriesOk = 0;

            foreach (var id in indici)
            {
                nome = "";
                cognome = "";
                floruit = "";
                //Leggo il paragrafo
                paragrafo = testo.Substring(id.Item1, id.Item2);

                if (!analizzaNome()) continue;
                //Procedo con l'analisi solo se il nome è valido

                individuo = new Individuo(nome, cognome);

                if (!analizzaParentesi())
                    ;

                
               

                //Export
                individui.Add(individuo);

            }
        }

        bool analizzaNome ()
        {
            Match match;
            //cognome_e_nome = new Regex(COGNOME + NOME);
            match = cognome_e_nome.Match(paragrafo);
            // Gestione eventuale errore
            if (!match.Success)
            {
                logErroreEstrazione(paragrafo, "Id. NOME E COGNOME");
                return false;
            }
            
            //Copio i valori, riposiziono *rPos
            cognome = match.Groups["cgn"].Value.ToCapitalCase();
            nome = match.Groups["nom"].Value.ToCapitalCase();
            paragrafo = paragrafo.Substring(match.Index + match.Length);
            entriesOk++;

            // Scrivo a Console
            Console.Write("\nLeggo...\t{0}\t{1}", nome, cognome);
            return true;
        }

        bool analizzaParentesi()
        {
            int idx_fine_parentesi;
            Match match;
            match = regexParentesi.Match(paragrafo);
            if (!match.Success)
            {
                logErroreEstrazione(paragrafo, "Parentesi non presente o illegibile!");
                return false;
            }
            string parentesi = match.Groups["contenuto"].Value;
            idx_fine_parentesi = match.Index + match.Length;

            // 1) Nomi alternativi
            nomiAlt = new List<string>(0);
            MatchCollection matchNomiAlt =
                Regex.Matches(parentesi, @"«(?<nome>[\w\s\']+)»");
            if (matchNomiAlt.Count > 0) Console.Write("\nDetto anche:");
            foreach (Match m in matchNomiAlt)
            {
                nomiAlt.Add(m.Groups["nome"].Value);
                Console.Write("{0}; ", m.Groups["nome"].Value);
            }
            // 2) Floruit
            foreach (var filtro in MATCH_FLORUIT)
            {
                match = Regex.Match(parentesi, @"\bfl\.?\s?(?<floruit>" + filtro);
                if (match.Success)
                {
                    floruit = match.Groups["floruit"].Value;
                    string result = string.Format("Floruit {0}.", floruit);
                    individuo.AddNota(result);
                    Console.WriteLine(result);
                    break;

                };
            }


            // 3) DATE NASCITA - MORTE!
            Data data;

            //Cerco segnalatori di presenza data nascita
            if (Regex.Match(parentesi, @"\b[Nn]\.").Success)
            {
                match = Regex.Match(parentesi, MATCH_DATA_NASCITA);
                if (match.Success && Data.TryParse(match.Groups["data"].Value, out data))
                {
                    individuo.SetData(data, TipoData.Nascita);
                    Console.Write("\nNato nel {0}\t", data.ToString());
                }
            }

            //Cerco segnalatori di presenza data morte
            bool morte = false;
            if ( Regex.Match(parentesi, @"†").Success)
            {
                match = Regex.Match(parentesi, MATCH_DATA_MORTE);
                morte = true;
            }
            else if (Regex.Match(parentesi, @"\b[Nn]\..+?(\.|([eo]\s))\d{4}\s?-\s?\d").Success)
            {
                match = Regex.Match(parentesi, MATCH_DATA_MORTE_NO_CROCE);
                morte = true;
            }

            if (morte && match.Success)
                if (Data.TryParse(match.Groups["data"].Value, out data))
                {
                    individuo.SetData(data, TipoData.Morte);
                    Console.Write("Morto nel {0}", data.ToString());
                }
                else logErroreEstrazione(match.Groups["data"].Value, "Data morte illegibile");


            // 4) Provenienza geografica
            match = Regex.Match(parentesi, @"(?:\b[Dd][ai]\b|[Nn]\.\sa)\s(?<luogo>[A-Z]\w+)");
            if (match.Success)
            {
                individuo.SetProvenienza(match.Groups["luogo"].Value);
                Console.Write("\nProvenienza: {0}", match.Groups["luogo"].Value);
            }

            //TODO: Copio i valori, 
            //riposiziono *rPos

            paragrafo = paragrafo.Substring(idx_fine_parentesi);
            return true;
        }

        void analizzaCorpo()
        {
            //Locali
            Match match;
            bool fonti, biblio;

            fonti = regexFonti.Match(paragrafo).Success;
            biblio = regexBiblio.Match(paragrafo).Success;
        }



        /// <summary>
        /// Log a console e eventuale GESTIONE paragrafi scartati
        /// </summary>
        /// <param name="testo"></param>
        static void logErroreEstrazione(string testo, string codiceErr)
        {
            conteggioErrori++;
            int len = testo.Length >= 32 ? 32 : testo.Length;
            Console.WriteLine("\n!!!\nERRORE N. {0} - Tipo: {1}\nImpossibile interpretare:{2}\n", conteggioErrori, codiceErr,testo.Substring(0, len));
        }

        





		void estraiLuoghiOrigine ()
		{
			//Variabili Locali
			Regex regexLuoghi;
			MatchCollection matches;

			//Init
			regexLuoghi = new Regex(@"\*([A-Z][a-z]+)\s\(");

			//Stampa a Console inizio
			Console.WriteLine("Inizio raccolta informazioni geografiche...");

			//Ricerca...
			matches = regexLuoghi.Matches (testo);
			luoghiDiOrigine = new string[matches.Count];
			foreach (Match m in matches)
				luoghiDiOrigine [m.Index] = m.Groups [1].Value;

			//Stampa a console fine
			Console.WriteLine("Raccolta luoghi geografici completata.\nTrovate {0} località", matches.Count);

			
		}










        //public List<string> Export ()
        //{
            
//            List<string> result = new List<string>(Nomi.Count);

//            for (int i = 0; i < Nomi.Count; i++)
//            {
//                string formato = string.Format(
//                    @"
//INDIVIDUO N.{0}: {1} {2} ({3})
//***INFO***
//{4}
//**********", 
//                    i, Nomi[i], Cognomi[i], Parentesi[i], Corpo[i]);
//                result.Add(formato);
//            }

//            return result;
        //}

        public int Entries { get { return entries; } }
        public int EntriesOk { get { return entriesOk; } }

       

    }
}
