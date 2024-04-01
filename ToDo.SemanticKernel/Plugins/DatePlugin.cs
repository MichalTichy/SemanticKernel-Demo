using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace ToDo.SemanticKernel.Plugins
{
    public class DatePlugin
    {
        [KernelFunction]
        [Description("Get the current date and time in the local time zone. In format dddd dd.MM.yyyy HH:mm")]
        public string GetCurrentDate()
        {
            return DateTime.Now.ToString("dddd dd.MM.yyyy HH:mm");
        }
    }
}