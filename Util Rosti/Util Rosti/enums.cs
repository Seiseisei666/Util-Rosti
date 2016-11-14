namespace Utility_Promus
{
    
    enum Titoli
    {
        Don,
        Maestro
    }

    enum TipoEvento
    {
        Nascita,
        Morte
    }

    enum Mesi
    {
        NON_ASS = -1,
        INVALIDO = 0,
        Gennaio = 1,
        Febbraio,
        Marzo,
        Aprile,
        Maggio,
        Giugno, Luglio, Agosto, Settembre, Ottobre, Novembre, Dicembre
    }

    public enum TipoData
    { NA= 0, prima_di = 1, sino_a, a_partire_da, dopo_il, il, intorno_a, tra};

    enum TerminiDiParentela
{
    figlio, padre, fratello, nipote, cugino, parente
        };

    enum TipoAttività
    {
        AUTO,nascita, morte, ingresso, uscita
    }

	enum TipoFiltro
	{
		Data,
		Fonte,
	}


}
