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

var builder = Host.CreateApplicationBuilder(args).AddAppSettings();

builder.Services.AddKernel().AddChatCompletionService(builder.Configuration.GetConnectionString("OpenAI"));
builder.Services.Configure<PluginOptions>(builder.Configuration.GetSection(PluginOptions.PluginConfig));

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
kernel.ImportPluginFromType<DateTimePlugin>();
kernel.ImportPluginFromType<QueryRewritePlugin>();
kernel.ImportPluginFromType<PdfRetrieverPlugin>();

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
    Console.Write("\nQuestion: ");

    var question = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(question))
    {
        break;
    }

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