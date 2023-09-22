using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace ToDo.SemanticKernel.Skills
{
    public class SaveToDoSkill
    {
        private readonly IToDoService toDoService;

        public SaveToDoSkill(IToDoService toDoService)
        {
            this.toDoService = toDoService;
        }

        private const string ToDoJsonParamName = "ToDoJson";

        [SKFunction, Description("Saves ToDo item and returs their names.")]
        [SKParameter(ToDoJsonParamName, "JSON representation of ToDo item")]
        public string SaveToDo(SKContext context)
        {
            var todoJson = context.Variables[ToDoJsonParamName];
            var toDoItems = JsonSerializer.Deserialize<ICollection<ToDoItem>>(todoJson) ?? Array.Empty<ToDoItem>();
            foreach (var item in toDoItems)
            {
                toDoService.Save(item);
            }

            return "Saved new tasks:\n" + string.Join(Environment.NewLine, toDoItems.Select(t => t.Description));
        }
    }
}