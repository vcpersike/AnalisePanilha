using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnalisePanilha.Shared.Components.Molecules
{
    public partial class ColumnSelectorBase : ComponentBase
    {
        [Parameter] public List<ColumnInfo> File1Columns { get; set; } = new List<ColumnInfo>();
        [Parameter] public List<ColumnInfo> File2Columns { get; set; } = new List<ColumnInfo>();
        [Parameter] public ColumnPair ActiveColumnPair { get; set; }
        [Parameter] public EventCallback<ColumnPair> OnApplyMappings { get; set; }

        protected string File1Column { get; set; } = "";
        protected string File2Column { get; set; } = "";
        protected string ValidationMessage { get; set; } = "";

        protected bool HasActiveSelection => ActiveColumnPair != null &&
                                          !string.IsNullOrEmpty(ActiveColumnPair.File1Column) &&
                                          !string.IsNullOrEmpty(ActiveColumnPair.File2Column);

        protected bool IsValid => !string.IsNullOrWhiteSpace(File1Column) &&
                               !string.IsNullOrWhiteSpace(File2Column) &&
                               IsValidColumnFormat(File1Column) &&
                               IsValidColumnFormat(File2Column);

        protected override void OnParametersSet()
        {
            if (HasActiveSelection && string.IsNullOrEmpty(File1Column) && string.IsNullOrEmpty(File2Column))
            {
                File1Column = ActiveColumnPair.File1Column;
                File2Column = ActiveColumnPair.File2Column;
            }
        }

        protected async Task ApplySelection()
        {
            if (!IsValid)
            {
                ValidationMessage = "Por favor, forneça valores válidos para ambas as colunas.";
                return;
            }

            ValidationMessage = "";

            // Padronizar formato das colunas (converter números para letras se necessário)
            string standardFile1Column = StandardizeColumnFormat(File1Column);
            string standardFile2Column = StandardizeColumnFormat(File2Column);

            var columnPair = new ColumnPair(standardFile1Column, standardFile2Column);
            await OnApplyMappings.InvokeAsync(columnPair);
        }

        protected void ClearSelection()
        {
            File1Column = "";
            File2Column = "";
            ValidationMessage = "";
        }

        private bool IsValidColumnFormat(string column)
        {
            if (string.IsNullOrWhiteSpace(column))
                return false;

            // Aceita formatos: letras (A-Z) ou números (1+)
            return Regex.IsMatch(column.Trim().ToUpper(), @"^[A-Z]+$") ||
                   Regex.IsMatch(column.Trim(), @"^[1-9]\d*$");
        }

        private string StandardizeColumnFormat(string column)
        {
            column = column.Trim().ToUpper();

            // Se for um número, converte para letra
            if (Regex.IsMatch(column, @"^[1-9]\d*$"))
            {
                int columnNumber = int.Parse(column);
                return ConvertNumberToColumnLetter(columnNumber);
            }

            return column;
        }

        private string ConvertNumberToColumnLetter(int columnNumber)
        {
            // Excel columns: A=1, B=2, ..., Z=26, AA=27, ...
            string columnName = "";

            while (columnNumber > 0)
            {
                int remainder = (columnNumber - 1) % 26;
                columnName = (char)('A' + remainder) + columnName;
                columnNumber = (columnNumber - 1) / 26;
            }

            return columnName;
        }
    }

    public class ColumnPair
    {
        public string File1Column { get; set; }
        public string File2Column { get; set; }

        public ColumnPair() { }

        public ColumnPair(string file1Column, string file2Column)
        {
            File1Column = file1Column;
            File2Column = file2Column;
        }
    }
}