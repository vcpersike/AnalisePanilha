using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalisePanilha.Shared.Components.Molecules
{
    public class ColumnInfo
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string ColumnLetter { get; set; }

        public ColumnInfo(int index, string name, string columnLetter)
        {
            Index = index;
            Name = name;
            ColumnLetter = columnLetter;
        }
    }
}
