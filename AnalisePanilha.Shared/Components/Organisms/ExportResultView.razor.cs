using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalisePanilha.Shared.Components.Organisms
{
    public partial class ExportResultViewBase : ComponentBase
    {
        [Parameter] public bool HasResults { get; set; }
        [Parameter] public bool IsExporting { get; set; }
        [Parameter] public string ExportedFilePath { get; set; }
        [Parameter] public string ErrorMessage { get; set; }
        [Parameter] public EventCallback OnExport { get; set; }
        [Parameter] public EventCallback OnOpenFile { get; set; }
    }
}
