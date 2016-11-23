using System;
using CsvHelper.Configuration;

namespace Utility_Promus
{
	class CSVAttività: CsvClassMap<Attività>
	{
		/*
		 * 
		att_tipo;att_incarico;att_strumento;att_registro;
		att_stabile;att_sal_imp;att_sal_divisa;att_sal_tipo;
		att_incert;att_note;
		X0VERN;XUTEN;XDTAGG

*/
		public CSVAttività ()
		{
			Map (m => m.Id);
			Map (m => m.Individuo.Id);
			Map (m => "NULL");
			Map (m => m.Puntuale.Chars("Si","No"));
			Map (m => m.tipoInizio);
			Map (m => m.tipoFine);
			Map (m => m.inizioMin.FormatoDB());
			Map (m => m.inizioMax.FormatoDB());
			Map (m => m.fineMin.FormatoDB());
			Map (m => m.fineMax.FormatoDB());
			Map (m => m.getDataLav (true, true));
			Map (m => m.getDataLav (false, true));
			Map (m => m.getDataLav (true, false));
			Map (m => m.getDataLav (false, false));
			Map (m => "");								//Tipo Attività
			Map (m => "NULL");
			Map (m => "NULL");
			//TODO: ISTITUZIONE
			Map (m => "");

			Map (m => m.Id);

			Map (m => m.GetDescrizione ());
			//Campi di sistema
			Map (m => m.X0VERN);
			Map (m => m.XUTEN);
			Map (m => m.XDTAGG);
		}
	}
}

