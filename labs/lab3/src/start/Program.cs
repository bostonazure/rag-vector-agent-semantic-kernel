using Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Plugins;
using System.Text;

var builder = Host.CreateApplicationBuilder(args).AddAppSettings();

builder.Services.AddKernel().AddChatCompletionService(builder.Configuration.GetConnectionString("OpenAI"));
builder.Services.Configure<PluginOptions>(builder.Configuration.GetSection(PluginOptions.PluginConfig));

var app = builder.Build();

var chatCompletionService = app.Services.GetRequiredService<IChatCompletionService>();
var kernel = app.Services.GetRequiredService<Kernel>();

kernel.ImportPluginFromPromptDirectory("Prompts");
kernel.ImportPluginFromType<DateTimePlugin>();
kernel.ImportPluginFromType<QueryRewritePlugin>();
// TODO: Import web retriever plugin

OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
    Temperature = 0.2f,
    MaxTokens = 500
};

// TODO: create a loop to capture user input and call the chat completion service