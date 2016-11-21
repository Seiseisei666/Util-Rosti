using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Utility_Promus.Base_Dati
{
    [Serializable]
    public class DataBase
    {

        Dictionary
            <string,                    
                Dictionary<string,
                    List<string>>>
          
            _tabelle;

        public DataBase()
        {
            _tabelle = new Dictionary<string, Dictionary<string, List<string>>>(16);
        }
        
        public void ImportTable (string nome, Dictionary<string,List<string>> tabella)
        {
            _tabelle.Add(nome, tabella);
        }

        /// <summary>
        /// Restituisce la lista dei valori relativi a un dato campo
        /// </summary>
        public List <string> GetValues (string tabella, string campo)
        {
            Dictionary<string, List<string>> _tabella;
            List<string> risultati;

            if (_tabelle.TryGetValue(tabella, out _tabella))
            {
                if (_tabella.TryGetValue(campo, out risultati))
                {
                    return risultati;
                }
            }
            return null;
        }


        public Dictionary<string,string> Entry (string tabella, string query)
        {


            Dictionary<string, List<string>> _tabella;
            Dictionary<string, string> risultati;
            int index = -1;

            if (_tabelle.TryGetValue(tabella, out _tabella))
            {
                foreach (var valori in _tabella.Values)
                {
                    index = valori.IndexOf(valori.First(v => v == query));
                    if (index >= 0) break;
                }

                if (index == -1) return null;

                risultati = new Dictionary<string, string>();

                foreach (var field in _tabella.Keys)
                {
                    risultati.Add(field, _tabella[field][index]);
                }
                return risultati;
            }
            return null;
        }

        public dynamic GetRecord (string tabella, string query)
        {
            Dictionary<string, List<string>> _tabella;

            if (_tabelle.TryGetValue (tabella, out _tabella))
            {
                var index =
                    (from key in _tabella.Keys
                     from val in _tabella[key]
                     where val == query
                     select _tabella[key].IndexOf(val)).First();

                var result =
                    from key in _tabella.Keys
                    select new
                    {
                        field = key,
                        value = _tabella[key][index]
                    };

                return result;
            }

            return null;
        }

        public dynamic GetRecord (string tabella, int riga)
        {
            Dictionary<string, List<string>> _tabella;

            if (_tabelle.TryGetValue(tabella, out _tabella))
            {
                try
                {
                    var result =
                        from key in _tabella.Keys
                        select new
                        {
                            field = key,
                            value = _tabella[key][riga]
                        };
                    return result;
                }
                catch (IndexOutOfRangeException)
                {
                  
                }
            }
            return null;
        }


    }
}
