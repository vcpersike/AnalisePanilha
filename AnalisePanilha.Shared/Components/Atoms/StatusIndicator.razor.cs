using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalisePanilha.Shared.Components.Atoms
{
    public partial class StatusIndicatorBase : ComponentBase
    {
        [Parameter] public bool IsDifferent { get; set; }

        protected string StatusClass => IsDifferent ? "status-different" : "status-equal";
        protected string Text => IsDifferent ? "Diferente" : "Igual";
        protected string Icon => IsDifferent ? "❌" : "✓";
    }
}
