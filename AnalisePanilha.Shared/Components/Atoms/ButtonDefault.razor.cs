using AnalisePanilha.Shared.Components.Atoms.Enums;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AnalisePanilha.Shared.Components.Atoms.ButtonDefault;

namespace AnalisePanilha.Shared.Components.Atoms
{
    public partial class ButtonDefaultBase : ComponentBase
    {
        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public EventCallback OnClick { get; set; }
        [Parameter] public bool IsDisabled { get; set; }
        [Parameter] public bool IsLoading { get; set; }
        [Parameter] public ButtonType Type { get; set; } = ButtonType.Primary;

        protected string CssClass => $"btn {GetButtonTypeClass()} {(IsDisabled || IsLoading ? "disabled" : "")}";

        protected string GetButtonTypeClass() => Type switch
        {
            ButtonType.Primary => "btn-primary",
            ButtonType.Secondary => "btn-secondary",
            ButtonType.Success => "btn-success",
            ButtonType.Danger => "btn-danger",
            ButtonType.Warning => "btn-warning",
            ButtonType.Info => "btn-info",
            _ => "btn-primary"
        };
    }
}
