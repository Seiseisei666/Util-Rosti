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
                    List<Tuple<string,int>>>>
          
            _tabelle;

        public DataBase()
        {
            _tabelle = new Dictionary<string, Dictionary<string, List<Tuple<string, int>>>>(16);
        }
        
        public void ImportTable (string nome, Dictionary<string,List<string>> tabella)
        {
            int row = 0;
            var new_tab = new Dictionary<string, List<Tuple<string, int>>>(tabella.Count);


            foreach (string key in tabella.Keys)
            {
                var lista = new List<Tuple<string, int>>(tabella[key].Count);
                row = 0;
                foreach (var s in tabella[key])
                    lista.Add(new Tuple<string, int>(s, row++));

                new_tab.Add(key, lista);
            }

            _tabelle.Add(nome, new_tab);

        }

        /// <summary>
        /// Restituisce la lista dei valori relativi a un dato campo
        /// </summary>
        public List <string> GetValues (string tabella, string campo)
        {
            Dictionary<string, List<Tuple<string,int>>> _tabella;
            List<Tuple<string,int>> risultati;

            if (_tabelle.TryGetValue(tabella, out _tabella))
            {
                if (_tabella.TryGetValue(campo, out risultati))
                {
                    var vals = risultati.Select(r => r.Item1);
                    return vals.ToList();
                }
            }
            return null;
        }

        public Dictionary<string,string> Entry (string tabella, string query)
        {
            Dictionary<string, List<Tuple<string,int>>> _tabella;
            Dictionary<string, string> risultati;
            int index = -1;

            if (_tabelle.TryGetValue(tabella, out _tabella))
            {
                foreach (var list in _tabella.Values)
                {
                    index = list.First(t => t.Item1 == query).Item2;
                    if (index >= 0) break;
                }

                if (index == -1) return null;

                risultati = new Dictionary<string, string>();

                foreach (var field in _tabella.Keys)
                {
                    risultati.Add(field, _tabella[field][index].Item1);
                }
                return risultati;
            }
            return null;
        }



    }
}
