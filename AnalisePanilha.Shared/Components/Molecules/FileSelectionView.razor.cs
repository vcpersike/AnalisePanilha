using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalisePanilha.Shared.Components.Molecules
{
    public partial class FileSelectionViewBase : ComponentBase
    {
        [Parameter] public string FileName1 { get; set; }
        [Parameter] public string FileName2 { get; set; }
        [Parameter] public bool CanCompare { get; set; }
        [Parameter] public bool IsComparing { get; set; }
        [Parameter] public string ErrorMessage { get; set; }
        [Parameter] public EventCallback<InputFileChangeEventArgs> OnFile1Selected { get; set; }
        [Parameter] public EventCallback<InputFileChangeEventArgs> OnFile2Selected { get; set; }
        [Parameter] public EventCallback OnCompare { get; set; }
        [Parameter] public EventCallback OnReset { get; set; }
        [Parameter] public EventCallback<string> OnError { get; set; }

        protected async Task OnFileSelected(InputFileChangeEventArgs e, int fileNumber)
        {
            if (fileNumber == 1)
                await OnFile1Selected.InvokeAsync(e);
            else
                await OnFile2Selected.InvokeAsync(e);
        }
        protected void HandleValidationError(string errorMessage)
        {
            // Você pode modificar essa implementação para atender às suas necessidades
            // Por exemplo, se o componente pai precisa ser notificado sobre o erro:
            OnError.InvokeAsync(errorMessage);

            // Se o parâmetro ErrorMessage é um parâmetro de duas vias (two-way binding)
            // você deveria implementar uma forma de atualizar esse valor
            // Isso depende de como você está gerenciando os estados em sua aplicação
        }
    }
}
