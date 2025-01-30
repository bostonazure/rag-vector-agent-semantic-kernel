# LAB 6: Agent Group Chat with human in the loop
## Introduction

This exercise explains how to create an Agent Group Chat that include a human to book a trip that includes flights and a hotel. The solution involves four AI agents with specific roles that collaborate to fulfill the user's travel request and a user that should approve or reject the agent’s proposal.


<img src="./assets/humanintheloop.png" alt="Human in the loop" width="70%" height="70%">

## Learning objetives
* Understand how can you include a human in the loop using SK plugins as and agent skill.


## Create Agents that participate in the Group Chat
To include the human in the Group Chat discussion you could include in the Agent group chat and agent that use an `SK Function` to ask the user for their input.


1. Create a new file `UserInputPlugin.cs` and add the Class `UserCOnfirmationsPlugin` with the following code. 

    This code creates the SK Function `GetUserInput`. This function takes as an argument the information that you would like the user to review and provide input on. For example, it could be used for the user to confirm trip details. 

```csharp
using Microsoft.SemanticKernel;
using System.ComponentModel;

public class UserInputsProxy
{
    [KernelFunction("GetUserInput")]
    [Description("you present the information to the user and get the user input.")]
    public async Task<string> GetUserFlighandHotelConfirmationAsync(string Information)
    {
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("User Confirmation is required for the following ");
        Console.WriteLine();

        Console.WriteLine(Information);
        Console.WriteLine();
        
        Console.WriteLine("Please let me know your comments:");
        string userInput = Console.ReadLine() ?? "No comments";
        return await Task.FromResult(userInput);
    }
}
```
2. Create a new file `HumanInTheLoopUserProxy.cs` and Create a new class `TravelAgentChatHelperWithHumanProxy`.

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;


namespace AgentsSample;

public static class TravelAgentChatHelperWithHumanProxy
{
}
```

3. Add method `CreateBasicAgent`. In this method, you are creating agent instances that may or may not include plugins. When you add a plugin to an agent, a new `SK Kernel` instance is created for it so that only that specific agent has access to the plugin. This approach is used for optimization purposes. Conversely, if the agent does not have a plugin, `FunctionChoiceBehavior.None()` is employed to prevent the agent from evaluating whether a plugin should be used. 
```csharp
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
```
4. Add the method `InitializeAgents`. This method is used to create all the agents. One agent, representing the human in the agent group chat, can be named `UserProxyAgent`. This agent has instructions on how to present the trip information to the user and includes a guardrail to avoid creating any content, responding only to what the user says. The `UserProxyAgent` reads the user input using the `SK Function UserInputsProxy`.

```csharp
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
```
4. Add method `TravelAgentGroupChatSecuentialAsync`.This group chat execution is similar to the previous lab exercise. The difference is that we have added the `UserProxyAgent` to this group chat to ask for the user's approval of the `TravelAgencyAgent`'s recommendation.
```csharp
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
```
5. Before run for testing Update menu main program class with the following code.
```csharp
case "4":
    await TravelAgentChatHelperWithHumanProxy.TravelAgentGroupChatSecuentialAsync(theAgentKernel);
    break;
```
6. Run the app use option 4 `TravelAgent GroupChat Human in the loop` and  select the city option `London` as apear in the following image.

    When you input is aked (as user) please provide a positive answer to the trip propousal.

    The Chat discussion secuence should be like is decribed bellow.

* user
```
I am planning an exciting trip from Boston, Massachusetts, to Santiago, Chile. 
The journey includes a stay in a luxurious hotel with a mountain view and breakfast included, from January 10th to January 15th, accommodating two guests. 
My flight, preferably with LATAM Airlines, will depart from Boston on January 10th and return on January 15th.I have noted your preferences for the trip, including your choice for LATAM Airlines and a luxurious hotel with a mountain view and breakfast included. I will review the flight and hotel options provided by other agents and select the best options for your journey. Please hold on for a moment while the options are shared.
```

* Assistant - FlightSearchAgent
```json
Here are two available flight options for your trip from Boston to Santiago with LATAM Airlines:
{
  "flights": [
    {
      "departure": {
        "date": "2024-01-10",
        "time": "10:00 AM",
        "airline": "LATAM Airlines",
        "flightNumber": "LA820",
        "from": "Boston Logan International Airport (BOS)",
        "to": "Arturo Merino Benítez Airport (SCL)"
      },
      "return": {
        "date": "2024-01-15",
        "time": "05:00 PM",
        "airline": "LATAM Airlines",
        "flightNumber": "LA821",
        "from": "Arturo Merino Benítez Airport (SCL)",
        "to": "Boston Logan International Airport (BOS)"
      },
      "price": "$950"
    },
    {
      "departure": {
        "date": "2024-01-10",
        "time": "07:00 PM",
        "airline": "LATAM Airlines",
        "flightNumber": "LA822",
        "from": "Boston Logan International Airport (BOS)",
        "to": "Arturo Merino Benítez Airport (SCL)"
      },
      "return": {
        "date": "2024-01-15",
        "time": "11:00 AM",
        "airline": "LATAM Airlines",
        "flightNumber": "LA823",
        "from": "Arturo Merino Benítez Airport (SCL)",
        "to": "Boston Logan International Airport (BOS)"
      },
      "price": "$980"
    }
  ]
}
Please note that these options are subject to availability and change. Let me know if I can assist you with anything else!
```

* Assistant - HotelSearchAgent
```json
Here are two luxurious hotel options with mountain views and breakfast included for your stay in Santiago, Chile:

{
  "hotels": [
    {
      "name": "The Ritz-Carlton, Santiago",
      "location": "El Golf, Santiago",
      "mountainView": true,
      "breakfastIncluded": true,
      "pricePerNight": "$350",
      "totalPrice": "$1,750",
      "checkInDate": "2024-01-10",
      "checkOutDate": "2024-01-15",
      "guests": 2
    },
    {
      "name": "Hotel Santiago by Mandarin Oriental",
      "location": "Las Condes, Santiago",
      "mountainView": true,
      "breakfastIncluded": true,
      "pricePerNight": "$320",
      "totalPrice": "$1,600",
      "checkInDate": "2024-01-10",
      "checkOutDate": "2024-01-15",
      "guests": 2
    }
  ]
}

Both these options provide mountain views and include breakfast. Let me know if you need further assistance or more details!
```

* Assistant - TravelAgencyAgent
```json
{
    "Information to confirm": {
        "Flight": "LATAM Airlines flight number LA820 departing from Boston Logan International Airport (BOS) to Arturo Merino Benítez Airport (SCL) on January 10th at 10:00 AM and returning on January 15th at 5:00 PM.",
        "Hotel": "The Ritz-Carlton, Santiago located in El Golf, Santiago with mountain views and breakfast included, from January 10th to January 15th for a total price of $1,750.",
        "Reason": "The chosen flight with LATAM Airlines offers a convenient morning departure and an afternoon return, allowing you to make the most of your time in Santiago. The Ritz-Carlton is selected for its luxurious reputation, prime location, excellent mountain views, and included breakfast, ensuring a comfortable and memorable stay."
    }
}
```
Now the user conformation is required and the `Assistant - UserProxyAgent` is asking te user for thier input. Using the `trace` debug information you could read when the agent call the `SK Funtion` to get the user input.
```json
Function UserInputsProxy-GetUserImput arguments: {"Information":"\u2022 LATAM Airlines flight number LA820 departing from Boston Logan International Airport (BOS) to Arturo Merino Ben\u00EDtez Airport (SCL) on January 10th at 10:00 AM and returning on January 15th at 5:00 PM.\n\u2022 The Ritz-Carlton, Santiago located in El Golf, Santiago with mountain views and breakfast included, from January 10th to January 15th for a total price of $1,750.\n\u2022 The chosen flight with LATAM Airlines offers a convenient morning departure and an afternoon return, allowing you to make the most of your time in Santiago. The Ritz-Carlton is selected for its luxurious reputation, prime location, excellent mountain views, and included breakfast, ensuring a comfortable and memorable stay.\nPlease confirm if the above details are correct or if there are any changes needed."}
```

The user see in the console interface the following prompt
```
User Confirmation is required for the following
LATAM Airlines flight number LA820 departing from Boston Logan International Airport (BOS) to Arturo Merino Benítez Airport (SCL) on January 10th at 10:00 AM and returning on January 15th at 5:00 PM. The Ritz-Carlton, Santiago located in El Golf, Santiago with mountain views and breakfast included, from January 10th to January 15th for a total price of $1,750.
The chosen flight with LATAM Airlines offers a convenient morning departure and an afternoon return, allowing you to make the most of your time in Santiago. The Ritz-Carlton is selected for its luxurious reputation, prime location, excellent mountain views, and included breakfast, ensuring a comfortable and memorable stay.
Please confirm if the above details are correct or if there are any changes needed.

Please let me know your comments:
```

Here is when you have to input a conformation. The `UserProxyAgen` will repeat in the group chat dialog the user response, something like:
```
This propousal works for me please book it.
```

* Assistant - UserProxyAgen
```
This propousal works for me please book it.
```

* Assistant - BookingAgent
```
Thanks for the confirmation! Your flight and hotel are booked. Your reservation number is 4BZ29A5J7. <BookingLoopEnd>
```

---
[Back to Lab 6 index.](./README.md)

This is sample code for education propouse, not intented to be used in production.