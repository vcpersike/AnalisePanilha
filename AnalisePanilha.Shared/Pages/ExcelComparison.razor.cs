using AnalisePanilha.Shared.ViewModels;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnalisePanilha.Shared.Components.Molecules;

namespace AnalisePanilha.Shared.Pages
{
    public partial class ExcelComparisonBase : ComponentBase, IDisposable
    {
        [Inject] protected ExcelComparisonViewModel ViewModel { get; set; }

        protected override void OnInitialized()
        {
            Console.WriteLine("ExcelComparison: OnInitialized");
            ViewModel.OnStateChanged += StateHasChanged;
        }

        public void Dispose()
        {
            Console.WriteLine("ExcelComparison: Dispose");
            ViewModel.OnStateChanged -= StateHasChanged;
        }

        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine("ExcelComparison: OnInitializedAsync");
            // Você pode descomentar isto para testes, mas mantenha comentado para produção
            // await LoadSampleFiles();
        }

        protected async Task HandleFile1Selected(InputFileChangeEventArgs e)
        {
            Console.WriteLine($"Arquivo 1 selecionado: {e.File.Name}");
            await ViewModel.HandleFileSelection(e, 1);

            // Após selecionar ambos os arquivos, carregue as informações das colunas
            if (!string.IsNullOrEmpty(ViewModel.FileName1) && !string.IsNullOrEmpty(ViewModel.FileName2))
            {
                await ViewModel.LoadColumnInfo();
            }
        }

        protected async Task HandleFile2Selected(InputFileChangeEventArgs e)
        {
            Console.WriteLine($"Arquivo 2 selecionado: {e.File.Name}");
            await ViewModel.HandleFileSelection(e, 2);

            // Após selecionar ambos os arquivos, carregue as informações das colunas
            if (!string.IsNullOrEmpty(ViewModel.FileName1) && !string.IsNullOrEmpty(ViewModel.FileName2))
            {
                await ViewModel.LoadColumnInfo();
            }
        }

        protected async Task HandleCompare()
        {
            Console.WriteLine("Botão Comparar clicado");
            await ViewModel.CompareFiles();
        }

        protected void HandleReset()
        {
            Console.WriteLine("Botão Limpar clicado");
            ViewModel.ResetSelection();
        }

        protected async Task HandleExport()
        {
            Console.WriteLine("Botão Exportar clicado");
            await ViewModel.ExportResults();
        }

        protected async Task HandleOpenFile()
        {
            Console.WriteLine("Botão Abrir Arquivo clicado");
            // Implementar a lógica para abrir o arquivo
        }

        protected async Task LoadSampleFiles()
        {
            // Método para carregar arquivos de exemplo durante o desenvolvimento
            Console.WriteLine("Carregando arquivos de exemplo");
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            await ViewModel.CompareSpecificFiles(
                Path.Combine(baseDir, "SampleData", "sample1.xlsx"),
                Path.Combine(baseDir, "SampleData", "sample2.xlsx")
            );
        }

        // Implementação do método HandleError referenciado no Razor
        protected void HandleError(string errorMessage)
        {
            Console.WriteLine($"Erro de validação: {errorMessage}");
            // O StateHasChanged não é necessário aqui porque o ViewModel já dispara um evento OnStateChanged
        }
        protected void HandleApplyColumnMappings(ColumnPair columnPair)
        {
            Console.WriteLine($"Colunas selecionadas: {columnPair.File1Column} -> {columnPair.File2Column}");
            ViewModel.SetColumnPairForComparison(columnPair);
        }
    }
}
