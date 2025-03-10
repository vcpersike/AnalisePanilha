using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalisePanilha.Shared.Components.Molecules
{
    public partial class ComparisonItemBase : ComponentBase
    {
        [Parameter] public int Row { get; set; }
        [Parameter] public int Column { get; set; }
        [Parameter] public string Value1 { get; set; }
        [Parameter] public string Value2 { get; set; }
        [Parameter] public bool IsDifferent { get; set; }
    }
}
