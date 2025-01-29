//dotnet new console --framework net8.0 -n lab0
// dotnet add package Microsoft.SemanticKernel ; dotnet add package Microsoft.SemanticKernel.Connectors.OpenAI ; dotnet add package Microsoft.Extensions.Hosting ; 
// dotnet build
// curl -L -o appsettings.Local.json https://bit.ly/grape06dec24

using Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

using Microsoft.Extensions.Logging;


var builder = Host.CreateApplicationBuilder(args).AddAppSettings();
// uncomment to HIDE token usage to "info" log stream: builder.Logging.AddConsole().SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Warning);
builder.Services.AddKernel().AddChatCompletionService(builder.Configuration.GetConnectionString("OpenAI"));

var app = builder.Build();
var chatCompletionService = app.Services.GetRequiredService<IChatCompletionService>();

var prompt = "tell one sentence about a famous programmer";

// Microsoft.SemanticKernel.Connectors.OpenAI
OpenAIPromptExecutionSettings settings = new()
{
    // Temperature = 0.75,
    MaxTokens = 128 // what is default?
};

var results = await chatCompletionService.GetChatMessageContentsAsync(prompt, settings);

foreach (var res in results)
{
    Console.WriteLine(res);
}
