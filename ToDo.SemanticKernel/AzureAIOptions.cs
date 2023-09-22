namespace ToDo.SemanticKernel
{
    public class AzureAIOptions
    {
        public string DeploymentName { get; set; } = null!;
        public string Endpoint { get; set; } = null!;
        public string ApiKey { get; set; } = null!;
    }
}