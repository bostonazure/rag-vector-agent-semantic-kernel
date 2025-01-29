using Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var builder = Host.CreateApplicationBuilder(args).AddAppSettings();

builder.Services.AddKernel().AddChatCompletionService(builder.Configuration.GetConnectionString("OpenAI"));

var app = builder.Build();

var chatCompletionService = app.Services.GetRequiredService<IChatCompletionService>();

#region Step 1
//////////////////////////////////////////////////////////////////////////////////////////////
// Step 1: Ask the LLM for some historic information

var prompt1 = $"Who created the first LLM?";

OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    Temperature = 0.7f,
    MaxTokens = 250
};

var step1Result = await chatCompletionService.GetChatMessageContentsAsync(prompt1, openAIPromptExecutionSettings);

Console.WriteLine("STEP 1 OUTPUT --------------------------------------------------------------------------------------");
Console.WriteLine($"\nPROMPT: \n{prompt1}");
foreach (var content in step1Result)
{
    Console.WriteLine($"\nRESPONSE:\n{content}");
}

#endregion Step 1

#region Step 2
//////////////////////////////////////////////////////////////////////////////////////////////
// Step 2: Use a prompt template to ask the LLM for an abstract on a topic

var kernel = app.Services.GetRequiredService<Kernel>();
var prompts = kernel.CreatePluginFromPromptDirectory("Prompts");

var topic = "large language models";

FunctionResult step2Result = await kernel.InvokeAsync(
            prompts["ResearchAbstract"],
            new() {
                { "topic", topic },
            }
        );

Console.WriteLine("STEP 2 OUTPUT --------------------------------------------------------------------------------------");
Console.WriteLine($"\nRESPONSE: \n\n{step2Result}");

#endregion Step 2

#region Step 3
//////////////////////////////////////////////////////////////////////////////////////////////
// Step 3: Ask the LLM to rewrite the abstract for a social media post

var prompt3A = "Please rewrite the above abstract for a short social media post.";

var step3AResult = await chatCompletionService.GetChatMessageContentsAsync(prompt3A, openAIPromptExecutionSettings);

Console.WriteLine("STEP 3A OUTPUT --------------------------------------------------------------------------------------");
foreach (var content in step3AResult)
{
    Console.WriteLine($"\nRESPONSE:\n{content}");
}

var history = new ChatHistory();
history.AddUserMessage(step2Result.RenderedPrompt!);
history.AddAssistantMessage(step2Result.ToString());

var prompt3B = "Please rewrite the above abstract for a short social media post.";

history.AddUserMessage(prompt3B);

var step3BResult = await chatCompletionService.GetChatMessageContentsAsync(history, openAIPromptExecutionSettings);

Console.WriteLine("STEP 3B OUTPUT --------------------------------------------------------------------------------------");
foreach (var content in step3BResult)
{
    Console.WriteLine($"\nRESPONSE:\n{content}");
}


#endregion Step 3