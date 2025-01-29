using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Data.Common;

namespace Configuration;

internal static class ConfigurationExtensions
{
    public static HostApplicationBuilder AddAppSettings(this HostApplicationBuilder builder)
    {
        builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
        builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true);
        builder.Services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Trace));

        return builder;
    }

    public static IKernelBuilder AddChatCompletionService(this IKernelBuilder kernelBuilder, string? connectionString)
    {
        var connectionStringBuilder = new DbConnectionStringBuilder();
        connectionStringBuilder.ConnectionString = connectionString;

        var source = connectionStringBuilder.TryGetValue("Source", out var sourceValue) ? (string)sourceValue : throw new InvalidOperationException($"Connection string is missing 'Source'");

        switch (source)
        {
            case "AzureOpenAI":
                {
                    var chatDeploymentName = connectionStringBuilder.TryGetValue("ChatDeploymentName", out var deploymentValue) ? (string)deploymentValue : throw new InvalidOperationException($"Connection string is missing 'ChatDeploymentName'");
                    var endpoint = connectionStringBuilder.TryGetValue("Endpoint", out var endpointValue) ? (string)endpointValue : throw new InvalidOperationException($"Connection string is missing 'Endpoint'");
                    var key = connectionStringBuilder.TryGetValue("Key", out var keyValue) ? (string)keyValue : throw new InvalidOperationException($"Connection string is missing 'Key'");

                    kernelBuilder.AddAzureOpenAIChatCompletion(chatDeploymentName, endpoint: endpoint, apiKey: key);

                    break;
                }
            case "OpenAI":
                {
                    var chatModelId = connectionStringBuilder.TryGetValue("ChatModelId", out var chatModelIdValue) ? (string)chatModelIdValue : throw new InvalidOperationException($"Connection string is missing 'ChatModelId'");
                    var apiKey = connectionStringBuilder.TryGetValue("ApiKey", out var apiKeyValue) ? (string)apiKeyValue : throw new InvalidOperationException($"Connection string is missing 'ApiKey'");

                    kernelBuilder.AddOpenAIChatCompletion(modelId: chatModelId, apiKey: apiKey);

                    break;
                }
            default:
                throw new ArgumentException($"Invalid source: {source}");
        }
        return kernelBuilder;
    }

    internal static IKernelBuilder AddTextEmbeddingGeneration(this IKernelBuilder kernelBuilder, string? connectionString)
    {
        var connectionStringBuilder = new DbConnectionStringBuilder();
        connectionStringBuilder.ConnectionString = connectionString;

        var source = connectionStringBuilder.TryGetValue("Source", out var sourceValue) ? (string)sourceValue : throw new InvalidOperationException($"Connection string is missing 'Source'");

        switch (source)
        {
            case "AzureOpenAI":
                {
                    var textEmbeddingsDeploymentName = connectionStringBuilder.TryGetValue("TextEmbeddingsDeploymentName", out var deploymentValue) ? (string)deploymentValue : throw new InvalidOperationException($"Connection string is missing 'TextEmbeddingsDeploymentName'");
                    var endpoint = connectionStringBuilder.TryGetValue("Endpoint", out var endpointValue) ? (string)endpointValue : throw new InvalidOperationException($"Connection string is missing 'Endpoint'");
                    var key = connectionStringBuilder.TryGetValue("Key", out var keyValue) ? (string)keyValue : throw new InvalidOperationException($"Connection string is missing 'Key'");

                    kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(textEmbeddingsDeploymentName, endpoint: endpoint, apiKey: key);
                    
                    break;
                }
            case "OpenAI":
                {
                    var textEmbeddingsModelId = connectionStringBuilder.TryGetValue("TextEmbeddingsModelId", out var chatModelIdValue) ? (string)chatModelIdValue : throw new InvalidOperationException($"Connection string is missing 'TextEmbeddingsModelId'");
                    var apiKey = connectionStringBuilder.TryGetValue("ApiKey", out var apiKeyValue) ? (string)apiKeyValue : throw new InvalidOperationException($"Connection string is missing 'ApiKey'");
                    
                    kernelBuilder.AddOpenAITextEmbeddingGeneration(modelId: textEmbeddingsModelId, apiKey: apiKey);

                    break;
                }
            default:
                throw new ArgumentException($"Invalid source: {source}");
        }
        return kernelBuilder;
    }

}
