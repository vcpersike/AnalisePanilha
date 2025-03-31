using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AnalisePanilha.Shared.Models;
using AnalisePanilha.Shared.Services;

namespace AnalisePanilha.Web.Services
{
    public class ExcelComparisonServiceWeb : IExcelComparisonService
    {
        public async Task<List<CellComparisonResult>> CompareExcelFiles(string filePath1, string filePath2)
        {
            var results = new List<CellComparisonResult>();

            if (!File.Exists(filePath1) || !File.Exists(filePath2))
                throw new FileNotFoundException("Um ou ambos os arquivos não foram encontrados.");

            await Task.Run(() =>
            {
                using var wb1 = new XLWorkbook(filePath1);
                using var wb2 = new XLWorkbook(filePath2);

                var ws1 = wb1.Worksheets.First();
                var ws2 = wb2.Worksheets.First();

                int rowCount = Math.Max(ws1.LastRowUsed()?.RowNumber() ?? 0, ws2.LastRowUsed()?.RowNumber() ?? 0);
                int colCount = Math.Max(ws1.LastColumnUsed()?.ColumnNumber() ?? 0, ws2.LastColumnUsed()?.ColumnNumber() ?? 0);

                int cellsProcessed = 0;
                int maxCells = 10000;

                for (int row = 1; row <= rowCount && cellsProcessed < maxCells; row++)
                {
                    for (int col = 1; col <= colCount && cellsProcessed < maxCells; col++)
                    {
                        string val1 = GetCellValue(ws1, row, col);
                        string val2 = GetCellValue(ws2, row, col);

                        if (!string.IsNullOrWhiteSpace(val1) || !string.IsNullOrWhiteSpace(val2))
                        {
                            results.Add(new CellComparisonResult
                            {
                                Row = row,
                                Column = col,
                                Value1 = val1,
                                Value2 = val2,
                                IsDifferent = val1 != val2
                            });
                        }

                        cellsProcessed++;
                    }
                }
            });

            return results;
        }

        public async Task<string> ExportComparisonResults(List<CellComparisonResult> results)
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"ComparisonResults_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");

            await Task.Run(() =>
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Resultados");

                worksheet.Cell(1, 1).Value = "Linha";
                worksheet.Cell(1, 2).Value = "Coluna";
                worksheet.Cell(1, 3).Value = "Valor 1";
                worksheet.Cell(1, 4).Value = "Valor 2";
                worksheet.Cell(1, 5).Value = "Status";

                var header = worksheet.Range(1, 1, 1, 5);
                header.Style.Font.Bold = true;
                header.Style.Fill.BackgroundColor = XLColor.LightGray;

                int row = 2;
                foreach (var result in results)
                {
                    worksheet.Cell(row, 1).Value = result.Row;
                    worksheet.Cell(row, 2).Value = result.Column;
                    worksheet.Cell(row, 3).Value = result.Value1;
                    worksheet.Cell(row, 4).Value = result.Value2;
                    worksheet.Cell(row, 5).Value = result.IsDifferent ? "Diferente" : "Igual";

                    worksheet.Cell(row, 5).Style.Fill.BackgroundColor =
                        result.IsDifferent ? XLColor.LightPink : XLColor.LightGreen;

                    row++;
                }

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(filePath);
            });

            return filePath;
        }

        public async Task<List<CellComparisonResult>> CompareExcelColumnsOnly(string filePath1, string filePath2, string column1, string column2)
        {
            var results = new List<CellComparisonResult>();

            await Task.Run(() =>
            {
                using var wb1 = new XLWorkbook(filePath1);
                using var wb2 = new XLWorkbook(filePath2);

                var ws1 = wb1.Worksheet(1);
                var ws2 = wb2.Worksheet(1);

                int rowCount = Math.Max(ws1.LastRowUsed()?.RowNumber() ?? 0, ws2.LastRowUsed()?.RowNumber() ?? 0);

                for (int row = 1; row <= rowCount; row++)
                {
                    var cell1 = ws1.Cell($"{column1}{row}");
                    var cell2 = ws2.Cell($"{column2}{row}");

                    results.Add(new CellComparisonResult
                    {
                        Row = row,
                        Column = cell1.Address.ColumnNumber,
                        Value1 = cell1?.GetValue<string>() ?? string.Empty,
                        Value2 = cell2?.GetValue<string>() ?? string.Empty,
                        IsDifferent = cell1?.GetValue<string>() != cell2?.GetValue<string>()
                    });
                }
            });

            return results;
        }

        private string GetCellValue(IXLWorksheet ws, int row, int col)
        {
            try
            {
                var cell = ws.Cell(row, col);
                return cell.IsEmpty() ? string.Empty : cell.GetString();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
