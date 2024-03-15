using ToDo.SemanticKernel.ReportingModels;

namespace ToDo.Web.Models
{
    public record Message(string Text, bool IsUser, Plan? Plan);
}