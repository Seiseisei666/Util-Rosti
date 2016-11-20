using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility_Promus
{
    class Attività
    {
        //Conteggio statico del numero
        static int count;
        public static int Count { get { return count; } }

        //Campi privati
        int id;
        TipoAttività tipo;
        Individuo individuo;
        string descrizione, indirizzo, occasioneSpec, siglaIstituzione;
        //TODO: LOCALITà ID
        bool puntuale;
        TipoData tipoInizio, tipoFine;

        Data inizioMin, inizioMax, fineMin, fineMax;
        //Data iniMinLav, iniMaxLav, fineMinLav, fineMaxLav;

        public Data getDataLav(bool min, bool ini = true)
        {
            if (puntuale)
            {
                if (min) return inizioMin.Inizio;
                else return inizioMax.Fine;
            }
            else
            {
                if (min && ini) return inizioMin.Inizio;
                else if (min && !ini) return inizioMax.Inizio;
                else if (!min && ini) return fineMin.Inizio;
                else return fineMax.Inizio;
            }
        }

        /// <summary>
        /// Costruttore evento con data puntuale
        /// </summary>
        public Attività (Individuo ind, string descrizione,TipoData tipo, Data data)
        {
            if (data == null) throw new ArgumentNullException("data");
            this.id = ++count;
            this.individuo = ind;
            this.descrizione = descrizione;
            this.inizioMin = data;
            this.inizioMax = data;
            this.puntuale = true;
            this.tipoInizio = tipo;
            this.tipo = TipoAttività.AUTO;
        }

        public Attività(Individuo ind, string descrizione,Data inizio, Data fine)
        {
            this.id = ++count;
            this.individuo = ind;
            this.descrizione = descrizione;
            this.inizioMin = inizio;
            this.inizioMax = inizio;
            this.fineMin = fine;
            this.fineMax = fine;
            this.puntuale = true;
            this.tipoInizio = TipoData.tra;
            this.tipo = TipoAttività.AUTO;
        }

        public string GetDescrizione ()
        {
            if (puntuale && fineMin == null && fineMax == null)
            {
                return tipoInizio.ToString().ToCapitalCase() + inizioMin.ToString() + ": '" + descrizione + "'";
            }

            else
                return "Tra il " + inizioMin.ToString() + " e il " + fineMin.ToString() + ": '" + descrizione + "'";

        }
    }
}
