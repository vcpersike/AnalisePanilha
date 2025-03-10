using AnalisePanilha.Shared.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalisePanilha.Shared.Components.Organisms
{
    public partial class ComparisonResultsListBase : ComponentBase
    {
        [Parameter] public List<CellComparisonResult> Results { get; set; } = new List<CellComparisonResult>();

        protected bool ShowDifferencesOnly { get; set; } = false;

        protected List<CellComparisonResult> FilteredResults =>
            ShowDifferencesOnly
                ? Results.Where(r => r.IsDifferent).ToList()
                : Results;

        protected int DifferentCount => Results.Count(r => r.IsDifferent);

        protected override void OnParametersSet()
        {
            Console.WriteLine($"ComparisonResultsList: OnParametersSet. Resultados: {Results.Count}");
        }
    }
}
