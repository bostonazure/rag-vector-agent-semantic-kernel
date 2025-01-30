# Lab 0: Can we just access the dang API?

## Learning Objectives

1. Get local copies of API keys for accessing Azure OpenAI, Azure Bing Search API, and Azure SQL DB
2. Run a simple SK console app that successfully utilizes resources hosted by Azure AI Forge

## Prerequisites

You have .NET 9.0 or better running.

### Downloading the Workshop Credentials

```console
curl -L -o appsettings.Local.json https://bit.ly/workshop-banana-2024
Invoke-WebRequest -Uri "https://bit.ly/workshop-banana-2024" -OutFile "appsettings.Local.json"
```

Listen for instructions when you get to this step!

```console
vscode .
```

```console
dotnet run
```

## High-Level Summary

### Focus
Accessing APIs and running a simple SK console app.

### Objectives
- Get local copies of API keys
- Run a simple SK console app

### Additional Exercises
- Experiment with different API endpoints

### Further Ideas
- Explore different API authentication methods
