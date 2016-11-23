using System;
using CsvHelper.Configuration;

namespace Utility_Promus
{
	class CSVIndividuo: CsvClassMap<Individuo>
	{

		public CSVIndividuo ()
		{
			Map (m => m.Id);
			Map (m => m.CognomeNome);
			Map (m => m.AttivitàPrevalente);
			Map (m => m.èMusicista.Chars("s", "n"));
			Map (m => m.èMaschio.Chars ("m","f"));
			Map (m => m.Provenienza);
			Map (m => m.Note);
			Map (m => m.X0VERN);
			Map (m => m.XUTEN);
			Map (m => m.XDTAGG);
		}
	}
}

