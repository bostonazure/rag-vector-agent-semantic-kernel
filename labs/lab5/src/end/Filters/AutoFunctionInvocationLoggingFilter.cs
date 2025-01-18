// From https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/Concepts/Filtering/TelemetryWithFilters.cs#L245
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Text.Json;

namespace Filters;

public class AutoFunctionInvocationLoggingFilter : IAutoFunctionInvocationFilter
{
    private readonly ILogger logger;
    public AutoFunctionInvocationLoggingFilter(ILoggerFactory loggerFactory)
    {
        logger = loggerFactory.CreateLogger(nameof(AutoFunctionInvocationLoggingFilter));
    }

    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {
        
        logger.LogWarning("ChatHistory: {ChatHistory}", JsonSerializer.Serialize(context.ChatHistory));
        logger.LogWarning("Function count: {FunctionCount}", context.FunctionCount);
        
        var functionCalls = FunctionCallContent.GetFunctionCalls(context.ChatHistory.Last()).ToList();

        
        functionCalls.ForEach(functionCall
            => logger.LogWarning(
                "Function call requests: {PluginName}-{FunctionName}({Arguments})",
                functionCall.PluginName,
                functionCall.FunctionName,
                JsonSerializer.Serialize(functionCall.Arguments)));
        

        await next(context);
    }
}
