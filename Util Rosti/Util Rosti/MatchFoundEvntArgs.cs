using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Utility_Promus
{
    public class MatchFoundEvntArgs:EventArgs
    {
        public Match Corrispondenza { get; private set; }
        public MatchFoundEvntArgs (Match m)
        {
            Corrispondenza = m;
        }
    }
}
