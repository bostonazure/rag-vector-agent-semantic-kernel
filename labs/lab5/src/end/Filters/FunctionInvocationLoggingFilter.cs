// From https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/Concepts/Filtering/TelemetryWithFilters.cs#L126

using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Filters;

public class FunctionInvocationLoggingFilter : IFunctionInvocationFilter
{
    private readonly ILogger logger;
    public FunctionInvocationLoggingFilter(ILoggerFactory loggerFactory)
    {
        logger = loggerFactory.CreateLogger(nameof(FunctionInvocationLoggingFilter));
    }
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        long startingTimestamp = Stopwatch.GetTimestamp();

        logger.LogWarning("Function {FunctionName} invoking.", context.Function.Name);

        if (context.Arguments.Count > 0)
        {
            logger.LogWarning("Function arguments: {Arguments}", JsonSerializer.Serialize(context.Arguments));
        }
        
        logger.LogWarning("Execution settings: {Settings}", JsonSerializer.Serialize(context.Arguments.ExecutionSettings));
        
        try
        {
            try
            {
                await next(context);
                Log(context);
            }
            catch
            {
                // Retry once
                await next(context);
                Log(context);
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Function failed. Error: {Message}", exception.Message);
            throw;
        }
        finally
        {
            
            TimeSpan duration = new((long)((Stopwatch.GetTimestamp() - startingTimestamp) * (10_000_000.0 / Stopwatch.Frequency)));

            // Capturing the duration in seconds as per OpenTelemetry convention for instrument units:
            // More information here: https://opentelemetry.io/docs/specs/semconv/general/metrics/#instrument-units
            logger.LogWarning("Function completed. Duration: {Duration}s", duration.TotalSeconds);
            
        }
    }

    private void Log(FunctionInvocationContext context)
    {
        logger.LogWarning("Function {FunctionName} succeeded.", context.Function.Name);

        if (context.IsStreaming)
        {
            // Overriding the result in a streaming scenario enables the filter to stream chunks 
            // back to the operation's origin without interrupting the data flow.
            var enumerable = context.Result.GetValue<IAsyncEnumerable<StreamingChatMessageContent>>();
            context.Result = new FunctionResult(context.Result, ProcessFunctionResultStreamingAsync(enumerable!));
        }
        else
        {
            ProcessFunctionResult(context.Result);
        }
    }

    private void ProcessFunctionResult(FunctionResult functionResult)
    {
        string? result = functionResult.GetValue<string>();
        object? usage = functionResult.Metadata?["Usage"];

        if (!string.IsNullOrWhiteSpace(result))
        {
            logger.LogWarning("Function result: {Result}", result);
        }

        if (usage is not null)
        {
            logger.LogWarning("Usage: {Usage}", JsonSerializer.Serialize(usage));
        }
    }

    private async IAsyncEnumerable<StreamingChatMessageContent> ProcessFunctionResultStreamingAsync(IAsyncEnumerable<StreamingChatMessageContent> data)
    {
        object? usage = null;

        var stringBuilder = new StringBuilder();

        await foreach (var item in data)
        {
            yield return item;

            if (item.Content is not null)
            {
                stringBuilder.Append(item.Content);
            }

            usage = item.Metadata?["Usage"];
        }

        var result = stringBuilder.ToString();

        if (!string.IsNullOrWhiteSpace(result))
        {
            logger.LogWarning("Function result: {Result}", result);
        }

        if (usage is not null)
        {
            logger.LogWarning("Usage: {Usage}", JsonSerializer.Serialize(usage));
        }
    }
}
