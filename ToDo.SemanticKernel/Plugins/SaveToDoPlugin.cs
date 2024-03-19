using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

namespace ToDo.SemanticKernel.Plugins
{
    public class SaveToDoPlugin(IToDoService toDoService)
    {
        [KernelFunction]
        [Description("Saves ToDo item and returs their names." +
                     "Should be called as final step when previous step outputs JSON.")]
        public string SaveToDo([Description("JSON representation of ToDo item")] string todoJson)
        {
            var toDoItems = JsonSerializer.Deserialize<ICollection<ToDoItem>>(todoJson) ?? Array.Empty<ToDoItem>();
            foreach (var item in toDoItems)
            {
                toDoService.Save(item);
            }

            return "Saved new tasks:\n" + string.Join(Environment.NewLine, toDoItems.Select(t => t.Description));
        }
    }
}