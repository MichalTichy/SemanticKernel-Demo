using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Skills.Core;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Planning.Sequential;
using Microsoft.SemanticKernel.TemplateEngine;
using System.Reflection;
using Microsoft.Extensions.Options;
using ToDo.SemanticKernel.ReportingModels;
using ToDo.SemanticKernel.Skills;

namespace ToDo.SemanticKernel
{
    public class ToDoAi
	{
        //Status reporting
        public bool Running { get; private set; }
        public CurrentPlan? CurrentPlan { get; private set; }


        private string history { get; set; } = "";

		private readonly ISequentialPlanner planner;
		private readonly IKernel kernel;

        public ToDoAi(IToDoService toDoService, IOptions<AzureAIOptions> options)
		{
            var optionsValue = options.Value;

            kernel = new KernelBuilder()
				.WithAzureChatCompletionService(
                    optionsValue.DeploymentName,
                    optionsValue.Endpoint,
                    optionsValue.ApiKey)
				.Build();

            //Add semantic skills
            kernel.ImportSemanticSkillFromDirectory(GetDirectoryWithPrompts(), "General","Todos");

            //Add native skills
			kernel.ImportSkill(new DateSkill(), "date");
			kernel.ImportSkill(new SaveToDoSkill(toDoService), "SaveTodo");
			
			planner = new SequentialPlanner(kernel);
		}


        public async Task<string> Ask(string prompt)
        {
            try
            {
                Running = true;

                var contextVariables = new ContextVariables(prompt)
                {
                    ["history"] = history
                };

                var plan = await planner.CreatePlanAsync(prompt);

                while (plan.HasNextStep)
                {
                    UpdateCurrentPlanInfo(plan); // reporting to the UI
                    plan = await plan.RunNextStepAsync(kernel, contextVariables);
                }
                var result = plan.State[plan.Outputs.First()];

                history += $"\nUser: {prompt}\nChatBot: {result}\n";
                return result;
            }
            catch (Exception)
            {
                return "Ups that did not work :(";
            }
            finally
            {
                CurrentPlan = null;
                Running = false;
            }
        }


        private static string GetDirectoryWithPrompts()
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Promts");
        }
        public void UpdateCurrentPlanInfo(Plan plan)
        {
            var steps = new List<Step>();
            for (var index = 0; index < plan.Steps.Count; index++)
            {
                var step = plan.Steps[index];
                steps.Add(new Step()
                {
                    Name = step.Name,
                    IsActive = plan.NextStepIndex == index,
                    IsDone = plan.NextStepIndex>index
                });
            }

            CurrentPlan = new CurrentPlan(steps);
        }

	}
}