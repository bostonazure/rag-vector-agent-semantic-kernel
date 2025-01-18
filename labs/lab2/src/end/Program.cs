using Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Plugins;

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
kernel.ImportPluginFromType<DateTimePlugin>("dateTimePlugin");

var prompt1 = "What time is it one the west coast of the united states right now? My current timezone {{dateTimePlugin.timeZone}} and current date and time is {{dateTimePlugin.dateWithTime}}";

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

var promptTemplateFactory = new KernelPromptTemplateFactory();
string userMessage = await promptTemplateFactory.Create(new PromptTemplateConfig(prompt1)).RenderAsync(kernel);

Console.WriteLine("STEP 2B OUTPUT --------------------------------------------------------------------------------------");

Console.WriteLine($"USER MESSAGE: {userMessage}");

var step2BResult = await chatCompletionService.GetChatMessageContentsAsync(userMessage, openAIPromptExecutionSettings, kernel);
foreach (var content in step2BResult)
{
    Console.WriteLine($"\nRESPONSE:\n{content}");
}

#endregion Step 2

#region Step 3
//////////////////////////////////////////////////////////////////////////////////////////////
// Step 3:

var rewriter = kernel.ImportPluginFromType<QueryRewritePlugin>();

var prompt2 = "What are some things to do in Boston this weekend?";

var step3Result = await kernel.InvokeAsync(rewriter["Rewrite"], 
    new()
    {
        { "question", prompt2 }
    });

Console.WriteLine("STEP 3 OUTPUT --------------------------------------------------------------------------------------");
Console.WriteLine($"\nPROMPT: \n{prompt2}");
Console.WriteLine($"Rewritten query: {step3Result}");

#endregion Step 3

#region Step 4
//////////////////////////////////////////////////////////////////////////////////////////////
// Step 4: 

kernel.ImportPluginFromObject(
    new WebSearchEnginePlugin(new BingConnector(pluginOptions.BingApiKey)));

var prompt3 = step3Result.ToString().Trim('"');

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