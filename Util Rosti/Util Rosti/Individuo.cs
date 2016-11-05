using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;



namespace Utility_Promus
{
   

    class Individuo
    {
        #region ***************TABELLA INDIVIDUI*************

        //Conteggio statico del numero
        static int count;
        public static int Count { get { return count; } }

        //Campi privati
        int id;
        string nome, cognome, provenienza;
        Attività attivitàPrevalente;
        Data nascita, morte;

        //Campi pubblici
        public bool èMusicista { get; private set; }
        public bool èMaschio { get; private set; }
        public string Note { get; private set; }

        //Descrizioni pubbliche di campi privati
        public string CognomeNome
        {
            get
            { return (this.cognome + ", " + this.nome); }
        }

        public string AttivitàPrevalente
        {
            get
            {
                string s = this.attivitàPrevalente.ToString();
                s = s.Replace('_', ' ');
                return s;
            }
        }

        /// <summary>
        /// Costruisce un individuo
        /// DEFAULT = Musicista maschio senza attività specifica
        /// </summary>
        public Individuo (string nome, string cognome, Attività attPreval = Attività.NULL, bool èMusicista = true)
        {
            this.id = ++count;
            this.nome = nome;
            this.cognome = cognome;
            this.attivitàPrevalente = attPreval;
            this.èMaschio = true;
            this.èMusicista = èMusicista;
            this.Note = string.Empty;
            this.provenienza = string.Empty;
        }

        public void AddNota(string nota, bool aCapo = false)
        {
            Note = Note + (aCapo ? ".\n" : "; ") + nota;
        }

        public void SetProvenienza (string provenienza)
        {
            if (this.provenienza != string.Empty)
                throw new Exception(provenienza);
            else this.provenienza = provenienza;
        }

        #endregion

        #region ******************ALTRE TABELLE**********************
        // Campi privati
        List<string> nomiAlternativi;
        Titoli titolo;

        public void SetData (Data data, TipoData tipo)
        {
            if (tipo == TipoData.Nascita)
                nascita = data;
            else morte = data;
        }
        #endregion

    }
}
