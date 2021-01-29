using Nerine.Data;

namespace Nerine.Collections.Tables
{
    public class Column
    {
        public string Name;

        public StructureType Type;

        public object Default = null;

        public bool Primary = false;
        
        public bool Nullable = false;
    }
}
