using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Utility_Rostirolla
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

            var fil2e = @"DAJE qwieqweiuqhweiuqhweihqwe
Bibliografia: qwoeiquweoiwquewq

DAJE2, porcodio (fl. 12309123).
ALto.
AOSdiasodijasdoiasjdoasifasofih
Bibliografia:

DA = ASC

DAJE3, qweqwe (123123).
Basso.
ASDJASOJAOSIGJASOIGJosigjaosigjasogijasogijasogiasjgoias
Bibliografia:
Ciao!

DAJE4, qwoeijqweoqwie (provaprova)
provaprovaprovaprova
prova
PROVA
prova=PROVA

DAJE5 = DAJE6";

           // debug(file);
            

            Console.WriteLine("Completata lettura; caricato dizionario da {0} righe",file.Length );
            Console.WriteLine("Inizio parsing...");

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

            Console.WriteLine("Parsing completato!\n\nNumero di paragrafi letti: {0}\n\nNumero di paragrafi completi: {1}", p.Entries, p.EntriesOk);

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
