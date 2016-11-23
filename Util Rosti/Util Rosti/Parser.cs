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

		Ricerca.Scanner scannerDate;

        int entries, entriesOk = 0;

        static int conteggioErrori = 0;

        /// <summary>
        /// Elenco degli individui trovati nel dizionario
        /// </summary>
        List<Individuo> individui;

        List<string> nomiAlt;
        List<Info_Parentela> parenteleDaVerificare;

        Individuo individuo;
        string paragrafo, nome, cognome, floruit;

        string[] luoghiDiOrigine;

        // Filtri Paragrafo ---------------------------------------
        //HACK: controllare eccezioni ("COGNOME o VARIANTE", 
        // in un secondo momento anche i NOMI GENERICI tipo "Bernardino [I]
        // e quelli fra parentesi quadre "[Nome]"
        static readonly string COGNOME = @"^(?<cgn>[A-Z]+)\,?\s";
        static readonly string NOME = @"(?<nom>[\w\'\s]+)";
        static readonly string PARENTESI = @"\((?<contenuto>.*)\)[\s\.\;]*\r?\n";
        static readonly string CORPO = @"(?<corpo>[\w\W]+)";
        static readonly string FONTI = @"(F(onti|ONTI)\:\s)(?<fonti>[\w\W]+)";
        static readonly string BIBLIOGRAFIA = @"(B(ibliografia|IBLIOGRAFIA)\:\s)(?<biblio>[\w\W]+)";

        readonly string [] FILTRI_PARAGRAFI = {
                @"^\*[A-Z]", //Ricerca di luogo geografico (iniziante con *)
				@"^[^=^\n^\r]+\=", //Ricerca di '='
				@"^[^\[^\n^\r]+\[", //Ricerca di 'Nome ['
				@"^[A-Z][a-z]+[\,\s]{1,2}[A-Z]+", //Ricerca di nome COGNOME oppure nome, COGNOME
                @"^\(", //Ricerca di "(in ordine cronologico)"
				@"^[A-Z]$" //Ricerca di inizio lettera
            };


        readonly string[] MATCH_FLORUIT =
        {
           // @"\d{4}-\d{4})", //AAAA-AAAA
            @"\d{4}(ca\.)?-\d{4})", //AAAA{ca.}-AAAA
            @"\d{4}ca\.)", //AAAAca.
			@"\d\d[\d\?]{2}|\d\d[\d\.]{2})", //AAA? o AA??
            @"\d{4})", //AAAA
        };

        readonly string MATCH_NASCITA = @"\bn\.";
        readonly string MATCH_DATA_NASCITA = @"\b[Nn]\..+?(?<data>\d{1,2}°?\..+?\d{4})";
        readonly string MATCH_DATA_MORTE = @"(?<data>\d{1,2}°?\.[IVX]+\.\d{4})†";
		readonly string MATCH_DATA_MORTE_NO_CROCE = @"\b[Nn]\..+?(\.|([eo]\s))\d{4}\s?-\s?ivi(\s-)?(?<data>\d{1,2}°?[\.\s]{1,3}[IVX]{1,4}[\.\s]{1,3}\d{2}[\d\.?]{2})";

        #region ANALISI CORPO DEL TESTO
        readonly string[] VOCI_O_STRUM =
        {
            "s", "a", "t", "b", "basso", "tenore", "contralto", "alto", "soprano", "cantore", "cantor", "organista", "sopran", "cantus", "cantante", "sopranista", "compositore", "maestro"

        };

        readonly string[] DATI_ULTERIORI =
        {
            "puer", "cappellano", "copista", "eunuco",
        };

        readonly string[] TERM_DI_PARENTELA =
        {
            "figlio", "padre", "parente", "fratello", "nipote", "cugino", 
        };

        readonly string[] SUCCESSIONE =
        {
            "anche", "poi"
        };

        readonly string[] STOP_LETTURA =
        {
            "presente", "appartenente",
        };
        #endregion

        readonly Regex regexParagrafo = new Regex("(?<txt>(?:.+?\n)+?)(?:\r?\n)+", RegexOptions.Compiled);
        readonly Regex cognome_e_nome = new Regex(COGNOME + NOME, RegexOptions.Compiled);
        readonly Regex regexParentesi = new Regex(PARENTESI, RegexOptions.Compiled);
        readonly Regex regexFonti = new Regex(@"F(onti|ONTI)\:", RegexOptions.Compiled);
        readonly Regex regexBiblio = new Regex(@"B(ibliografia|IBLIOGRAFIA)\:", RegexOptions.Compiled);
        readonly Regex rxSingolaFrase = new Regex(@"(?<info>.+?)(?:[\n\r;]|(?<!\b[\w]{1,2})\.\s(?![IVX\.\d]{2,10}))\s?(?!(?:[^\(]+\)|[^\[]\]|[^«]+»))", RegexOptions.Compiled); //Ogni singola frase delimitata da  ";" e a capo
        readonly Regex rxSingolaFrase_virgola = new Regex(@"(?<info>.+?)(?:[\n\r;,])\s?(?!(?:[^\(]+\)|[^\[]\]|[^«]+»))", RegexOptions.Compiled); //Ogni singola frase delimitata da "," ";" ". " e a capo


        List<Tuple<int, int>> indici;





        public Parser (string testo)
        {
            this.testo = testo;
			//fineParagrafo = new Regex(@"(\r\n)+", RegexOptions.Compiled);
			regexParagrafo= new Regex( "(?<txt>(?:.+[\r\n])+?)(?:\r?\n)+", RegexOptions.Compiled);
			scannerDate = new Ricerca.Scanner ("Data");
            scannerDate.InfoRetrieved += retrieveInfo;
            individui = new List<Individuo>();
            indici = new List<Tuple<int, int>>();

          //  Pattern.MatchFound += parseInfo;

            defineParagraphs();


        }

        void defineParagraphs()
        {
            //Variabili locali
            int progress = 0;
            string paragrafo;
            Match match;

            //Stampo a console - inizio
            Console.WriteLine("***Inizio divisione in paragrafi...***\n");

            //Inizio Ricerca
            match = regexParagrafo.Match(testo);

            while (match.Success)
            {
				
				paragrafo = match.Groups["txt"].Value;
				System.Diagnostics.Debug.WriteLine (paragrafo);
                if (isParagraphValid(paragrafo))
					indici.Add(new Tuple<int, int>(match.Index, match.Length));


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
            parenteleDaVerificare = new List<Info_Parentela>();

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

				analizzaParentesi ();

                
				analizzaCorpo ();

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
            Console.Write("\nLEGGO: {0} {1}", nome, cognome);
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
            MatchCollection matchNomiAlt =
                Regex.Matches(parentesi, @"«(?<nome>[\w\s\']+)»");
            if (matchNomiAlt.Count > 0) Console.Write("\nDetto anche: ");
            nomiAlt = new List<string>(matchNomiAlt.Count);
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
                   //******************************* individuo.AddAttività("Nascita", TipoAttività.nascita, data);
                    Console.Write("\nNato nel {0}\t", data.ToString());
                }
            }
			match = null;
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

			if (morte && match!=null && match.Success)
                if (Data.TryParse(match.Groups["data"].Value, out data))
                {
                //*************************************    individuo.AddAttività("Morte", TipoAttività.morte, data);
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
            Match match; 						//match multifunzione
            string filtro = CORPO;				//Filtro per delimitare il corpo del testo
            string parola;						//locale per singola parola dell'header
			bool fonti, biblio;					//Sono presenti le sezioni Fonti e Biblio?
			MatchCollection matches_frasi;		//locale per identificare ogni singola frase del corpo testo

            //************** Analizzo fino al primo ritorno a capo: 
            //Info Voce, parentele, altre notizie semplic
            //Ogni singola info è delimitata da , o ;
            int divisione = Regex.Match(paragrafo, @"(?<header>.*?)\r?\n").Length;
            string header = paragrafo.Substring(0, divisione);
            string body = paragrafo.Substring(divisione);
            matches_frasi = rxSingolaFrase_virgola.Matches(header); //Ogni singola frase delimitata da "," ";" ". " e a capo
                                                                                    

			foreach (Match m in matches_frasi)
			{	
				string info = m.Groups["info"].Value; //Tutta la riga di intestazione (dopo nome cogn e parentesi)

                //Analizzo TUTTE le parole della stringa di info
                match = Regex.Match(info, @"(?:\b(?<word>[SBTA][\s\.]|[\w]{3,})\b)"); //Qualsiasi parola singola con + di 3 lettere | S|A|T|B
                
				parola = match.Groups["word"].Value.ToLower();

                if (STOP_LETTURA.Contains(parola)) break;

                else if (VOCI_O_STRUM.Contains(parola))
                {   //Se info è ad es. "S di Santa Maria Maggiore" mi copio TUTTA la stringa
                    individuo.AddVoce_o_Strumento(info);
                    continue;
                }
                else if (SUCCESSIONE.Contains(parola) || DATI_ULTERIORI.Contains(parola))
                {
                    individuo.AddNota(info);
                    continue;
                }

                else if (TERM_DI_PARENTELA.Contains(parola))
                {
                    individuo.AddNota(info);
                    continue;
                }

                else System.Diagnostics.Debug.WriteLine("INFO HEADER NON COMPRENSIBILE: " + info);

            }

            //**** Qua comincia l'analisi del corpo vero e proprio del testo (string body)

            //Controllo presenza di fonti e/o bibliografia
            fonti = regexFonti.Match(paragrafo).Success;
            biblio = regexBiblio.Match(paragrafo).Success;

            if (fonti) filtro += FONTI;
            if (biblio) filtro += BIBLIOGRAFIA;
            match = Regex.Match(body, filtro);
            string corpo = match.Groups ["corpo"].Value; //TODO: vale la pena dividere  fra header body biblio e fonti prima, all'inizio del metodo

            //Setup per l'analisi delle info
            MatchCollection ritorniAcapo = Regex.Matches(corpo, @".+?\r?\n");
            foreach (Match paragrafetto in ritorniAcapo)
            {
                matches_frasi = rxSingolaFrase.Matches(paragrafetto.Value);
                foreach (Match matchFrase in matches_frasi)
{
                    Ricerca.IRetriever retrievedInfo = scannerDate;
					scannerDate.Scan (matchFrase.Value);
                }
				scannerDate.Flush();
            }
        }

        void retrieveInfo (object sender, EventArgs e)
        {
            Ricerca.IRetriever result = (Ricerca.IRetriever)sender;
            Data dataInizio, dataFine;
            string g_i, m_i, a_i, g_f, m_f, a_f;
            TipoData tipoData;
            string tipoAttività;
            string descrizione = result.getInfo("testo");

            tipoData = (TipoData)int.Parse(result.getInfo("tipo_data"));

if (tipoData== TipoData.tra)
            {
                g_i = result.getInfo("gg-inizio");
                m_i = result.getInfo("mese-inizio");
                a_i = result.getInfo("aaaa-inizio");
                g_f = result.getInfo("gg-fine");
                m_f = result.getInfo("mese-fine");
                a_f = result.getInfo("aaaa-fine");

                Data.TryParse(g_i, m_i, a_i, out dataInizio);
                Data.TryParse(g_f, m_f, a_f, out dataFine);
                this.individuo.AddAttività(new Attività(individuo, descrizione, dataInizio, dataFine));
            }

            else
            {
                g_i = result.getInfo("gg");
                m_i = result.getInfo("mese");
                a_i = result.getInfo("aaaa");
                Data.TryParse(g_i, m_i, a_i, out dataInizio);
                individuo.AddAttività(new Attività(individuo, descrizione, tipoData, dataInizio));
            }
            

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










        public List<string> Export()
        {

            List<string> result = new List<string>(individui.Count);

            foreach(Individuo ind in individui)
            {
				result.Add (ind.GetDescrizione());
            }
            
            return result;
        }

        public int Entries { get { return entries; } }
        public int EntriesOk { get { return entriesOk; } }

       

    }
}
