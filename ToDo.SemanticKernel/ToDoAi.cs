using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.TemplateEngine;
using System.Reflection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;
using ToDo.SemanticKernel.ReportingModels;
using ToDo.SemanticKernel.Skills;
using Microsoft.SemanticKernel.Planning.Handlebars;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Plugins.Core;

namespace ToDo.SemanticKernel
{
    public class ToDoAi(IOptions<AzureAIOptions> options, IToDoService toDoService)
    {
        #region Helper code

        //Status reporting
        public bool Running { get; private set; }
        public Plan? CurrentPlan { get; private set; }

        #endregion


        private readonly Kernel kernel = CreateKernel(options, toDoService);
        private readonly HandlebarsPlanner planner = new();

        private readonly ChatHistory history = new();

        public static Kernel CreateKernel(IOptions<AzureAIOptions> options, IToDoService toDoService)
        {
            var optionsValue = options.Value;
            var builder = Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(optionsValue.DeploymentName, optionsValue.Endpoint, optionsValue.ApiKey);

            builder.Plugins
                .AddFromType<TextPlugin>()
                .AddFromType<MathPlugin>()
                .AddFromType<DatePlugin>("date")
                .AddFromObject(new SaveToDoSkill(toDoService), "saveTodo");

            builder.Plugins
                .AddFromPromptDirectory(GetDirectoryWithPrompts("General"))
                .AddFromPromptDirectory(GetDirectoryWithPrompts("Todos"));


            builder.Services.AddLogging(c => c.AddConsole().SetMinimumLevel(LogLevel.Trace));
            return builder.Build();
        }

        public async Task<string> Ask(string prompt)
        {
            CurrentPlan = null;
            try
            {
                Running = true;
                var kernelArguments = new KernelArguments
                {
                    { "userInput", prompt },
                    { "history", string.Join("\n", history.Select(x => x.Role + ": " + x.Content)) }
                };
                var plan = await planner.CreatePlanAsync(kernel, prompt, kernelArguments);
                UpdateCurrentPlanInfo(plan);
                var result = await plan.InvokeAsync(kernel, kernelArguments);

                history.AddUserMessage(prompt);
                history.AddAssistantMessage(result);

                return result;
            }
            catch (Exception)
            {
                return "Ups that did not work :(";
            }
            finally
            {
                Running = false;
            }
        }

        #region Helper code

        private static string GetDirectoryWithPrompts(string name)
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Promts", name);
        }

        public void UpdateCurrentPlanInfo(HandlebarsPlan plan)
        {
            var steps = new List<Step>();

            var stringPlanRepresentation = plan.ToString();
            var pattern = @"{{!--(.*?)--}}";

            // Find matches
            var matches = Regex.Matches(stringPlanRepresentation, pattern, RegexOptions.Singleline);

            // Iterate over matches and print the extracted descriptions
            foreach (Match match in matches)
            {
                steps.Add(new Step { Name = match.Groups[1].Value.Trim() });
            }

            CurrentPlan = new Plan(steps);
        }

        #endregion
    }
}