using System;

namespace Utility_Promus
{
	 class Riferimento<T>: TabellaDB where T: TabellaDB
	{
		/*
rif_att_id;		att_id;	doc_id;	rif_att_pag;	rif_att_note;	X0VERN;XUTEN;XDTAGG;;;;;;;;;;;;;;;;;;;;;;;;
rif_ind_id;		ind_id;	doc_id;	rif_ind_pag;	rif_ind_note;	X0VERN;XUTEN;XDTAGG;;;;;;;;;;;;;;;;;;;;;;;;
rif_legami_id;	leg_id;	doc_id;	rif_pag;		rif_note;		X0VERN;XUTEN;XDTAGG;;;;;;;;;;;;;;;;;;;;;;;;
nom_id;					doc_id;	rif_nom_pag;rif_nom_note;	X0VERN;XUTEN;XDTAGG;;;;;;;;;;;;;;;;;;;;;;;;;
		 */

		public int Id { get {return base.Id;}}
		public int Rif_Id { get { return _elemento.Id; } }
		public int Doc_Id {get; private set;}
		public string IndicazioniPagine { get; private set; }
		public string Note { get { return GetNote (); } }
		public string X0VERN { get { return this.X0VERN; } }
		public string XUTEN { get { return this.XUTEN; } }
		public string XDTAGG { get { return this.XDTAGG; } }

		T _elemento;


        public Riferimento (T elem, int Doc_Id, string IndicazioniPagine)
		{
            this._elemento = elem;
            this.Doc_Id = Doc_Id;
            this.IndicazioniPagine = IndicazioniPagine;
		}
	}
}

