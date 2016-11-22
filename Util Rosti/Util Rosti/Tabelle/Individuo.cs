using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace Utility_Promus
{
   
	[Serializable()]
	class Individuo : TabellaDB
    {
        #region ***************TABELLA INDIVIDUI*************

        //Conteggio statico del numero
        static int count;
        public static int Count { get { return count; } }

        //Campi privati
        string nome, cognome, provenienza, note;
        List<Attività> attività;



        List<string> voce_o_strumento;
        Data nascita, morte;

        //Campi pubblici
        public bool èMusicista { get; private set; }
        public bool èMaschio { get; private set; }
        public string Note { get
            {
                if (voce_o_strumento.Count <= 1) return note;
                string helper = note;
                for (int i = 1; i < voce_o_strumento.Count; i++)
                    helper += "anche " + voce_o_strumento[i];
                return helper;
            }
        }

        //Descrizioni pubbliche di campi privati
        public string CognomeNome
        {
            get
            { return (this.cognome + ", " + this.nome); }
        }

        public string Provenienza { get { return provenienza; } }

        public string AttivitàPrevalente
        {
            get
            {
                return voce_o_strumento.First();
            //    string s = this.attivitàPrevalente.ToString();
            //    s = s.Replace('_', ' ');
            //    return s;
            }
        }

		public List<Attività> GetAttività ()
		{
			return this.attività;
		}

        /// <summary>
        /// Costruisce un individuo
        /// DEFAULT = Musicista maschio senza attività specifica
        /// </summary>
        public Individuo (string nome, string cognome, string attPreval = "", bool èMusicista = true)
        {
			this.Id = ++count;
            this.nome = nome;
            this.cognome = cognome;
            this.voce_o_strumento = new List<string>();
            voce_o_strumento.Add( attPreval);
            this.èMaschio = true;
            this.èMusicista = èMusicista;
            this.note = string.Empty;
            this.provenienza = string.Empty;
            attività = new List<Attività>(0);
        }

        public void AddNota(string nota, bool aCapo = false)
        {
			string _note = "";
			if (!string.IsNullOrEmpty(this.note))
				_note = note + (aCapo ? ".\n" : "; ");
			_note += nota;
        }

        public void SetProvenienza (string provenienza)
        {
            if (this.provenienza != string.Empty)
                throw new Exception(provenienza);
            else this.provenienza = provenienza;
        }

        public void AddVoce_o_Strumento (string voce)
        {
            voce_o_strumento.Add(voce);
        }

        public void AddAttività (Attività a)
        {
            attività.Add(a);
        }

		public string GetDescrizione ()
		{
			string descr = CognomeNome;
			descr += "\r\n";
			foreach (var v in voce_o_strumento)
				descr += (v + ", ");
			int cAtt = 1;
			foreach (var a in attività)
				descr += (string.Format("\r\n*{0}*: {1}", cAtt++, a.GetDescrizione ()));
			return descr;
		}
        #endregion

        #region ******************ALTRE TABELLE**********************
        // Campi privati
        List<string> nomiAlternativi;
        Titoli titolo;

        public void SetData (Data data, TipoEvento tipo)
        {
            if (tipo == TipoEvento.Nascita)
                nascita = data;
            else morte = data;
        }
        #endregion

    }
}
