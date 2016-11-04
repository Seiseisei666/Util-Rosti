using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



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
        public Individuo (string nome, string cognome, Attività attPreval = Attività.NULL, bool èMusicista = true, bool èMaschio = true, string note = "NULL", string provenienza = "NULL")
        {
            this.id = ++count;
            this.nome = nome;
            this.cognome = cognome;
            this.attivitàPrevalente = attPreval;
            this.èMaschio = èMaschio;
            this.èMusicista = èMusicista;
            this.Note = note;
            this.provenienza = provenienza;
        }
        #endregion

        #region ******************ALTRE TABELLE**********************
        // Campi privati
        List<string> nomiAlternativi;
        Titoli titolo;


        #endregion

    }
}
