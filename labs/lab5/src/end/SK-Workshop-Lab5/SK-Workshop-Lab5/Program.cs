﻿using Configuration;
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
using SK_Workshop_Lab5.Filters;
using SK_Workshop_Lab5.Extensions;

var builder = Host.CreateApplicationBuilder(args).AddAppSettings();

builder.Services.AddKernel().AddChatCompletionService(builder.Configuration.GetConnectionString("OpenAI"));
builder.Services.Configure<PluginOptions>(builder.Configuration.GetSection(PluginOptions.PluginConfig));

// Add filters with logging.
builder.Services.AddSingleton<IFunctionInvocationFilter, FunctionInvocationLoggingFilter>();
builder.Services.AddSingleton<IPromptRenderFilter, PromptRenderLoggingFilter>();
builder.Services.AddSingleton<IAutoFunctionInvocationFilter, AutoFunctionInvocationLoggingFilter>();

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
var yamlPrompts = kernel.ImportPluginFromDirectory("YamlPrompts");
kernel.ImportPluginFromType<DateTimePlugin>();
kernel.ImportPluginFromType<QueryRewritePlugin>();
var s1Retriever = kernel.ImportPluginFromType<PdfRetrieverPlugin>();
var webRetriever = kernel.ImportPluginFromType<WebRetrieverPlugin>();

var assetsDir = PathUtils.FindAncestorDirectory("assets");
await memoryStore.PopulateAsync(assetsDir);

var responseTokens = new StringBuilder();
ChatHistory chatHistory = [];
while (true)
{
    List<KernelFunction> functionsList = new();

    Console.Write("\nQuestion: ");

    var question = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(question))
    {
        break;
    }

    // Get user's intent
    var intent = await kernel.InvokeAsync(
        yamlPrompts["UserIntent"],
        new()
        {
            { "$request", question },
            { "history",  string.Join("\n", chatHistory.Select(x => x.Role + ": " + x.Content)) }
        });

    string intentText = intent.ToString();

    if (intentText == "WebSearch")
    {
        functionsList.Add(webRetriever["Retrieve"]);
    }
    else
    {
        functionsList.Add(s1Retriever["Retrieve"]);
    }

    OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Required(functionsList),
        Temperature = 0.2f,
        MaxTokens = 500
    };


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