using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalisePanilha.Shared.Models
{
    public class CellComparisonResult
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public bool IsDifferent { get; set; }
    }

}
