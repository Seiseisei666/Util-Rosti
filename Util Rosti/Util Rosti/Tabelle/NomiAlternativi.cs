using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility_Promus.Tabelle
{
    class NomiAlternativi : TabellaDB
    {
        public int Id { get; private set; }
        public int Ind_Id { get { return individuo.Id; } }
        public string Nome { get; private set;  }
        public string Tipo { get { return codici[tipo]; } }
        public string NotiziaIncerta { get { return notizia_incerta.Chars("S", "N");} }
        public string Note { get { return GetNote(); } }
        public string X0VERN { get { return this.X0VERN; } }
        public string XUTEN { get { return this.XUTEN; } }
        public string XDTAGG { get { return this.XDTAGG; } }


        static Dictionary<TipoNomeAlternativo, string> codici = new Dictionary<TipoNomeAlternativo, string>
        {
            {TipoNomeAlternativo.Appellativo, "appellativo" },
            {TipoNomeAlternativo.Origine, "or" },
            {TipoNomeAlternativo.Soprannome, "sop" },
            {TipoNomeAlternativo.Variante, "var" }
        };

        static int count;
        TipoNomeAlternativo tipo;
        bool notizia_incerta;
        Individuo individuo;

        public NomiAlternativi ( Individuo individuo, string nome, TipoNomeAlternativo tipo = TipoNomeAlternativo.Variante)
        {
            this.individuo = individuo;
            notizia_incerta = !nome.Contains("?");
            this.Nome = nome.Trim();
            this.tipo = tipo;
            this.Id = ++count;
        }
    }
}
