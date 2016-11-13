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
    class Scanner
    {

        bool isRunning;

        string testo;

        Dictionary<Regex, List<Regex>> relazioni;

        public Scanner()
        {
            CsvReader reader = new CsvReader(new System.IO.StringReader(Properties.Resources.filtri));
            reader.Configuration.Delimiter = ";";

            var validEntries = new Dictionary<int,Tuple<string, string, int[]>>();

            try
            {
                while (reader.Read())
                {
                    string tipo, figli, pattern; int id;

                    tipo = reader.GetField("TIPO");
                    if (tipo != "Data") continue;

                    pattern = reader.GetField("REGEX");
                    figli = reader.GetField("CHILDREN");
                    id = reader.GetField<int>("ID");

                    var idsFigli = 
                        figli.Split(',')
                        .Select(s => int.Parse(s))
                        .ToArray<int>();
                    
                    validEntries.Add(id, new Tuple<string, string, int[]>("", pattern, idsFigli));
                }
            }
            catch 
            {
                throw;
            }


            var regexGenerati = new Regex[validEntries.Max(e => e.Key)+1];
            foreach (var e in validEntries)
                regexGenerati[e.Key] = new Regex(e.Value.Item2, RegexOptions.Compiled);

            var padri = validEntries.Where(
                e => !validEntries.Values.Any(
                    t => t.Item3.Contains(e.Key)));

            relazioni = new Dictionary<Regex, List<Regex>>();

            foreach (var p in padri)
            {
                var figli = new List<Regex>(p.Value.Item3.Count());
                foreach (int i in p.Value.Item3)
                    figli.Add(regexGenerati[i]);
                relazioni.Add(regexGenerati[p.Key], figli);
            }
            
        }

        public void Scan (string testo)
        {
            this.testo = testo;
            this.isRunning = true;

            foreach (var pattern in relazioni.Keys)
            {
                if (isRunning)
                    tryMatch(pattern);
            }
        }

        void tryMatch (Regex regex)
        {
            Match match = regex.Match(testo);
            Regex next;
            if (match.Success)
            {
                Regex next = null;
                if (relazioni)
            }

        }
    }
}
