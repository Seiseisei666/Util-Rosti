using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Utility_Promus
{
    class Program
    {
  

        static void Main(string[] args)
        {
           // Regex regexFloruit = new Regex(@"(fl\.\s)(\d{1,3}|[IVX]{1,3})(\/)(\d{1,3}|[IVX]{1,3})(\/)(\d{4})");
			string file = string.Empty;
			try 
			{
				file = File.ReadAllText(@"dizionario.txt");
			}
			catch (Exception ex) 
			{
				if (ex is FileNotFoundException)
					Console.WriteLine ("File non trovato!");
			}

           

           // debug(file);
            

            Console.WriteLine("Completata lettura; caricato dizionario da {0} righe.",file.Length );


            //int x = 0;
            //bool prec = false;
            //for (int i = 0; i < file.Length; i++)
            //{
            //    char c = file.ElementAt(i);
            //    if (c.Equals('\n'))
            //        {
            //        if (prec == true) { System.Diagnostics.Debug.WriteLine(x++); prec = false; }
            //        else prec = true;

            //        }
            //}
            //qwe
            Parser p = new Parser(file);
            p.Start();

            Console.WriteLine("Analisi completata!\n\nNumero di voci analizzate: {0}\n\nNumero di voci valide: {1}", p.Entries, p.EntriesOk);

            Console.WriteLine("Inizio scrittura su file...");


           // File.WriteAllLines(@"export.txt", p.Export());

            Console.WriteLine("Scrittura su file completata.\nPremere un tasto per continuare");
            
            Console.ReadKey();
        }

        static void debug (string file)
        {
            var car = file.ToCharArray();
            char b;
            foreach (char c in car)
            {
                if (c.Equals('\r')) b = 'à';
                else if (c.Equals('\n')) b = 'ç';
                else b = c;
                Console.Write(b);
            }
            Console.ReadKey();
            return;

        }
    }
}
