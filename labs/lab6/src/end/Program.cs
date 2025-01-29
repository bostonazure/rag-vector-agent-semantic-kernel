using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;


using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

using Microsoft.Extensions.Configuration;

namespace AgentsSample;
class Program
{

    static Kernel CreateKernel()
    {
        var configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

        var modelId = configuration["OpenAI:ModelId"] ?? throw new InvalidOperationException("Model ID not set in secrets.");
        var endpoint = configuration["OpenAI:Endpoint"] ?? throw new InvalidOperationException("Endpoint not set in secrets.");
        var apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("API Key not set in secrets.");

        var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);
        builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));
        return builder.Build();
    }

    static async Task Main(string[] args)
    {
        //1. Create a Semantic Kernel KERNEL for the agent
        Console.WriteLine("Beginning Kernel creation!");
        Kernel myKernel = CreateKernel();
        Console.WriteLine("Kernel created!");

        //2. Clone the Kernel for the agent
        Kernel theAgentKernel = myKernel.Clone();

        //3. Select which LAB going to be executed
        Console.WriteLine("Select which lab to execute:");
        Console.WriteLine("1. Call_CityPoetAgentBasic");
        Console.WriteLine("2. Call_CityPoetAgentWithSkills");
        Console.WriteLine("3. WriterReviewGroupAgent");
        Console.WriteLine("4. TravelAgentGroupChatSequential");
        Console.WriteLine("5. TravelAgentGroupChatStrategy");

        var choice = Console.ReadLine();
        switch (choice)
        {
            case "1":
                await Call_CityPoetAgentBasic(myKernel.Clone());
                break;
            case "2":
                await Call_CityPoetAgentWithSkills(theAgentKernel);
                break;
            case "3":
                //await ProgramChatGroupAgent.WriterReviewGroupAgent(theAgentKernel);
                new NotImplementedException("This lab is not implemented yet.");
                break;
            case "4":
                await TravelAgentChatHelper.TravelAgentGroupChatSecuential(theAgentKernel);
                new NotImplementedException("This lab is not implemented yet.");
                break;
            case "5":
                //await ProgramChatGroupAgent.TravelAgentGroupChatStrategy(myKernel.Clone());
                new NotImplementedException("This lab is not implemented yet.");
                break;
            default:
                Console.WriteLine("Invalid choice. Please select a valid lab number.");
                break;
        }
    }

    /// <summary>
    /// Create a CityPoetAgent with basic functionality
    /// </summary>
    /// <param name="AgentKernel">Kernel to be used by the agent</param>
    /// <returns></returns>
    static ChatCompletionAgent CreateAgentCityPoetBasic(Kernel AgentKernel)
    {
        //1. Create a CityPoetAgent
        Console.WriteLine("Defining agent...");
        ChatCompletionAgent agent =
            new()
            {
                // 1.1 Name of the agent
                Name = "CityPoetAgent",
                // 1.2 Instructions for the agent, this is the definition of the agent behavior. Should be clear.
                // This instruction is simple, the agent only writes poems about cities.
                // It also includes the current date and time. $now is a placeholder for the current date and time.
                Instructions =
                    """
                You are an agent designed to write PoemAgentKernel based on a suject and another poet tone.
                
                Use the current date and time to provide up-to-date details or time-sensitive responses, 
                the current date and time is: {{$now}}. Include the date and time in the poem.

                You only write poems about Cities, if the subject is not a city, you will not write a poem.

                """,
                // 1.3 Kernel to use for the agent
                Kernel = AgentKernel
            };
        Console.WriteLine(" Agent created!");
        return agent;
    }

    /// <summary>
    /// Call the CityPoetAgent with basic functionality, this method will start a conversation with the agent
    /// </summary>
    /// <param name="AgentKernel"> The agent's kernel</param>
    /// <returns></returns>
    static async Task Call_CityPoetAgentBasic(Kernel AgentKernel)
    {
        //1. Create a CityPoetAgent
        ChatCompletionAgent agent = CreateAgentCityPoetBasic(AgentKernel);

        //2. Create a ChatHistory
        ChatHistory history = [];
        bool isComplete = false;

        //3. Start the conversation loop, to exit the loop, the user must not provide a subject
        do
        {
            //3.1 User inputs
            Console.WriteLine("Enter a subject for the poem, or press enter to exit.");
            Console.Write("User > ");
            string userInput = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(userInput))
            {
                //continue;
                isComplete = true;
                break;
            }

            //3.2 Add user input to the history
            history.Add(new ChatMessageContent(AuthorRole.User, userInput));
            Console.WriteLine();
            DateTime now = DateTime.Now;

            //3.3 Create arguments to send to the Agent, in this case, we are only sending the current date and time
            KernelArguments arguments =
               new()
                {
                { "now", $"{now.ToShortDateString()} {now.ToShortTimeString()}" }
                };

            //3.4 Invoke the agent
            Console.WriteLine();
            await foreach (var message in agent.InvokeStreamingAsync(history, arguments))
            {
                Console.Write(message);
            }
            Console.WriteLine();

        } while (!isComplete);

    }
    static ChatCompletionAgent CreateAgentCityPoetWithSkills(Kernel AgentKernel)
    {
        //1. Add a WeatherPlugin to the Kernel, for this agent to use
        // This plugin will provide the weather of the city at the end of the poem.
        WeatherPlugin weatherPlugin = new();
        AgentKernel.Plugins.AddFromObject(weatherPlugin);

        //2. Create a CityPoetAgent
        Console.WriteLine("Defining agent with skills...");
        ChatCompletionAgent agent =
            new()
            {
                // 1.1 Name of the agent
                Name = "CityPoetAgent",
                // 1.2 Instructions for the agent, this is the definition of the agent behavior. Should be clear.
                // This instruction is more detailed than the previous one, it includes the subject and tone of the poem.
                // It also includes the weather of the city at the end of the poem. (This is a skill)
                Instructions =
                    """
                You are an agent designed to write PoemagentKernel based on a suject and another poet tone.
                
                Use the current date and time to provide up-to-date details or time-sensitive responses.
                The current date and time is: {{$now}}. include the date and time in the poem.

                The subject you are writing about is: {{$subject}}
                You only write poems about Cities, if the subject is not a city, you will not write a poem.

                The tone you are writing in is: {{$poetName}}. 

                At the end of the Poem, add the current weather of the city as last paragraph. Use this phrase 'The current Weather is: '
                """,
                // 1.3 Kernel to use for the agent
                Kernel = AgentKernel,
                // 1.4 Arguments to pass to the agent. 
                // This agent will use the FunctionChoiceBehavior.Auto() to select the best function to use.
                Arguments =
                    new KernelArguments(new AzureOpenAIPromptExecutionSettings()
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                    })

            };
        return agent;
    }

    /// <summary>
    /// Call the CityPoetAgent with skills, this method will start a conversation with the agent
    /// </summary>
    /// <param name="AgentKernel"></param>
    static async Task Call_CityPoetAgentWithSkills(Kernel AgentKernel)
    {
        //1. Create a CityPoetAgent
        ChatCompletionAgent agent = CreateAgentCityPoetWithSkills(AgentKernel);
        //2. Create a ChatHistory
        ChatHistory history = [];
        bool isComplete = false;
        //3. Start the conversation
        do
        {
            //3.1 User inputs, specific to the agent as structure data to be pass as arguments.
            // The user will provide the subject of the poem and the poet name to apply the tone.
            Console.Write("User Subject of the poem> ");
            string inputSubject = Console.ReadLine() ?? string.Empty;
            Console.Write("User Poet name> ");
            string inputPoet = Console.ReadLine() ?? string.Empty;

            //3.1.1 Combine the user inputs in a message for the agent
            string userInput = $"Subject: {inputSubject} with tone {inputPoet}";

            //3.1.2 Check if the user wants to exit the conversation
            if (string.IsNullOrWhiteSpace(inputSubject))
            {
                //continue;
                isComplete = true;
                break;
            }

            //3.2 Add user input to the history
            history.Add(new ChatMessageContent(AuthorRole.User, userInput));

            DateTime now = DateTime.Now;

            //3.3 Create arguments to send to the Agent
            // This time we are sending the subject and the poet name to the agent.
            // The agent will use this information to write the poem.
            // This is the technical to pass specific data to the agent.
            KernelArguments arguments = new()
            {
                { "now", $"{now.ToShortDateString()} {now.ToShortTimeString()}" },
                { "subject", inputSubject },
                { "poetName", inputPoet }
            };

            Console.WriteLine();

            //3.4 Invoke the agent
            await foreach (ChatMessageContent response in agent.InvokeAsync(history, arguments))
            {
                // Display response.
                Console.WriteLine($"{response.Content}");
            }
            Console.WriteLine();


        } while (!isComplete);

    }
}