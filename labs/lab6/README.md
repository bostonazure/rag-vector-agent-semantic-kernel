# Semantic Kernel Agent Lab

Welcome to the Semantic Kernel Agent Lab!

This lab's objective is to provide an introduction on how to build an agent using Semantic Kernel, add specific skills to the agent for executing actions to solve problems, and finally, create a team of agents to collaboratively solve more complex problems.

This lab contains four exercises to help you gain hands-on experience with Semantic Kernel Agents.


![Agents](./assets/Lab6.png)

At the time of this workshop, The Semantic Kernel Agent Framework is experimental, still in development and is subject to change. 
All documentation is [Semantic Kernel Agent Framework (experimental)](https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/?pivots=programming-language-csharp).

## Exercises

### Exercise 1: Introduction to Agent with Semantic Kernel
The objective of the first lab exercise is to create an agent with reasoning capabilities to solve domain-specific requests. To achieve this, the agent utilizes a Large Language Model (LLM).

For detailed instructions, please refer to the [Basic agent with SK](./EXE1_Basic_Agent.md).

### Exercise 2: Building Your First Semantic Kernel Agent with Skills
In this exercise, you will create your first Semantic Kernel agent that has a skill to get the current weather of a city by calling a public API.

In the context of Semantic Kernel, agent skills refer to the capabilities or functions that an AI agent can perform to solve specific tasks. These skills enable the agent to interact with its environment, process information, and provide meaningful outputs.

[Agent with skills](./EXE2_Agent.md)

### Exercise 3: Agent Group Chat
In Semantic Kernel, an Agent Group Chat is a framework that enables multiple AI agents to interact and collaborate within the same conversation. This setup allows agents to work together, leveraging their individual skills and capabilities to achieve a common goal.

In this exercise, you will explore build a team of 4 agent that going to work togther to solve an user trip request. 

[Group Chat agents](./EXE3_GRoupAgent.md)



### Exercise 4: Agent Group Chat with Humman in the loop

The objective of this exercise is to create an agent group chat that collaborates to address the user's travel requirements while ensuring human oversight for final approval before booking the trip. Human involvement can be maintained through various techniques, such as user proxy or delegation flow. In this exercise, you will implement user proxy.

[Group Chat agents and human in the loop](./EXE4_GourpAgentwithHuman.md)


Happy coding!

## Lab Prerequisites

* All the exercises are built in [Visual Studio Code](https://code.visualstudio.com/download) but you could use Visual Studio as well
* [AzureOpenAI](https://learn.microsoft.com/en-us/azure/ai-services/openai/overview) LLM Endpoint and Key
* Last [.NET SDK](https://dotnet.microsoft.com/en-us/download) installed

## High-Level Summary

### Focus
Building agents with Semantic Kernel.

### Objectives
- Create an agent with reasoning capabilities to solve domain-specific requests
- Add specific skills to the agent for executing actions to solve problems
- Create a team of agents to collaboratively solve more complex problems

### Additional Exercises
- Experiment with different agent skills and their interactions

### Further Ideas
- Explore the impact of different agent collaboration strategies
