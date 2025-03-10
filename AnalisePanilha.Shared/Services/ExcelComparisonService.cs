using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AnalisePanilha.Shared.Models;
using AnalisePanilha.Shared.Services.Interfaces;
using System.Runtime.InteropServices;

namespace AnalisePanilha.Shared.Services
{
    public class ExcelComparisonService : IExcelComparisonService
    {
        public async Task<List<CellComparisonResult>> CompareExcelFiles(string filePath1, string filePath2)
        {
            Console.WriteLine($"Iniciando o serviço de comparação. Arquivos: {Path.GetFileName(filePath1)} e {Path.GetFileName(filePath2)}");

            var results = new List<CellComparisonResult>();

            // Verificação de existência dos arquivos
            if (!File.Exists(filePath1))
            {
                Console.WriteLine($"ERRO: Arquivo não encontrado: {filePath1}");
                throw new FileNotFoundException($"Arquivo não encontrado: {filePath1}");
            }

            if (!File.Exists(filePath2))
            {
                Console.WriteLine($"ERRO: Arquivo não encontrado: {filePath2}");
                throw new FileNotFoundException($"Arquivo não encontrado: {filePath2}");
            }

            try
            {
                await Task.Run(() => {
                    Console.WriteLine("Abrindo workbooks...");

                    using var wb1 = new XLWorkbook(filePath1);
                    using var wb2 = new XLWorkbook(filePath2);

                    var ws1 = wb1.Worksheets.First();
                    var ws2 = wb2.Worksheets.First();

                    Console.WriteLine($"Planilhas carregadas. Planilha 1: {ws1.Name}, Planilha 2: {ws2.Name}");

                    int rowCount = Math.Max(ws1.LastRowUsed()?.RowNumber() ?? 0,
                                          ws2.LastRowUsed()?.RowNumber() ?? 0);
                    int colCount = Math.Max(ws1.LastColumnUsed()?.ColumnNumber() ?? 0,
                                           ws2.LastColumnUsed()?.ColumnNumber() ?? 0);

                    Console.WriteLine($"Dimensões: {rowCount} linhas x {colCount} colunas");

                    // Para evitar processamento excessivo, limite as células a comparar
                    int maxCellsToProcess = 10000; // Ajuste este número conforme necessário
                    int cellsProcessed = 0;

                    for (int row = 1; row <= rowCount && cellsProcessed < maxCellsToProcess; row++)
                    {
                        for (int col = 1; col <= colCount && cellsProcessed < maxCellsToProcess; col++)
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

                    Console.WriteLine($"Comparação concluída. Células processadas: {cellsProcessed}");
                    Console.WriteLine($"Total de resultados: {results.Count}");
                    Console.WriteLine($"Células diferentes: {results.Count(r => r.IsDifferent)}");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO no processamento dos workbooks: {ex.Message}");
                Console.WriteLine($"Tipo de exceção: {ex.GetType().Name}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }

            return results;
        }
        public async Task<string> ExportComparisonResults(List<CellComparisonResult> results)
        {
            string filePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                $"ComparisonResults_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");

            // Usar Task.Run para operações bloqueantes de IO
            await Task.Run(() => {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Resultados");

                // Cabeçalhos
                worksheet.Cell(1, 1).Value = "Linha";
                worksheet.Cell(1, 2).Value = "Coluna";
                worksheet.Cell(1, 3).Value = "Valor no Arquivo 1";
                worksheet.Cell(1, 4).Value = "Valor no Arquivo 2";
                worksheet.Cell(1, 5).Value = "Status";

                // Estilizar cabeçalhos
                var headerRange = worksheet.Range(1, 1, 1, 5);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Preencher dados
                int row = 2;
                foreach (var result in results)
                {
                    worksheet.Cell(row, 1).Value = result.Row;
                    worksheet.Cell(row, 2).Value = result.Column;
                    worksheet.Cell(row, 3).Value = result.Value1;
                    worksheet.Cell(row, 4).Value = result.Value2;
                    worksheet.Cell(row, 5).Value = result.IsDifferent ? "Diferente" : "Igual";

                    // Destacar as diferenças
                    if (result.IsDifferent)
                    {
                        worksheet.Cell(row, 5).Style.Fill.BackgroundColor = XLColor.LightPink;
                    }
                    else
                    {
                        worksheet.Cell(row, 5).Style.Fill.BackgroundColor = XLColor.LightGreen;
                    }

                    row++;
                }

                // Auto-ajustar colunas
                worksheet.Columns().AdjustToContents();

                // Salvar
                workbook.SaveAs(filePath);
            });

            return filePath;
        }

        public async Task<List<CellComparisonResult>> CompareExcelColumnsOnly(string filePath1, string filePath2, string column1, string column2)
        {
            var results = new List<CellComparisonResult>();

            await Task.Run(() =>
            {
                using (var workbook1 = new XLWorkbook(filePath1))
                using (var workbook2 = new XLWorkbook(filePath2))
                {
                    var worksheet1 = workbook1.Worksheet(1);
                    var worksheet2 = workbook2.Worksheet(1);

                    // Determinar o número de linhas para processar (a maior entre os dois arquivos)
                    int lastRow1 = worksheet1.LastRowUsed().RowNumber();
                    int lastRow2 = worksheet2.LastRowUsed().RowNumber();
                    int maxRows = Math.Max(lastRow1, lastRow2);

                    // Comparar cada linha nas colunas especificadas
                    for (int rowIndex = 1; rowIndex <= maxRows; rowIndex++)
                    {
                        var cell1 = worksheet1.Cell($"{column1}{rowIndex}");
                        var cell2 = worksheet2.Cell($"{column2}{rowIndex}");

                        string value1 = cell1.IsEmpty() ? "" : cell1.Value.ToString();
                        string value2 = cell2.IsEmpty() ? "" : cell2.Value.ToString();

                        bool isDifferent = value1 != value2;

                        results.Add(new CellComparisonResult
                        {
                            Row = rowIndex,
                            Column = cell1.Address.ColumnNumber, // Ou poderia ser qualquer outro identificador de coluna
                            Value1 = value1,
                            Value2 = value2,
                            IsDifferent = isDifferent
                        });
                    }
                }
            });

            return results;
        }

        private string GetCellValue(IXLWorksheet worksheet, int row, int col)
        {
            try
            {
                var cell = worksheet.Cell(row, col);
                return cell.IsEmpty() ? string.Empty : cell.GetString();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}