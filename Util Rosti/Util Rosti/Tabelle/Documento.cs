using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility_Promus.Tabelle
{
    class Documento: TabellaDB
    {
        public int Id { get; private set; }
        public string Sigla { get; private set; }
        public string Tipo { get { return codici[tipo]; } }
        public string Forma { get { return "stampato"; } }
        public string Autore { get; private set; }
        public string Titolo { get; private set; }
        public string Rivista { get { return "NULL"; } }
        public string Pagine { get; private set; }
        public string Doc_Serie { get { return "NULL"; } }
        public string Doc_Luogo { get; private set; }
        public string Doc_Anno { get; private set; }
        public int Anno { get; private set; }
        public string Note { get { return GetNote(); } }
        public string Arc_Ic { get { return "NULL"; } }
        public string Doc_Collocazione { get { return "NULL"; } }
        public string X0VERN { get { return this.X0VERN; } }
        public string XUTEN { get { return this.XUTEN; } }
        public string XDTAGG { get { return this.XDTAGG; } }

        static int count;
        TIpoDocumentoBibliografico tipo;
        Dictionary<TIpoDocumentoBibliografico, string> codici = new Dictionary<TIpoDocumentoBibliografico, string>
        {
            {TIpoDocumentoBibliografico.FonteBibliografica, "b" },
            {TIpoDocumentoBibliografico.FonteDocumentale, "f" }
        };

        public override string GetNote()
        {
            return "NULL";
        }
    }
}
