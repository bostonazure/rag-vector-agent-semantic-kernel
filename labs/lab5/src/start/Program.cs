using Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.SqlServer;
using Microsoft.SemanticKernel.Memory;
using Plugins;
using Memory;
using System.Text;
using Filters;
using Extensions;

var builder = Host.CreateApplicationBuilder(args).AddAppSettings();

builder.Services.AddKernel().AddChatCompletionService(builder.Configuration.GetConnectionString("OpenAI"));
builder.Services.Configure<PluginOptions>(builder.Configuration.GetSection(PluginOptions.PluginConfig));

// Add filters with logging.
// TODO: 

var semanticTextMemory = new MemoryBuilder()
    .WithSqlServerMemoryStore(builder.Configuration.GetConnectionString("SqlAzureDB")!)
    .WithTextEmbeddingGeneration(builder.Configuration.GetConnectionString("OpenAI")!)
    .Build();

var memoryStore = new MemoryStore(semanticTextMemory);

builder.Services.AddSingleton(memoryStore);

var app = builder.Build();

var chatCompletionService = app.Services.GetRequiredService<IChatCompletionService>();
var kernel = app.Services.GetRequiredService<Kernel>();

kernel.ImportPluginFromPromptDirectory("Prompts");
// TODO: Import the YamlPrompts plugin
kernel.ImportPluginFromType<DateTimePlugin>();
kernel.ImportPluginFromType<QueryRewritePlugin>();
kernel.ImportPluginFromType<PdfRetrieverPlugin>();
// TODO: Import the WebRetrieverPlugin

OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
    Temperature = 0.2f,
    MaxTokens = 500
};

var assetsDir = PathUtils.FindAncestorDirectory("assets");
await memoryStore.PopulateAsync(assetsDir);

var responseTokens = new StringBuilder();
ChatHistory chatHistory = [];
while (true)
{
    // TODO: Create a function list

    Console.Write("\nQuestion: ");

    var question = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(question))
    {
        break;
    }

    // Get user's intent
    // TODO:

    // TODO: move OpenAIPromptExecutionSettings and pass functionsList

    chatHistory.AddUserMessage(question);
    responseTokens.Clear();
    await foreach (var token in chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, openAIPromptExecutionSettings, kernel))
    {
        Console.Write(token);
        responseTokens.Append(token);
    }

    chatHistory.AddAssistantMessage(responseTokens.ToString());
    Console.WriteLine();
}