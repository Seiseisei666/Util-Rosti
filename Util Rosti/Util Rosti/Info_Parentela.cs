using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility_Promus
{
    class Info_Parentela
    {
        Individuo individuo;
        string s_altro_individuo;
        TerminiDiParentela tipo_parentela;

        public Info_Parentela (Individuo individuo, string altro, TerminiDiParentela tipo)
        {
            this.individuo = individuo;
            this.s_altro_individuo = altro;
            this.tipo_parentela = tipo;
        }
    }
}
