using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper;

namespace Utility_Promus
{
    class Program
    {
        static readonly Dictionary<Tuple<string, string>, string> Tabelle_Helper;

        static readonly string pathDbIndividui = @"Resources\individui (1).csv";
        static readonly string pathDbAttività = @"Resources\attivita.csv";
        string pathDbHelper = "xtabserv.dat";
        static readonly string header = @"
         ___________________________________
        ***********************************)
        ** P R O M U S  -  U T I L I T Y **)
        ** - - - - - - - - - - - - - - - **)
        ** .......Versione 0.1.......... **)
        ***********************************

";



        static void Main(string[] args)
        {
            Init();
            Console.Write(header);
            GetSelection();

        }


        static void Parse(string filename)
        {
            bool b = Regex.IsMatch(filename, @"\w\.txt");
            if (!b)
            {
                Console.WriteLine("ATTENZIONE: File non valido o inesistente");
                GetSelection();
            }


            string file = string.Empty;

            try
            {
                file = File.ReadAllText(filename);
            }
            catch (Exception ex)
            {
                if (ex is FileNotFoundException)
                    Console.WriteLine("File non trovato!");
                GetSelection();
            }

            Console.WriteLine("Completata lettura; caricato dizionario da {0} caratteri.", file.Length);


            Parser p = new Parser(file);
            p.Start();

            Console.WriteLine("Analisi completata!\n\nNumero di voci analizzate: {0}\n\nNumero di voci valide: {1}", p.Entries, p.EntriesOk);

            Console.WriteLine("Scrivo su file? S/N");

            var sel = Console.ReadKey().Key;

            if (sel == ConsoleKey.S)
            {
                Console.WriteLine("Inizio scrittura su file...");


                File.WriteAllLines(@"export.txt", p.Export());

                Console.WriteLine("Scrittura su file completata.\nPremere un tasto per continuare");

            }

            Console.ReadKey();

        }


        static void Init()
        {
            Console.Write(@"
        INIZIALIZZAZIONE IN CORSO...");

            string txt = "";
            if (File.Exists(pathDbIndividui)) txt = "\n - Database individui identificato...";
            else txt += "\n - ATTENZIONE!!! Database individui non presente!";
            if (File.Exists(pathDbAttività)) txt = "\n - Database attività identificato...";
            else txt += "\n - ATTENZIONE!!! Database attività non presente!";
            Console.Write(txt);
            return;

        }

        static void GetSelection()
        {
            string txt = @"

    -a filename.txt : Analizza un dizionario dei cantori e ne estrae le informazioni
    -i filename.csv : Carica un nuovo file di database dei cantori
    -t filename.csv : Carica un nuovo file di database di eventi biografici 
    -q              : Esce dal programma

    :>";
            Console.Write(txt);
            var args = "a dizionario.txt";

            string cmd; string param;
            cmd = args.Substring(0, 1);
            if (args.Length > 2) param = args.Substring(2);
            else param = "";


            switch (cmd.ToUpper())
            {

                case ("A"):
                    Parse(param);
                    break;

                case ("I"):
                    LoadDb(param, true);
                    break;
                case ("Q"):
                    return;
            }


        }

        static void LoadDb(string path, bool individui)
        {

        }

        static void LoadHelperTables(string file)
        {
            Dictionary<Tuple<string, string>, string> output = new Dictionary<Tuple<string, string>, string>();
            StreamReader stream = new StreamReader(file);
            CsvReader reader = new CsvReader(stream);
            reader.Configuration.Delimiter = "#";

            while (reader.Read())
            {
                string codice, descrizione, tabella;

                tabella = reader.GetField("XCODTAB");
                codice = reader.GetField("XCODELE");
                descrizione = reader.GetField("XDESCELE");

                output.Add(new Tuple<string, string>(tabella, descrizione), codice);
            }

            using (FileStream fs = new FileStream("xtabserv.dat", FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();

                try
                {
                    bf.Serialize(fs, output);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERRORE: Impossibile salvare i dati su file. Messaggio di errore: {0}", ex.Message);
                }

            }

        }


    }
}
