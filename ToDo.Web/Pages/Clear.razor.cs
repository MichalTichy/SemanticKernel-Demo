using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using ToDo.SemanticKernel;
using ToDo.Web.Models;

namespace ToDo.Web.Pages
{
    public partial class Clear
    {
        [Inject]
        public IToDoService ToDoService { get; set; } = null!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        protected override Task OnInitializedAsync()
        {
            ToDoService.Clear();
            NavigationManager.NavigateTo("/");
            return Task.CompletedTask;
        }
    }
}
