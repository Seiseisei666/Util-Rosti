using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Utility_Rostirolla
{
    class Parser
    {
        string _DEBUG;
        string testo;
        int entries, entriesOk = 0;

		string[] luoghiDiOrigine;

        List<string> Nomi, Cognomi, Parentesi, Corpo, Fonti, Biblio;

        // Filtri Paragrafo ---------------------------------------
		//HACK: controllare eccezioni ("COGNOME o VARIANTE", 
		// in un secondo momento anche i NOMI GENERICI tipo "Bernardino [I]
		// e quelli fra parentesi quadre "[Nome]"
        readonly string COGNOME = @"^([A-Z]+)\,?\s";
        readonly string NOME = @"([\w\'\s]+)";
        readonly string PARENTESI = @"\((.*)\).*\n";
        readonly string CORPO = @"([\w\W]+)";
        readonly string FONTI = @"(?:[F][ontiONTI]+\:\s)(.\n)+";
        readonly string BIBLIOGRAFIA = @"(?:[B][ibliografiaIBLIOGRAFIA]+\:\s)(.\n)+";

        readonly string [] FILTRI_PARAGRAFI = {
                @"^\*[A-Z]", //Ricerca di luogo geografico (iniziante con *)
				@"^[^=^\n^\r]+\=", //Ricerca di '='
				@"^[^\[^\n^\r]+\[", //Ricerca di 'Nome ['
				@"^[A-Z][a-z]+[\,\s]{1,2}[A-Z]+", //Ricerca di nome COGNOME oppure nome, COGNOME
                @"^\(" //Ricerca di "(in ordine cronologico)"
            };


        Regex filtroParagrafoCompleto;
        Regex filtroParagrafo;
        Regex fineParagrafo;
        List<Tuple<int, int>> indici;


        public Parser (string testo)
        {
            this.testo = testo;
			fineParagrafo = new Regex(@"\r\n\r\n", RegexOptions.Compiled);
            filtroParagrafoCompleto = new Regex(
                COGNOME + NOME + PARENTESI 
              //  
              + CORPO 
              //+ FONTI + BIBLIOGRAFIA
                , RegexOptions.Compiled);
            filtroParagrafo = new Regex(COGNOME + NOME + PARENTESI + CORPO + BIBLIOGRAFIA, RegexOptions.Compiled);


            indici = new List<Tuple<int, int>>();
            Nomi = new List<string>(entries);
            Cognomi = new List<string>(entries);
            Parentesi = new List<string>(entries);
            Corpo = new List<string>(entries);
            Fonti = new List<string>(entries);
            Biblio = new List<string>(entries);

            defineParagraphs();

         //   paragrafi = inizioParagrafo.Matches(testo);
        }

        public void Start ()
        {
            Console.WriteLine("\nInizio estrapolazione dati...\n");

            //Variabili locali
            string currentPar;
            Match parsedPar;
            int fail = 0;

            //Init
            entriesOk = 0;
    
            foreach (var id in indici)
            {
                currentPar = testo.Substring(id.Item1, id.Item2);
                
                parsedPar = filtroParagrafoCompleto.Match(currentPar);

                if (parsedPar.Success)
                {
                    var g = parsedPar.Groups;
                    Nomi.Add(g[2].Value);
                    Cognomi.Add(g[1].Value);
                    Parentesi.Add(g[3].Value);
                    Corpo.Add(g[4].Value);
                    entriesOk++;
                    //Stampa a console:

                    Console.WriteLine("[{0}]: {1} {2}...", entriesOk, g[2].Value.ToUpper(), g[1].Value.ToUpper());
                }

                else
                {
                    // Provo altri modelli
                    parsedPar = filtroParagrafo.Match(currentPar);
                    if (parsedPar.Success)
                    {
                        var g = parsedPar.Groups;
                        Nomi.Add(g[2].Value);
                        Cognomi.Add(g[1].Value);
                        Parentesi.Add(g[3].Value);
                        Corpo.Add(g[4].Value);
                        entriesOk++;
                        //Stampa a console:

                        Console.WriteLine("[{0}]: {1} {2}...", entriesOk, g[2].Value.ToUpper(), g[1].Value.ToUpper());
                    }
                    else
                    {
                        //Log
                        fail++;
                        int len = currentPar.Length >= 32 ? 32 : currentPar.Length;
                        Console.WriteLine("ERRORE N. {0} - Impossibile leggere:{1}", fail, currentPar.Substring(0, len));
                    }

                   

                }

            }
        }
      

        void defineParagraphs ()
        {
			//Variabili locali
			int progress = 0;
            int inizioPar = 0, lunghezzaPar;
			string paragrafo;
            Match match;

			//Stampo a console - inizio
            Console.WriteLine("***INIZIO DIVISIONE IN PARAGRAFI***");

            //Inizio Ricerca
            match = fineParagrafo.Match(testo);

            while (match.Success)
            {
                lunghezzaPar = match.Index - inizioPar;

				paragrafo = testo.Substring (inizioPar, lunghezzaPar);

				if (isParagraphValid(paragrafo))
					indici.Add(new Tuple<int, int>(inizioPar, lunghezzaPar));
				
                inizioPar = match.Index+4;

				// Progress bar..........
                if (progress < (match.Index*100f/testo.Length))
                {
                    Console.Write('.');
                    progress+= 5;
                }
				//Loop
                match = match.NextMatch();
				entries++;
            }
			//HACK Aggiungo l'ultima entry
			paragrafo = testo.Substring (inizioPar, testo.Length - inizioPar);

			if (isParagraphValid(paragrafo))
				indici.Add(new Tuple<int, int>(inizioPar, testo.Length - inizioPar));

            entriesOk = indici.Count;
			Console.WriteLine("Trovati {0} Paragrafi validi su un totale di {1} paragrafi analizzati.", entriesOk, entries);

        }

       /// <summary>
       /// Mette a confronto il paragrafo input con vari esempi di paragrafi NON VALIDI = che non fanno riferimento a individui ma sono rimandi o altro
       /// </summary>
       /// <returns><c>true</c>se tutti i check falliscono, <c>false</c>in caso contrario.</returns>
       /// <param name="txt">stringa che rappresenta il paragrafo</param>
		bool isParagraphValid (string txt)
		{
			//Variabili locali
			Regex regex;


			//Ricerca
			foreach (string s in FILTRI_PARAGRAFI) {
				regex = new Regex (s);
				if (regex.Match (txt).Success)
					return false;
			}

			return true;
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










        public List<string> Export ()
        {
            List<string> result = new List<string>(Nomi.Count);

            for (int i = 0; i < Nomi.Count; i++)
            {
                string formato = string.Format(
                    @"
INDIVIDUO N.{0}: {1} {2} ({3})
***INFO***
{4}
**********", 
                    i, Nomi[i], Cognomi[i], Parentesi[i], Corpo[i]);
                result.Add(formato);
            }

            return result;
        }

        public int Entries { get { return entries; } }
        public int EntriesOk { get { return entriesOk; } }

       

    }
}
