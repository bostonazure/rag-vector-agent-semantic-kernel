using Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var builder = Host.CreateApplicationBuilder(args).AddAppSettings();

// TODO: Add the SemanticKernel and ChatCompletion services to the DI container

var app = builder.Build();

// TODO: Get the ChatCompletionService from the DI container

#region Step 1
//////////////////////////////////////////////////////////////////////////////////////////////
// Step 1: Ask the LLM for some historic information

var prompt1 = $"Who created the first LLM?";

OpenAIPromptExecutionSettings openAIPromptExecutionSettings;
// TODO: Initialize the OpenAIPromptExecutionSettings with the appropriate values

IReadOnlyList<ChatMessageContent> step1Result = null;// TODO: Use ChatCompletionService to call LLM

Console.WriteLine("STEP 1 OUTPUT --------------------------------------------------------------------------------------");
Console.WriteLine($"\nPROMPT: \n{prompt1}");
foreach (var content in step1Result)
{
    Console.WriteLine($"\nRESPONSE:\n{content}");
}

#endregion Step 1

#region Step 2
//////////////////////////////////////////////////////////////////////////////////////////////
// Step 2: Ask the LLM for some historic information

// TODO: Get the Kernel from the DI container
// TODO: Load the prompts from the "Prompts" directory

var topic = "large language models";

// TODO: Use the Kernel to call the LLM with the "ResearchAbstract" prompt
FunctionResult step2Result = null;

Console.WriteLine("STEP 2 OUTPUT --------------------------------------------------------------------------------------");
Console.WriteLine($"\nRESPONSE: \n\n{step2Result}");

#endregion Step 2

#region Step 3
//////////////////////////////////////////////////////////////////////////////////////////////
// Step 3: Ask the LLM for some historic information

var prompt3A = "Please rewrite the above abstract for a short social media post.";

// TODO: Use the ChatCompletionService to call the LLM with the prompt3A
IReadOnlyList<ChatMessageContent> step3AResult = null;

Console.WriteLine("STEP 3A OUTPUT --------------------------------------------------------------------------------------");
foreach (var content in step3AResult)
{
    Console.WriteLine($"\nRESPONSE:\n{content}");
}

// TODO: Create a ChatHistory object and add the messages from steps 2 and 3A

var prompt3B = "Please rewrite the above abstract for a short social media post.";

// TODO: Create a ChatMessage object for prompt3B and add it to the ChatHistory object

// TODO: Use the ChatCompletionService to call the LLM with the prompt3B
IReadOnlyList<ChatMessageContent> step3BResult = null;

Console.WriteLine("STEP 3B OUTPUT --------------------------------------------------------------------------------------");
foreach (var content in step3BResult)
{
    Console.WriteLine($"\nRESPONSE:\n{content}");
}


#endregion Step 3