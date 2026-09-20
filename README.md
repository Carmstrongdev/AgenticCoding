# AI Incident Analysis Agent

## Overview

This project demonstrates how to build a simple **AI-powered IT incident analysis agent in C#** using Microsoft's AI agent framework and a Gemini language model.

The application simulates an IT support environment where an AI agent receives a ticket and must retrieve the actual incident information through a C# function before analyzing the problem.

The primary goal of the project is to demonstrate **AI tool/function calling**, structured incident data, and guardrails that prevent the AI from inventing information.

---

## What This Project Demonstrates

* C# application development
* AI agent integration
* Gemini 2.5 Flash integration
* Function/tool calling
* Structured incident records
* AI instructions and guardrails
* Async AI requests
* Exception handling
* Separation between AI reasoning and authoritative application data

---

## How It Works

The application follows this basic workflow:

```text
User / IT Ticket
       │
       ▼
   AI Agent
       │
       │ Needs incident information
       ▼
  GetIncident()
       │
       ▼
 IncidentRecord
       │
       ▼
   AI Analysis
       │
       ▼
 Incident Response
```

The user provides an incident description:

```text
Analyze ticket incident INC-1042. There appears to be some sort of
MFA, Phishing, social engineering, or cloud issue.
Start with the cloud if you aren't sure.
```

The agent then has access to the `GetIncident()` function, which provides the actual incident data.

---

## Incident Data

For this demonstration, incident `INC-1042` is simulated rather than retrieved from a real ticketing system.

The `GetIncident()` function creates an `IncidentRecord` containing:

```text
User: alex.rivera
Status: Open
Phishing Suspected: true
MFA Issue Suspected: false
Cloud Misconfigurations: false
```

The function acts as the application's source of truth.

This is important because the AI is explicitly instructed not to invent incident flags.

---

## `IncidentRecord`

The `IncidentRecord` class represents the structured information associated with an IT incident.

```csharp
class IncidentRecord
{
    public string User { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool PhishingSuspected { get; set; } = false;
    public bool MFAIssueSuspected { get; set; } = false;
    public bool CloudMisconfigurations { get; set; } = false;
}
```

The class contains:

* `User` — the user associated with the incident
* `Status` — the current incident status
* `PhishingSuspected` — whether phishing is suspected
* `MFAIssueSuspected` — whether an MFA problem is suspected
* `CloudMisconfigurations` — whether a cloud configuration issue is suspected

This provides the AI with structured data rather than requiring it to interpret unstructured text.

---

## The `GetIncident()` Function

The `GetIncident()` method simulates an IT service management system.

```csharp
IncidentRecord GetIncident(string incidentID)
```

The function accepts an incident ID and returns the corresponding `IncidentRecord`.

For `INC-1042`, the application creates the incident and populates its fields:

```csharp
incidentIdentified.User = "alex.rivera";
incidentIdentified.Status = "Open";
incidentIdentified.PhishingSuspected = true;
incidentIdentified.MFAIssueSuspected = false;
incidentIdentified.CloudMisconfigurations = false;
```

If an unknown incident ID is supplied, the application returns a default record:

```csharp
return new IncidentRecord
{
    User = "unknown",
    Status = "Unknown"
};
```

In a production implementation, this function could instead query an ITSM platform, database, REST API, or ticketing system.

---

## Connecting the AI Agent

The application creates a chat client and connects it to Gemini 2.5 Flash:

```csharp
IChatClient chatClient =
    new ChatClient(vertexAI: false, apiKey: apiKey)
        .AsIChatClient("gemini-2.5-flash");
```

The important concept here is that the language model is not operating by itself. The application provides it with access to a C# function that it can use when it needs incident information.

---

## AI Agent Instructions

The agent is configured with explicit instructions:

```text
You are an IT support agent.
If you need incident facts, you MUST call GetIncident.
Do not invent flags.
Do not contradict the tool.
```

These instructions establish a basic guardrail around the AI's behavior.

The application then exposes `GetIncident()` as an AI-accessible tool:

```csharp
tools: [AIFunctionFactory.Create(GetIncident)]
```

This converts the C# method into a function that the AI agent can call.

---

## Why Tool Calling Matters

A language model may be capable of generating a plausible answer even when it does not have the underlying data.

For an IT support system, that can be dangerous.

For example, the ticket might mention MFA, phishing, and cloud infrastructure. Without access to authoritative incident data, an AI could potentially assume that one of those issues is confirmed.

This project takes a different approach:

```text
AI
 │
 ├── Needs incident facts
 │
 ▼
GetIncident()
 │
 ▼
Authoritative application data
 │
 ▼
AI analyzes the returned information
```

The AI is therefore given a mechanism to retrieve facts instead of relying entirely on generated assumptions.

---

## Printing Incident Information

The `PrintIncident()` method provides a simple way to display the incident data:

```csharp
void PrintIncident(IncidentRecord incident)
{
    Console.WriteLine($"Incident User: {incident.User}");
    Console.WriteLine($"Incident Status: {incident.Status}");
    Console.WriteLine($"Phishing Suspected: {incident.PhishingSuspected}");
    Console.WriteLine($"MFA Issue Suspected: {incident.MFAIssueSuspected}");
    Console.WriteLine($"Cloud Misconfigurations: {incident.CloudMisconfigurations}");
}
```

This is also useful for debugging because it allows the developer to compare the application's actual incident data with the AI's response.

---

## Error Handling

The AI interaction is wrapped in a `try/catch` block:

```csharp
try
{
    // AI agent execution
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    PrintIncident(GetIncident("INC-1042"));
}
```

If the AI request fails, the program catches the exception and falls back to displaying the incident information directly.

This demonstrates a basic **fail-safe behavior**: even if the AI layer fails, the underlying incident information can still be retrieved and displayed.

---

## Key Design Concept

The most important architectural concept in this project is the separation between:

### Application Data

The application owns the actual incident information.

```text
IncidentRecord
```

### AI Reasoning

The AI interprets and analyzes that information.

```text
IncidentAgent
```

### Tool Interface

The C# function connects the two.

```text
GetIncident()
```

Conceptually:

```text
             ┌──────────────────┐
             │   Incident Data  │
             │                  │
             │ IncidentRecord   │
             └────────┬─────────┘
                      │
                      │
                GetIncident()
                      │
                      ▼
             ┌──────────────────┐
             │    AI Agent      │
             │                  │
             │ Gemini 2.5 Flash │
             └────────┬─────────┘
                      │
                      ▼
             Incident Analysis
```

This pattern can be expanded into a much larger enterprise application.

---

## Potential Real-World Implementation

Instead of hard-coding the incident data, `GetIncident()` could eventually interact with a real IT management system:

```text
AI Agent
   │
   ▼
GetIncident("INC-1042")
   │
   ▼
ITSM API
   │
   ▼
Incident Database
   │
   ▼
Structured IncidentRecord
   │
   ▼
AI Analysis
```

The same architecture could be used for systems such as:

* IT service management
* Security operations
* Help desk automation
* Cloud operations
* Incident response
* Infrastructure monitoring
* Internal enterprise support

---

## Technologies

* **C#**
* **.NET**
* **Microsoft.Extensions.AI**
* **Microsoft.Agents.AI**
* **Google Gemini 2.5 Flash**
* **AI Function Calling**
* **Object-oriented programming**
* **Exception handling**
* **Async programming**

---

## Project Takeaway

This project demonstrates a fundamental pattern for building reliable AI applications:

> **The AI should reason over application-provided data rather than being responsible for inventing that data.**

The C# application provides structured incident information through a callable function, while the AI agent uses that information to analyze the ticket.

This approach creates a clear boundary between **data retrieval** and **AI reasoning**, which can be extended to real enterprise systems and APIs.
