using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalisePanilha.Shared.Components.Atoms
{
    public partial class LabelStyledBase : ComponentBase
    {
        [Parameter] public string Text { get; set; }
        [Parameter] public string Class { get; set; } = "";
    }
}
