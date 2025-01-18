using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Plugins;
public class DateTimePlugin
{
    [KernelFunction, Description("Get the local time zone name")]
    public string TimeZone()
    {
        return TimeZoneInfo.Local.DisplayName;
    }

    [KernelFunction, Description("Get the current date and time")]
    public string DateWithTime()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
