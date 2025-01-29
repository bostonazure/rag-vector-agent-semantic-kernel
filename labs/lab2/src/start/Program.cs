using Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
// TODO: using Microsoft.SemanticKernel.Plugins.Web;
// TODO: using Microsoft.SemanticKernel.Plugins.Web.Bing;
// TODO: using Plugins;

var builder = Host.CreateApplicationBuilder(args).AddAppSettings();

builder.Services.AddKernel().AddChatCompletionService(builder.Configuration.GetConnectionString("OpenAI"));

var pluginOptions = new PluginOptions();
builder.Configuration.GetSection(PluginOptions.PluginConfig).Bind(pluginOptions);

var app = builder.Build();

var chatCompletionService = app.Services.GetRequiredService<IChatCompletionService>();

#region Step 2
//////////////////////////////////////////////////////////////////////////////////////////////
// Step 2: Use a custom plugin with a prompt

var kernel = app.Services.GetRequiredService<Kernel>();
// TODO: import the DateTimePlugin

var prompt1 = ""; // TODO: set prompt using plugin values

OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    Temperature = 0.7f,
    MaxTokens = 250
};

var step2AResult = await chatCompletionService.GetChatMessageContentsAsync(prompt1, openAIPromptExecutionSettings);

Console.WriteLine("STEP 2A OUTPUT --------------------------------------------------------------------------------------");
Console.WriteLine($"\nPROMPT: \n{prompt1}");
foreach (var content in step2AResult)
{
    Console.WriteLine($"\nRESPONSE:\n{content}");
}

// TODO: Create and render prompt template
string userMessage = "";

Console.WriteLine("STEP 2B OUTPUT --------------------------------------------------------------------------------------");

Console.WriteLine($"USER MESSAGE: {userMessage}");

var step2BResult = await chatCompletionService.GetChatMessageContentsAsync(userMessage, openAIPromptExecutionSettings);
foreach (var content in step2BResult)
{
    Console.WriteLine($"\nRESPONSE:\n{content}");
}

#endregion Step 2

#region Step 3
//////////////////////////////////////////////////////////////////////////////////////////////
// Step 3:

KernelPlugin rewriter = null; // TODO: Import QueryRewritePlugin

var prompt2 = "What are some things to do in Boston this weekend?";

var step3Result = ""; // TODO: Use the rewriter plugin to rewrite the prompt

Console.WriteLine("STEP 3 OUTPUT --------------------------------------------------------------------------------------");
Console.WriteLine($"\nPROMPT: \n{prompt2}");
Console.WriteLine($"Rewritten query: {step3Result}");

#endregion Step 3

#region Step 4
//////////////////////////////////////////////////////////////////////////////////////////////
// Step 4: 

// TODO: Import WebSearchEnginePlugin and BingConnector

var prompt3 = step3Result.ToString().Trim('"'); // NOTE: We need to trim any " from the string

OpenAIPromptExecutionSettings openAIPromptExecutionSettings2 = new()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
    Temperature = 0.7f,
    MaxTokens = 250
};

var step4Result = await chatCompletionService.GetChatMessageContentsAsync(prompt3, openAIPromptExecutionSettings2, kernel);

Console.WriteLine("STEP 4 OUTPUT --------------------------------------------------------------------------------------");
Console.WriteLine($"\nPROMPT: \n{prompt3}");
foreach (var content in step4Result)
{
    Console.WriteLine($"\nRESPONSE:\n{content}");
}

#endregion Step 4