using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility_Promus
{
    [AttributeUsageAttribute(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = true)]
    class CSVField : Attribute
    {
        public string fieldName { get; private set; }
        public CSVField(string fieldName)
        { this.fieldName = fieldName; }
    }
}
