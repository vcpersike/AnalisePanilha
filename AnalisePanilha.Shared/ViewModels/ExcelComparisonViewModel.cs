using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AnalisePanilha.Shared.Components.Molecules;
using AnalisePanilha.Shared.Models;
using AnalisePanilha.Shared.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace AnalisePanilha.Shared.ViewModels
{
    public class ExcelComparisonViewModel
    {
        private readonly IExcelComparisonService _excelComparisonService;
        private readonly NavigationManager _navigationManager;

        public string FilePath1 { get; private set; }
        public string FilePath2 { get; private set; }
        public string FileName1 { get; private set; }
        public string FileName2 { get; private set; }
        public List<CellComparisonResult> ComparisonResults { get; private set; }
        public string ExportedFilePath { get; private set; }
        public bool IsComparing { get; private set; }
        public bool IsExporting { get; private set; }
        public string ErrorMessage { get; private set; }
        public List<ColumnInfo> File1Columns { get; private set; } = new List<ColumnInfo>();
        public List<ColumnInfo> File2Columns { get; private set; } = new List<ColumnInfo>();
        public ColumnPair ActiveColumnPair { get; private set; }

        public bool CanCompare => !string.IsNullOrEmpty(FilePath1) && !string.IsNullOrEmpty(FilePath2);
        public bool HasResults => ComparisonResults != null && ComparisonResults.Count > 0;

        public event Action OnStateChanged;

        public ExcelComparisonViewModel(IExcelComparisonService excelComparisonService, NavigationManager navigationManager)
        {
            _excelComparisonService = excelComparisonService;
            _navigationManager = navigationManager;
            ComparisonResults = new List<CellComparisonResult>();
        }

        public async Task HandleFileSelection(InputFileChangeEventArgs e, int fileNumber)
        {
            try
            {
                const long maxFileSize = 100 * 1024 * 1024;

                if (e.File.Size > maxFileSize)
                {
                    ErrorMessage = $"O arquivo excede o limite de tamanho (10MB): {e.File.Name}";
                    NotifyStateChanged();
                    return;
                }

                string tempPath = Path.Combine(Path.GetTempPath(), $"excel_temp_{Guid.NewGuid()}.xlsx");

                await using (var stream = File.Create(tempPath))
                {
                    await e.File.OpenReadStream(maxFileSize).CopyToAsync(stream);
                }

                if (fileNumber == 1)
                {
                    FilePath1 = tempPath;
                    FileName1 = e.File.Name;
                }
                else
                {
                    FilePath2 = tempPath;
                    FileName2 = e.File.Name;
                }

                if (!string.IsNullOrEmpty(FilePath1) && !string.IsNullOrEmpty(FilePath2))
                {
                    await LoadColumnInfo();
                }

                ErrorMessage = null;
                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao processar o arquivo: {ex.Message}";
                NotifyStateChanged();
            }
        }

        public async Task LoadColumnInfo()
        {
            if (string.IsNullOrEmpty(FilePath1) || string.IsNullOrEmpty(FilePath2))
                return;

            try
            {
                File1Columns.Clear();
                File2Columns.Clear();

                using (var workbook = new XLWorkbook(FilePath1))
                {
                    var worksheet = workbook.Worksheet(1);
                    var headerRow = worksheet.Row(1);

                    int colIndex = 1;
                    foreach (var cell in headerRow.CellsUsed())
                    {
                        string columnName = cell.Value.ToString();
                        string columnLetter = GetExcelColumnName(cell.Address.ColumnNumber);
                        File1Columns.Add(new ColumnInfo(colIndex++, columnName, columnLetter));
                    }
                }

                using (var workbook = new XLWorkbook(FilePath2))
                {
                    var worksheet = workbook.Worksheet(1);
                    var headerRow = worksheet.Row(1);

                    int colIndex = 1;
                    foreach (var cell in headerRow.CellsUsed())
                    {
                        string columnName = cell.Value.ToString();
                        string columnLetter = GetExcelColumnName(cell.Address.ColumnNumber);
                        File2Columns.Add(new ColumnInfo(colIndex++, columnName, columnLetter));
                    }
                }

                Console.WriteLine($"Colunas carregadas: Arquivo 1: {File1Columns.Count}, Arquivo 2: {File2Columns.Count}");
                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao carregar informações das colunas: {ex.Message}";
                NotifyStateChanged();
            }
        }

        private string GetExcelColumnName(int columnNumber)
        {
            if (columnNumber <= 0)
                return string.Empty;

            string columnName = "";

            while (columnNumber > 0)
            {
                int remainder = (columnNumber - 1) % 26;
                columnName = (char)('A' + remainder) + columnName;
                columnNumber = (columnNumber - 1) / 26;
            }

            return columnName;
        }

        public void SetColumnPairForComparison(ColumnPair columnPair)
        {
            ActiveColumnPair = columnPair;
            Console.WriteLine($"Colunas selecionadas para comparação: {columnPair.File1Column} -> {columnPair.File2Column}");
            NotifyStateChanged();
        }

        public async Task CompareFiles()
        {
            if (!CanCompare) return;

            try
            {
                IsComparing = true;
                ErrorMessage = null;
                NotifyStateChanged();

                Console.WriteLine($"Iniciando comparação entre arquivos: {FileName1} e {FileName2}");

                if (ActiveColumnPair != null &&
                    !string.IsNullOrEmpty(ActiveColumnPair.File1Column) &&
                    !string.IsNullOrEmpty(ActiveColumnPair.File2Column))
                {
                    Console.WriteLine($"Comparando colunas específicas: {ActiveColumnPair.File1Column} e {ActiveColumnPair.File2Column}");
                    ComparisonResults = await _excelComparisonService.CompareExcelColumnsOnly(
                        FilePath1,
                        FilePath2,
                        ActiveColumnPair.File1Column,
                        ActiveColumnPair.File2Column);
                }
                else
                {
                    ComparisonResults = await _excelComparisonService.CompareExcelFiles(FilePath1, FilePath2);
                }

                Console.WriteLine($"Comparação concluída. Total de resultados: {ComparisonResults.Count}");
                Console.WriteLine($"Diferenças encontradas: {ComparisonResults.Count(r => r.IsDifferent)}");

                IsComparing = false;
                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO NA COMPARAÇÃO: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                ErrorMessage = $"Erro ao comparar arquivos: {ex.Message}";
                IsComparing = false;
                NotifyStateChanged();
            }
        }

        public async Task ExportResults()
        {
            if (!HasResults) return;

            try
            {
                IsExporting = true;
                ErrorMessage = null;
                NotifyStateChanged();

                ExportedFilePath = await _excelComparisonService.ExportComparisonResults(ComparisonResults);

                IsExporting = false;
                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao exportar resultados: {ex.Message}";
                IsExporting = false;
                NotifyStateChanged();
            }
        }

        public void ResetSelection()
        {
            FilePath1 = null;
            FilePath2 = null;
            FileName1 = null;
            FileName2 = null;
            ComparisonResults = new List<CellComparisonResult>();
            ExportedFilePath = null;
            ErrorMessage = null;
            File1Columns.Clear();
            File2Columns.Clear();
            ActiveColumnPair = null;
            NotifyStateChanged();
        }

        public void SetErrorMessage(string message)
        {
            ErrorMessage = message;
            NotifyStateChanged();
        }

        public void LoadSampleFiles(string path1, string path2)
        {
            if (File.Exists(path1) && File.Exists(path2))
            {
                FilePath1 = path1;
                FilePath2 = path2;
                FileName1 = Path.GetFileName(path1);
                FileName2 = Path.GetFileName(path2);
                ErrorMessage = null;
                LoadColumnInfo().Wait();
                NotifyStateChanged();
            }
            else
            {
                ErrorMessage = "Um ou ambos os arquivos de amostra não foram encontrados.";
                NotifyStateChanged();
            }
        }

        public async Task CompareSpecificFiles(string path1, string path2)
        {
            try
            {
                if (!File.Exists(path1))
                {
                    ErrorMessage = $"Arquivo não encontrado: {path1}";
                    NotifyStateChanged();
                    return;
                }

                if (!File.Exists(path2))
                {
                    ErrorMessage = $"Arquivo não encontrado: {path2}";
                    NotifyStateChanged();
                    return;
                }

                IsComparing = true;
                ErrorMessage = null;
                NotifyStateChanged();

                FilePath1 = path1;
                FilePath2 = path2;
                FileName1 = Path.GetFileName(path1);
                FileName2 = Path.GetFileName(path2);

                await LoadColumnInfo();

                if (ActiveColumnPair != null &&
                    !string.IsNullOrEmpty(ActiveColumnPair.File1Column) &&
                    !string.IsNullOrEmpty(ActiveColumnPair.File2Column))
                {
                    ComparisonResults = await _excelComparisonService.CompareExcelColumnsOnly(
                        path1,
                        path2,
                        ActiveColumnPair.File1Column,
                        ActiveColumnPair.File2Column);
                }
                else
                {
                    ComparisonResults = await _excelComparisonService.CompareExcelFiles(path1, path2);
                }

                IsComparing = false;
                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao comparar arquivos: {ex.Message}";
                IsComparing = false;
                NotifyStateChanged();
            }
        }

        private void NotifyStateChanged()
        {
            OnStateChanged?.Invoke();
        }
    }
}