using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AgentsSample;

public static class TravelAgentChatHelper
{
    /// <summary>
    ///  Create a basic agent with the given name, kernel, and instructions.
    ///  This agent has not skills, and it is used to create a chat completion agent.
    /// </summary>
    private static ChatCompletionAgent CreateBasicAgent(string agentName, Kernel agentKernel, string agentInstructions, string agentDescription)
    {
        return new ChatCompletionAgent()
        {
            // Define what and how the agent should respond.
            Instructions = agentInstructions,
            // Define the agent name.
            Name = agentName,
            // Define the kernel that the agent will use to generate responses.
            Kernel = agentKernel,
            // Define the agent description. This is used to describe the agent's role in the chat.
            Description = agentDescription
        };
    }
    /// <summary>
    /// Get the user trip request to be used in the chat.
    /// </summary>
    /// <returns></returns>
    private static string GetUserTripRequest()
    {
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("Select your trip destination:");
        Console.WriteLine("1. Paris");
        Console.WriteLine("2. Santiago");
        Console.WriteLine("3. London");
        Console.WriteLine("4. Japan");
        Console.Write("Enter your choice (1-4): ");

        string choice = Console.ReadLine() ?? string.Empty;
        return choice switch
        {
            "1" => """
                I am planning an exciting trip from Boston, Massachusetts, to Paris, France. 
                The journey includes a stay in a charming Parisian hotel with an Eiffel Tower view and breakfast included, from December 25th to December 30th, accommodating two guests. 
                My flight, preferably with Delta Airlines, will depart from Boston on December 25th and return on December 30th.
            """,
            "2" => """
                I am planning an exciting trip from Boston, Massachusetts, to Santiago, Chile. 
                The journey includes a stay in a luxurious hotel with a mountain view and breakfast included, from January 10th to January 15th, accommodating two guests. 
                My flight, preferably with LATAM Airlines, will depart from Boston on January 10th and return on January 15th.
            """,
            "3" => """
                I am planning an exciting trip from Boston, Massachusetts, to London, England. 
                The journey includes a stay in a historic hotel with a view of the Thames and breakfast included, from February 5th to February 10th, accommodating two guests. 
                My flight, preferably with British Airways, will depart from Boston, Massachusetts on February 5th and return on February 10th.
            """,
            "4" => """
                I am planning an exciting trip from Boston, Massachusetts, to Tokyo, Japan. 
                The journey includes a stay in a modern hotel with a view of Tokyo Tower and breakfast included, from March 20th to March 25th, accommodating two guests. 
                My flight, preferably with Japan Airlines, will depart from Boston, Massachusetts on March 20th and return on March 25th.
            """,
            _ => """
                I am planning an exciting trip from Boston, Massachusetts, to Paris, France. 
                The journey includes a stay in a charming Parisian hotel with an Eiffel Tower view and breakfast included, from December 25th to December 30th, accommodating two guests. 
                My flight, preferably with Delta Airlines, will depart from Boston on December 25th and return on December 30th.
            """
        };
    }

    /// <summary>
    /// Check if the final message contains the termination key to determine if the chat should end.
    /// </summary>
    private sealed class ApprovalTerminationStrategy(string terminationKey) : TerminationStrategy
    {
        // Terminate when the final message contains the term including the termination key.
        protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
            => Task.FromResult(history[history.Count - 1].Content?.Contains(terminationKey, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    /// <summary>
    /// Create and Agent Chat Group  to solve the travel booking  problem propoused by the user in the chat.
    /// </summary>
    public static async Task TravelAgentGroupChatSecuential(Kernel kernel)
    {
        //1. Define the terminaiton key of the chat discussion.
        string terminationKey = "Trip booked";

        //2. Define the instructions for each agent.
        // Instructions define what each agent can do and how they should respond.
        // Instructions should be clear and concise to guide the agent in the right direction.
        string HotelSearchAgentInstructions = """
            You are a Hotel Reservation Agent, and your goal is to help to find the best hotel for a trip.
            You will answer with a list of available hotels and their prices, limit your options to 3 maximum options.
            Do not book the hotel; only provide the available options.
        """;


        string FlightSearchAssistantInstructions = """
            You are a Flight Reservation Agent, and your goal is to help to find the best flight for a trip.
            You will answer with a list of available flights and their prices, limit your options to 3 maximum options.
            Do not book the flight; only provide the available options.
        """;

        string TravelAgencyAgentInstructions = """
            
            Do not provide flight or hotel reservation options, but you will review the options provided by the other agents.

            You are a Travel Agency Agent, and your goal is to ASK for fligh and hotel options and to select the best travel options for the user's trip based in the user request
            and the flight and hotel options provided by other agents.

            You should ONLY select the best flight and hotel options provide by other agents, Do not create any flight or Hotel options.

            You should select the best flight and hotel options for the user based on the user request. PRovide the reason why you selected the options.
            After select and justify the options, you should approve the request by just responding "approve and ready to book"

        """;

        string BookingAgentInstructions = $$$"""
            You are a Booking Agent, and your goal is to book hotel and flight when 'Approve and ready to book!' is received.
            you recieve the approved  flight information and approved hotel information and book the flight and hotel.
            Ypu only participate when you receved appoved request from Travel Agency Agent that include the specific flight and hotel to book.
            You response '{{{terminationKey}}}'  only.
        """;

        ChatCompletionAgent HotelReservationAgent = CreateBasicAgent("HotelSearchAgent", kernel, HotelSearchAgentInstructions, "Hotel Search Assistant, not booking");
        ChatCompletionAgent TravelAgencyAgent = CreateBasicAgent("TravelAgencyAgent", kernel, TravelAgencyAgentInstructions, "Travel Agency Agent, you review flight and hotel options, select the best and command to book.");
        ChatCompletionAgent FlyReservationAgent = CreateBasicAgent("FlightSearchAgent", kernel, FlightSearchAssistantInstructions, "Flight Search Assistant, not booking");
        ChatCompletionAgent BookingAgent = CreateBasicAgent("BookingAgent", kernel, BookingAgentInstructions, "Booking Agent, you book the flight and hotel when you recieve 'Approve and ready to book!' confirmation");

        //3. Create the chat group with the agents and the termination strategy.
        // The termination strategy defines when the chat should end.
        // Define which agents participate in the chat and the maximum number of iterations.
        AgentGroupChat chat =
            new(TravelAgencyAgent, BookingAgent, FlyReservationAgent, HotelReservationAgent)
            {
                ExecutionSettings =
                    new()
                    {
                        TerminationStrategy =
                            new ApprovalTerminationStrategy(terminationKey)
                            {
                                //Only agent that can terminate the chat is the Booking agent after have confirmed the booking.
                                Agents = [BookingAgent],
                                // To avoid infinite loops, maximum iterations are set to 6.
                                MaximumIterations = 6,
                            }
                    }
            };


        //4. Get the trip request form the user
        string userTripRequest = GetUserTripRequest();

        //5. Start the chat
        //adding the user userTripRequest to the chat as first message
        chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, userTripRequest));

        Console.WriteLine($"{AuthorRole.User}: ");
        Console.WriteLine($"{userTripRequest}");

        //6. Trigger the Chat discussion where the agents iterate to solve the problem. 
        // Iterate over the chat messages and display them
        await foreach (var content in chat.InvokeAsync())
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

    }

}