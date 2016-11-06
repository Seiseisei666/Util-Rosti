using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using System.IO;

namespace Utility_Promus
{
    class CSVManager
    {
        CsvParser parser;
        public CSVManager()
        {
            parser = new CsvParser(new StreamReader("prova.csv"));
            parser.Configuration.Delimiter = "#";

            while (true)
            {
                var riga = parser.Read();
                if (riga == null) break;

            }
        }
    }
}
