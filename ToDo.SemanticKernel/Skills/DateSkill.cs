using System.ComponentModel;
using Microsoft.SemanticKernel.SkillDefinition;

namespace ToDo.SemanticKernel.Skills
{
    public class DateSkill
    {
        [SKFunction, Description("Get the current date and time in the local time zone. In format dddd dd.MM.yyyy HH:mm")]
        public string GetCurrentDate()
        {
            return DateTime.Now.ToString("dddd dd.MM.yyyy HH:mm");
        }
    }
}