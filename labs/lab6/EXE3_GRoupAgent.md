# LAB 6: Agent Group Chat
## Introduction

This exercise explains how to create an Agent Group Chat to book a trip that includes flights and a hotel. The solution involves four AI agents with specific roles that collaborate to fulfill the user's travel request.

This first Agent group chat will respond to user requests autonomously.



<img src="./assets/aiAgentChat.png" alt="ity Peot and Weather Agent" width="70%" height="70%">

## Learning objetives
* Undetstand how Semantic Kernel `AgentGroupChat` works
* Use **Agent Group Chat** to coordinate collboration between AI agents
* Use Agent instructions to define AI agent's roles and  responsabilities

## Create Agents that participate in the Group Chat
1. Task: Create a new file call AgentGroupChat.cs
    
    Add this starting code

    ```csharp
    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.Agents;
    using Microsoft.SemanticKernel.Agents.Chat;
    using Microsoft.SemanticKernel.ChatCompletion;

    namespace AgentsSample;

    public static class TravelAgentChatHelper 
    {
    }       
    ```

2. Task: Create a method to create basic agents named **CreateBasicAgent**

    `CreateBasicAgent` is a agent builder (without skills) to simplify the creation of multiples agents.

    Add this mehod to `TravelAgentChatHelper ` class.
    ```csharp
        /// <summary>
        ///  Create a basic agent with the given name, kernel, and instructions.
        ///  This agent has not skills, and it is used to create a chat completion agent.
        /// </summary>
        private static ChatCompletionAgent CreateBasicAgent (string agentName, Kernel agentKernel, string agentInstructions, string agentDescription)
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
    ```

4. Task: Get the user trip request to be used in the chat.

    The `GetUserTripRequest` method is a utility method to select between multiples travels user option.

    This method provide a trip description to be use as a request (challenge)  to the Ai agent group chat.
    
    Add this mehod to `TravelAgentChatHelper ` class.

    ```csharp
        /// <summary>
        /// Get the user trip request to be used in the chat.
        /// </summary>
        /// <returns></returns>
        public static string GetUserTripRequest()
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
    ```

5. Task: Create a new class `ApprovalTerminationStrategy`to review the termination condition based on **Termination key** has been expresed by the agent

    The `ApprovalTerminationStrategy` class in a Semantic Kernel agent is designed to implement a specific strategy for terminating a process based on approval criteria.

    In this case, the chat discussion going to end when the specific `terminationKey` is mentioned by the AI agent.

    This class is a member of ``class TravelAgentChatHelper`` so should be added in the TravelAgentChatHelper class.
    ```csharp
        /// <summary>
        /// Check if the final message contains the termination key to determine if the chat should end.
        /// </summary>
        public sealed class ApprovalTerminationStrategy(string terminationKey) : TerminationStrategy
        {
            // Terminate when the final message contains the term including the termination key.
            protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
                => Task.FromResult(history[history.Count - 1].Content?.Contains(terminationKey, StringComparison.OrdinalIgnoreCase) ?? false);
        }
    ```
6. Task: Add the method `TravelAgentGroupChatSecuential`

    this method create:
    * ** 4 Agents**: Each agent has different role and responsability
    
        TravelAgencyAgent: request, evaluate and select the travel  options provided by the other agentes.
        
        BookingAgent: This agent only can book flights and hotels when it is commaned to do it.

        HotelSearchAgent: This agent only could search for hotel options.
        
        FlightSearchAgent: this agent only can search for fly options.

    * **AgentGroupChat**: created as sequencial chat defined by the order on how the agent was created.
    The code `ApprovalTerminationStrategy(terminationKey)` in a Semantic Kernel agent creates an instance of the ApprovalTerminationStrategy class, which defines a specific termination strategy based on a given key. This approach ensures that the agent's process only terminates when the specified approval condition is met, making the termination logic modular and adaptable.

    * **UserInput** is the user's trip request (challenge to solve) 
    
    * **chat.InvokeAsync()** Trigger the Chat discussion where the agents iterate to solve the problem. 


    Add this method in the class `TravelAgentChatHelper`.
    ```csharp
    /// <summary>
        /// Create and Agent Chat Group  to solve the travel booking  problem propoused by the user in the chat.
        /// </summary>
        public static async Task TravelAgentGroupChatSecuential(Kernel kernel)
        {
            //1. Define the terminaiton key of the chat discussion.
            string terminationKey="Trip booked";

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
                new(TravelAgencyAgent,BookingAgent, FlyReservationAgent, HotelReservationAgent)
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
            string  userTripRequest =  GetUserTripRequest();

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

    ```
7. Task: Update the **Main** method to include in the user options this Groupchat in the user menu.

```csharp
    case "3":
        await TravelAgentChatHelper.TravelAgentGroupChatSecuential(theAgentKernel);
        break;
```
## Run and test the Group Agent
8. Task: Run the console application and test

    8.1  Select the option **TravelAgent GroupChat Sequential autonumous**

    8.2 Select your trip destination option, for exmple **Santiago**.

    8.3 The Chat discussion secuence should be like is decribed bellow. 
    
        User trigger the chat with his trip request, TravelAgencyAgent ask for flight and Hotel options, BookingAgent skip to participate becouse it did not recieved a booking request, FlightSearchAgent provide options, HotelSearchAgent provide  hotel options, TravelAgencyAgent review, select and ask to book and finally BookingAgent "Book and finish" the dicussion.

    This is a execution outcome example:

    * TravelAgencyAgent
    ```
    ## Assistant - TravelAgencyAgent
    I have noted your preferences for the trip, including your choice for LATAM Airlines and a luxurious hotel with a mountain view and breakfast included. I will review the flight and hotel options provided by other agents and select the best options for your journey. Please hold on for a moment while the options are shared.
    ```
    *  Assistant - BookingAgent
    ```
    Could you confirm which hotel you would like to stay at and the specific LATAM flight details you would prefer, so I can finalize the booking for you once it is approved?
    ```
    * Assistant - FlightSearchAgent
    ```
    ## Assistant - FlightSearchAgent ##
    Here are some flight options for your trip from Boston to Santiago with LATAM Airlines:

    1. **LATAM Airlines Flight 815**
    - **Departure:** January 10, 2024, at 11:05 AM from Boston Logan International Airport
    - **Arrival:** January 11, 2024, at 6:50 AM in Santiago International Airport
    - **Return Flight:**
        - **Departure:** January 15, 2024, at 10:25 PM from Santiago International Airport
        - **Arrival:** January 16, 2024, at 9:10 AM in Boston Logan International Airport
    - **Price:** $875 per person (round trip)

    2. **LATAM Airlines Flight 819**
    - **Departure:** January 10, 2024, at 8:20 PM from Boston Logan International Airport
    - **Arrival:** January 11, 2024, at 5:30 AM in Santiago International Airport
    - **Return Flight:**
        - **Departure:** January 15, 2024, at 11:55 PM from Santiago International Airport
        - **Arrival:** January 16, 2024, at 10:35 AM in Boston Logan International Airport
    - **Price:** $920 per person (round trip)

    3. **LATAM Airlines Flight 823**
    - **Departure:** January 10, 2024, at 5:50 PM from Boston Logan International Airport
    - **Arrival:** January 11, 2024, at 2:45 AM in Santiago International Airport
    - **Return Flight:**
        - **Departure:** January 15, 2024, at 9:45 PM from Santiago International Airport
        - **Arrival:** January 16, 2024, at 8:25 AM in Boston Logan International Airport
    - **Price:** $940 per person (round trip)

    Please let me know which option works best for you!
    ```
    * Assistant - HotelSearchAgent
    ```
    ## Assistant - HotelSearchAgent ##
    Here are three luxury hotel options in Santiago, Chile, that include mountain views and breakfast for your stay from January 10th to January 15th:

    1. **Mandarin Oriental, Santiago**
    - **Location:** Avenida Presidente Kennedy 4601, Las Condes, Santiago
    - **Details:** Offers stunning views of the Andes Mountain range. The hotel features elegant rooms, a renowned spa, and fine dining options. Breakfast included.
    - **Price:** $360 per night

    2. **The Ritz-Carlton, Santiago**
    - **Location:** El Alcalde 15, Las Condes, Santiago
    - **Details:** Situated in the prestigious neighborhood of El Golf, offering picturesque mountain views and luxurious amenities such as an indoor pool and multiple dining venues. Breakfast included.
    - **Price:** $400 per night

    3. **Hotel Santiago by NH Collection**
    - **Location:** Avenida Kennedy 4601, Las Condes, Santiago
    - **Details:** Features modern rooms with views of the Andes and a range of facilities, including an outdoor lagoon pool and spa. Breakfast included.
    - **Price:** $330 per night

    Please let me know if any of these options suit your requirements or if you would like more information on any of them!
    ```
    * Assistant - TravelAgencyAgent
    ```
    ## Assistant - TravelAgencyAgent ##
    After reviewing the options, here are my recommendations based on your preferences:

    **Flight Selection:**
    - **LATAM Airlines Flight 815** seems to be the best option for you. It departs at a convenient time in the morning and arrives early enough the next day for you to start your exploration of Santiago. The return flight also allows you to have a full day in Santiago on January 15th before heading to the airport for your flight back to Boston. Additionally, this option is the most cost-effective, at $875 per person for the round trip.

    **Hotel Selection:**
    - **Mandarin Oriental, Santiago** is my recommended choice. It offers stunning views of the Andes, which aligns with your desire for a mountain view, and includes breakfast. The elegant setting and renowned spa create a luxurious experience consistent with your request, all at a slightly more affordable rate of $360 per night compared to some other options.

    These selections provide a balance of luxurious amenities, scenic views, and optimal scheduling for your travel days, all while being mindful of cost.

    approve and ready to book
    ```
    * Assistant - BookingAgent 
    ```
    ## Assistant - BookingAgent ##
    Trip booked.
    ```




---
[Back to Lab 6 index.](./README.md)

This is sample code for education propouse, not intented to be used in production.