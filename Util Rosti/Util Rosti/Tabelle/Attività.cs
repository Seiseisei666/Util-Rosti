using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility_Promus
{
	class Attività: TabellaDB
    {
        //Conteggio statico del numero
        static int count;
        public static int Count { get { return count; } }


        TipoAttività tipo;
		public Individuo Individuo { get; private set; }
        string descrizione, indirizzo, occasioneSpec, siglaIstituzione;
        //TODO: LOCALITà ID
		public bool Puntuale {get; private set;}
		public TipoData tipoInizio {get; private set;}
		public TipoData tipoFine{get; private set;}

		public Data inizioMin{get; private set;}
		public Data inizioMax{get; private set;}
		public Data fineMin{get; private set;} 
		public Data fineMax{get; private set;}
        //Data iniMinLav, iniMaxLav, fineMinLav, fineMaxLav;

        public Data getDataLav(bool min, bool ini = true)
        {
            if (Puntuale)
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
			base.Id = ++count;
			this.Individuo = ind;
            this.descrizione = descrizione.RemoveNewLineChars();
            this.inizioMin = data;
            this.inizioMax = data;
            this.Puntuale = true;
            this.tipoInizio = tipo;
            this.tipo = TipoAttività.AUTO;
        }

        public Attività(Individuo ind, string descrizione,Data inizio, Data fine)
        {
            if (inizio == null || fine == null) throw new ArgumentNullException();
			this.Id = ++count;
			this.Individuo = ind;
            this.descrizione = descrizione.RemoveNewLineChars();
            this.inizioMin = inizio;
            this.inizioMax = inizio;
            this.fineMin = fine;
            this.fineMax = fine;
            this.Puntuale = true;
            this.tipoInizio = TipoData.tra;
            this.tipo = TipoAttività.AUTO;
        }

        public string GetDescrizione ()
        {
            if (Puntuale && fineMin == null && fineMax == null)
            {
                return tipoInizio.ToString().ToCapitalCase() + " " + inizioMin.ToString() + ": '" + descrizione + "'";
            }

            else
                return "Tra il " + inizioMin.ToString() + " e il " + fineMin.ToString() + ": '" + descrizione + "'";

        }
    }
}
