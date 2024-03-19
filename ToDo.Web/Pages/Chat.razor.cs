using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using ToDo.SemanticKernel;
using ToDo.SemanticKernel.ReportingModels;
using ToDo.Web.Models;

namespace ToDo.Web.Pages
{
    public partial class Chat : IDisposable
    {
        private readonly CancellationTokenSource pageCancellationTokenSource = new();

        [Inject]
        public ToDoAi Ai { get; set; } = null!;


        [Inject]
        public IToDoService ToDoService { get; set; } = null!;

        public ICollection<ToDoItem> ToDoItems { get; set; } = new List<ToDoItem>();

        public Plan? Plan { get; set; } = new(new List<Step>());

        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public string? CurrentMessage { get; set; }

        private void SendMessage()
        {
            //Validation
            if (string.IsNullOrWhiteSpace(CurrentMessage))
            {
                return;
            }


            //User message handling
            Messages.Add(new Message(CurrentMessage, true, null));

            _ = Task.Run(async () =>
            {
                var message = CurrentMessage;
                CurrentMessage = null;

                await ScrollToBottomOfTheChat();

                //Response Handling
                var result = await Ai.Ask(message);
                Messages.Add(new Message(result, false, Plan));

                await ScrollToBottomOfTheChat();
            });
        }

        private Task CompleteTodo(ToDoItem todo)
        {
            ToDoService.Complete(todo.Id);
            LoadTodoItems();

            return Task.CompletedTask;
        }

        #region Infrastructure

        [Inject]
        private IJSRuntime JsRuntime { get; set; } = null!;

        private async Task ScrollToBottomOfTheChat()
        {
            await JsRuntime.InvokeVoidAsync("scrollToBottom");
        }


        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                var cancellationToken = pageCancellationTokenSource.Token;
                _ = Task.Run(async () =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        LoadTodoItems();
                        Plan = Ai.CurrentPlan;

                        await InvokeAsync(StateHasChanged);
                        await Task.Delay(100, cancellationToken);
                    }
                }, cancellationToken);
            }

            base.OnAfterRender(firstRender);
        }

        private void LoadTodoItems()
        {
            ToDoItems = ToDoService.GetAll();
        }

        public void Dispose()
        {
            pageCancellationTokenSource.Cancel();
            pageCancellationTokenSource.Dispose();
        }

        #endregion
    }
}