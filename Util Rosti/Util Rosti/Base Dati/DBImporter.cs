using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Utility_Promus.Base_Dati
{
    class DBImporter
    {

        Dictionary<string,List<List<string>>> _dati;

        DataBase _database;
        string _filename;

        public DBImporter(string filename)
        {
            _database = new DataBase();
            _filename = filename;
            Console.WriteLine("Inizio importazione Database...");
        }

        public void StartImport ()
        { 
            CsvParser parser;

            bool errori = false;
            int count = 0;


            try
            {
                using (var stream = new StreamReader(_filename))
                {
                    parser = new CsvParser(stream);
                    parser.Configuration.HasHeaderRecord = false;
                    parser.Configuration.Delimiter = ("#");

                    List<string> campi = null;
                    Dictionary<string, List<string>> entries = null;
                    string nome_tabella = "";
                    int progress = 0;

                    while (true)
                    {
                        var row = parser.Read();
                        if (row == null) break;

                        //Progress bar
                        progress++; count++;
                        if (progress >= 100)
                            Console.Write("*");
                        if (progress >= 2000)
                        {
                            Console.Write("\n");
                            progress = 0;
                        }

                        //Riga di intestazione campi
                        if (!string.IsNullOrEmpty(row.First()))
                        {
                            //fLUSH
                            if (entries != null)
                                toDatabase(nome_tabella, entries);
                            //Inizio nuova tabella
                            nome_tabella = row.First();
                            Console.WriteLine("TABELLA: {0}", nome_tabella);
                            progress = 0;

                            //Salvo i nomi dei campi
                            campi = row.Skip(1)
                                .Where (s => !string.IsNullOrEmpty(s))
                                .ToList();
                            //Nuovo Dizionario
                            entries = new Dictionary<string, List<string>>(16);

                            foreach (var c in campi)
                            {
                                entries.Add(c, new List<string>(20));
                            }
                            continue;
                        }

                        //Dati
                        else
                        {
                            var val = row.Skip(1)
                                .Where (s=> !string.IsNullOrEmpty(s))
                                .ToList();
                            
                            for (int i = 0; i < val.Count(); i++)
                            {
                                string campo = campi[i];
                                entries[campo].Add(val[i]);
                            }
                        }
                    }

                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Impossibile accedere al file: file corrotto o inesistente");
                errori = true;
            }
            catch (CsvParserException)
            {
                Console.WriteLine("Impossibile leggere il file: formato invalido");
                errori = true;
            }
            finally
            {
                Console.WriteLine("Operazione terminata.");
                if (errori) Console.WriteLine("Sono stati riscontrati degli errori");
                else
                    Console.WriteLine("Caricate {0} righe.", count);
            }
  
        }

        void toDatabase(string nome, Dictionary <string, List <string>> entries)
        {
            _database.ImportTable(nome, entries);
        }

        public void Save ()
        {
            Console.WriteLine("Salvo le informazioni su disco...");

            var frm = new BinaryFormatter();
            using (var writer = new FileStream(@"data.bin", FileMode.Create))
                try
                {
                    frm.Serialize(writer, _database);
                }
                catch (Exception)
                {
                    Console.WriteLine("Si è verificato un errore. Scrittura su file interrotta");
                }

        }
    }
}
