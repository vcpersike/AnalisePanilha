using AnalisePanilha.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalisePanilha.Shared.Services.Interfaces
{
    public interface IExcelComparisonService
    {
        Task<List<CellComparisonResult>> CompareExcelFiles(string filePath1, string filePath2);
        Task<List<CellComparisonResult>> CompareExcelColumnsOnly(string filePath1, string filePath2, string column1, string column2);
        Task<string> ExportComparisonResults(List<CellComparisonResult> results);
    }

}
