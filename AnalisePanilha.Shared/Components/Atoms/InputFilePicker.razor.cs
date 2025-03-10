using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace AnalisePanilha.Shared.Components.Atoms
{
    public partial class InputFilePickerBase : ComponentBase
    {
        [Parameter] public string Label { get; set; } = "Arquivo";
        [Parameter] public string FileName { get; set; }
        [Parameter] public EventCallback<InputFileChangeEventArgs> OnChange { get; set; }
        [Parameter] public EventCallback<string> OnValidationError { get; set; }

        protected string Id = Guid.NewGuid().ToString("N");
        protected string ValidationMessage { get; set; }

        [Inject] protected IJSRuntime JSRuntime { get; set; }

        protected async Task BrowseFile()
        {
            await JSRuntime.InvokeVoidAsync("eval", $"document.getElementById('{Id}').click()");
        }

        protected async Task OnFileSelected(InputFileChangeEventArgs e)
        {
            if (e.FileCount > 0)
            {
                var file = e.File;

                // Extrair o nome do arquivo sem a extensão
                string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(file.Name);

                // Verificar se o nome do arquivo contém apenas letras e números
                if (!IsAlphanumeric(fileNameWithoutExtension))
                {
                    ValidationMessage = "O nome do arquivo deve conter apenas letras e números.";
                    await OnValidationError.InvokeAsync(ValidationMessage);
                    return;
                }

                // Limpar mensagem de validação se tudo estiver correto
                ValidationMessage = string.Empty;

                // Prosseguir com o evento de alteração de arquivo
                await OnChange.InvokeAsync(e);
            }
        }

        // Método para verificar se uma string contém apenas caracteres alfanuméricos
        private bool IsAlphanumeric(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            // Verifica se a string contém apenas letras e números
            return Regex.IsMatch(text, "^[a-zA-Z0-9]+$");
        }
    }
}
