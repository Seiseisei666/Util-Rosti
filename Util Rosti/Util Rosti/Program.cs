using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using Utility_Promus.Base_Dati;
using CsvHelper;

namespace Utility_Promus
{
    class Program
    {
        public static Dictionary<Tuple<string, string>, string> Tabelle_Helper { get; private set; }

        static readonly string pathDb = @"data.bin";
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
            //Init();
            Console.Write(header);
            //GetSelection();
            Parse("dizionario.txt");
            Console.WriteLine("\nPremere un tasto per uscire dal programma.");
            Console.ReadKey();
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
				file = File.ReadAllText(filename, Encoding.Default);
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

     //       var sel = Console.ReadKey().Key;

//            if (sel == ConsoleKey.S)
//            {
                Console.WriteLine("Inizio scrittura su file...");


                File.WriteAllLines(@"export.txt", p.Export());

                Console.WriteLine("Scrittura su file completata.\nPremere un tasto per continuare");

          //  }

          //  Console.ReadKey();
			return;
        }


        static void Init()
        {
            Console.Write(@"
        INIZIALIZZAZIONE IN CORSO...");

            string txt = "";
            bool dbPres = File.Exists(pathDb);
            if (dbPres) txt = "\n - Database identificato...";
            else txt += "\n - ATTENZIONE!!! Database non presente!";
            Console.Write(txt);

            DataBase _database;

            Console.WriteLine("\nCaricamento database in corso...");

            //Caricamento DB
            try
            {
                using (FileStream stream = new FileStream(pathDb, FileMode.Open))
                {
                    var bformatter = new BinaryFormatter();
                    _database = (DataBase)bformatter.Deserialize(stream);
                }

                var nomi = _database.GetValues("individui", "ind_nome");

                for (int i = 0; i < 100; i++)
                    Console.WriteLine(nomi[i]);
            }

            catch (Exception e)
            {
                Console.WriteLine("Impossibile caricare il database. Errore di lettura file");
                Console.WriteLine("Messaggio di errore:\n" + e.Message);
            }

                return;

        }

        static void GetSelection()
        {
            string txt = @"

    -a filename.txt : Analizza un dizionario dei cantori e ne estrae le informazioni
    -i filename.csv : Carica un nuovo file di database
    -q              : Esce dal programma

    :>";
            Console.Write(txt);
            // var args = "a dizionario.txt";

            var args = Console.ReadLine();

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
                    LoadDb(param);
                    break;
                case ("Q"):
                    return;
            }


        }

        static void LoadDb(string path)
        {
            DBImporter importer = new DBImporter(path);
            
                importer.StartImport();
           
            importer.Save();
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
