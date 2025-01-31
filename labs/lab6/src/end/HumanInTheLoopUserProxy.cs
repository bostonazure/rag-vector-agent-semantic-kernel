using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;


namespace AgentsSample;

public static class TravelAgentChatHelperWithHumanProxy
{
    /// <summary>
    ///  Create a basic agent with the given name, kernel, and instructions.
    ///  This agent has not skills, and it is used to create a chat completion agent.
    /// </summary>
    private static ChatCompletionAgent CreateBasicAgent (string agentName, Kernel agentKernel, string agentInstructions, string agentDescription, object? plugin = null)
    {

        var myFunctionChoiceBehavior= FunctionChoiceBehavior.None();
        // Add the plugin to the kernel if provided
        if (plugin != null)
        {
            agentKernel=agentKernel.Clone();
            agentKernel.Plugins.AddFromObject(plugin);
            myFunctionChoiceBehavior= FunctionChoiceBehavior.Auto();
        }
 
        return new ChatCompletionAgent()
        {
            // Define what and how the agent should respond.
            Instructions = agentInstructions,
            // Define the agent name.
            Name = agentName,
            // Define the kernel that the agent will use to generate responses.
            Kernel = agentKernel,
            // Define the agent description. This is used to describe the agent's role in the chat.
            Description = agentDescription,// 1.3 Kernel to use for the agent
                 Arguments =
                    new KernelArguments(new AzureOpenAIPromptExecutionSettings() 
                    { 
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() 
                    })
        };
    }
        //// <summary>
    /// Initialize the agents for the travel booking chat.
    /// </summary>
    /// <param name="kernel">The kernel to use for the agents.</param>
    /// <returns>A dictionary of agents with their names as keys.</returns>
    public static Dictionary<string, ChatCompletionAgent> InitializeAgents(Kernel  kernel,string terminationKey)
    {
        

        //2. Define the instructions for each agent.
        // Instructions define what each agent can do and how they should respond.
        // Instructions should be clear and concise to guide the agent in the right direction.
        string HotelSearchAgentInstructions = """
            You are a Hotel Reservation Agent, and your goal is to help to find the best hotel for a trip.
            You will answer with a list of available hotels and their prices, limit your options to 2 maximum options.
            Do not book the hotel; only provide the available options.
            Format your answer as in json format.
        """;

        string FlightSearchAssistantInstructions = """
            You are a Flight Reservation Agent, and your goal is to help to find the best flight for a trip.
            You will answer with a list of available flights and their prices, limit your options to 2 maximum options.
            Do not book the flight; only provide the available options.
            Format your answer as in json format.
        """;

        string TravelAgencyAgentInstructions = """
            You are a Travel Agency Agent, and your goal is to help the user find the best flight and hotel options for a trip.
            you take the flight and hotel options provided by the Flight and Hotel agents and recomend 1 flight and 1 hotel to the user.
            You will explain why you choose the specific flight and hotel.

            format your answer  as:
            {   "Information to confirm": {
                "Flight": "Flight information",
                "Hotel": "Hotel information",
                "Reason": "Reason for the selection of the flight and hotel"
                }
            }
        """;

        string BookingAgentInstructions = $$$"""
            You are a Booking Agent, and your goal is to book hotel and flight after the user have confirmed.
            
            If the user's answer is positive:
            1. Generate a Random reservation number.
            2. Response "Thanks for the confirmation your flight and hotel is booked, your reservation numeber is" and add the genearted number.
            3. Added to you response '{{{terminationKey}}}'.
            
            if the user's answer is negative:
            1. Response "The trip is not booked, <TNB>" Added to you response '{{{terminationKey}}}'.
        """;

        string UserPorxyAgent="""
            you are and agent that present information to the user that cames the following format:

            {   "Information to confirm": {
                "Flight": "Flight information",
                "Hotel": "Hotel information",
                "Reason": "Reason for the selection of the flight and hotel"
                }
            }
           
            you present the information using bullet point format and ask for confirm to the user and ask for the user input using the GetUserImput function call.

            you ONLY response what the user input said, you should not add any other information in your response different from the user input.

            

        """;

        // Create a Hotel Reservation Agent
        ChatCompletionAgent HotelReservationAgent = CreateBasicAgent(
            "HotelSearchAgent", 
            kernel, 
            HotelSearchAgentInstructions, 
            "Hotel Search Assistant, not booking"
        );

        // Create a Travel Agency Agent with a User Input Plugin
        ChatCompletionAgent TravelAgencyAgent = CreateBasicAgent(
            "TravelAgencyAgent", 
            kernel, 
            TravelAgencyAgentInstructions, 
            "Travel Agency Agent, you ask other agent for Flights and Hotels options."
        );

        // Create a Flight Reservation Agent
        ChatCompletionAgent FlyReservationAgent = CreateBasicAgent(
            "FlightSearchAgent", 
            kernel, 
            FlightSearchAssistantInstructions, 
            "Flight Search Assistant, not booking"
        );

        // Create a Booking Agent
        ChatCompletionAgent BookingAgent = CreateBasicAgent(
            "BookingAgent", 
            kernel, 
            BookingAgentInstructions, 
            "Booking Agent, you book the flight and hotel when you receive confirmation for the user"
        );

        ChatCompletionAgent UserProxyAgent = CreateBasicAgent(
            "UserProxyAgent", 
            kernel, 
            UserPorxyAgent, 
            "User Proxy Agent, you collect user input and send it to the other agents",
            new UserInputsProxy()
        );

        return new Dictionary<string, ChatCompletionAgent>
        {
            { "HotelReservationAgent", HotelReservationAgent },
            { "TravelAgencyAgent", TravelAgencyAgent },
            { "FlyReservationAgent", FlyReservationAgent },
            { "BookingAgent", BookingAgent },
            { "UserProxyAgent", UserProxyAgent }
        };
    }

    /// <summary>
    /// Create and Agent Chat Group  to solve the travel booking  problem propoused by the user in the chat.
    /// </summary>
    public static async Task TravelAgentGroupChatSecuentialAsync(Kernel kernel)
    {
        //1. Define the terminaiton key of the chat discussion.
        string terminationKey="<BookingLoopEnd>";

        //2. Define the instructions for each agent.
        Dictionary<string, ChatCompletionAgent> agents = InitializeAgents(kernel,terminationKey);
       
        
       //3. Create the chat group with the agents and the termination strategy.
       // The termination strategy defines when the chat should end.
       // Define which agents participate in the chat and the maximum number of iterations.
        AgentGroupChat travelAgentGroupChat =
           // new(TravelAgencyAgent,BookingAgent, FlyReservationAgent, HotelReservationAgent)
            new(  agents["FlyReservationAgent"], agents["HotelReservationAgent"],agents["TravelAgencyAgent"],agents["UserProxyAgent"],agents["BookingAgent"])
            {
                ExecutionSettings =
                    new()
                    {
                        TerminationStrategy =
                            new TravelAgentChatHelper.ApprovalTerminationStrategy(terminationKey)
                            {
                                //Only agent that can terminate the chat is the Booking agent after have confirmed the booking.
                                Agents = [agents["BookingAgent"]],
                                // To avoid infinite loops, maximum iterations are set to 6.
                                MaximumIterations = 8
                            }
                    }
            };
        
        
        //4. Get the trip request form the user
        string  userTripRequest =  TravelAgentChatHelper.GetUserTripRequest(); 

        while (!travelAgentGroupChat.IsComplete)
        {
            await travelAgentGroupChat.ResetAsync();
            // Add the user input to the chat
            travelAgentGroupChat.AddChatMessage(new ChatMessageContent(AuthorRole.User, userTripRequest));

            Console.WriteLine($"{AuthorRole.User}: ");
            Console.WriteLine($"{userTripRequest}");

            // Iterate over the chat messages and display them
            await foreach (var content in travelAgentGroupChat.InvokeAsync())
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"## {content.Role} - {content.AuthorName ?? "*"} ##");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{content.Content}");
                Console.WriteLine();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Blue;

                
            }
            // Get the chat history of the Booking Agent
            ChatMessageContent[] BookingAgentHistory = await travelAgentGroupChat.GetChatMessagesAsync(agents["BookingAgent"]).ToArrayAsync();
            // Check if the trip is booked
            if (BookingAgentHistory.Length > 0 && BookingAgentHistory[0]?.Content?.Contains("<TNB>") == true)
            {
                Console.WriteLine("The trip is not booked,resatrt the chat to book another trip");
                await travelAgentGroupChat.ResetAsync();
                travelAgentGroupChat.IsComplete = false;
            }
        }
    }   


}