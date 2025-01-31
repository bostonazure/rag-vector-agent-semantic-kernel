using Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

using Microsoft.Extensions.Logging;

// --------- FIRST STEP ----------
// ASK LAB INSTRUCTOR FOR THE PASSWORD
var password = "𝒜𝒮𝒦 𝒴𝒪𝒰ℛ ℒ𝒜ℬ ℐ𝒩𝒮𝒯ℛ𝒰𝒞𝒯𝒪ℛ ℱ𝒪ℛ 𝒯ℋℰ 𝒫𝒜𝒮𝒮𝒲𝒪ℛ𝒟";

var configger = new ConfigureLabKeys(password);
configger.RandomizeDecryptDistribute();

var builder = Host.CreateApplicationBuilder(args).AddAppSettings();
// uncomment to HIDE token usage to "info" log stream: builder.Logging.AddConsole().SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Warning);
builder.Services.AddKernel().AddChatCompletionService(builder.Configuration.GetConnectionString("OpenAI"));

var app = builder.Build();
var chatCompletionService = app.Services.GetRequiredService<IChatCompletionService>();

var prompt = "In a single run-on sentence, introduce a famous programmer.";
prompt = "Out of the top 1001 famous programmers, who is the one ranked exactly in the middle?";
// prompt = "Make an internal list of the top 1001 famous progammers and computer scientists. If there are challenge because this is subjective and contextual, still go through the exercise, and if you need a tie breaker rank higher those born first. Then report in a single run-on sentence the one ranked exactly in the middle (at # 501)";
prompt = "Make an internal list of the top 1001 progammers and computer scientists. If there are challenge because this is subjective and contextual, still go through the exercise, and if you need a tie breaker rank higher those born first. Then extract the middle 51, reorganize them in alphabetical order by last name. Then report in a single run-on sentence the one ranked exactly in the middle (at # 26). Distribute N Unicode (non-ASCII) characters within the sentence, where N is number of characters in the person's first name.";

// prompt = "Who is the president of the USA?";
// prompt = "How old is Joe Biden right now?";
// prompt = $"Today is {DateTime.Now}. How old is Joe Biden right now?";

// prompt = "How old is the president of the USA?";

// Microsoft.SemanticKernel.Connectors.OpenAI
OpenAIPromptExecutionSettings settings = new()
{
    // Temperature = 0.75,
    MaxTokens = 128 // what is default?
};

Console.WriteLine($"Prompt: 《{prompt}》\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
var results = await chatCompletionService.GetChatMessageContentsAsync(prompt, settings);

foreach (var res in results)
{
    Console.WriteLine(res);
}
