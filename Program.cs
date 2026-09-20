using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.ComponentModel;
using Google.GenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;


string userPrompt = @"Analyze ticket incident INC-1042. There appears to be some sort of MFA, Phishing, social engineering,
or cloud issue. Start with the cloud if you aren't sure.";

Console.WriteLine(" [USER / TICKET] : " + userPrompt);

PrintIncident(GetIncident("INC-1042"));

IncidentRecord GetIncident(string incidentID)
{
    // Simulating fetching incident details
    if (incidentID == "INC-1042")
    {
        var incidentIdentified = new IncidentRecord();
        incidentIdentified.User = "alex.rivera";
        incidentIdentified.Status = "Open";
        incidentIdentified.PhishingSuspected = true;
        incidentIdentified.MFAIssueSuspected = false;
        incidentIdentified.CloudMisconfigurations = false;

        return incidentIdentified;
    }

    return new IncidentRecord { User = "unknown", Status = "Unknown" };
}

try
{
    IChatClient chatClient = new ChatClient(vertexAI: false, apiKey: apiKey).AsIChatClient("gemini-2.5-flash");
    AsAIAgent agent = chatClient.AsAIAgent
    (
        name: "IncidentAgent",
        instructions: "You are an IT support agent. If you need incident facts, you MUST call GetIncident. Do not invent flags. Do not contradict the tool. ",
        tools: [AIFunctionFactory.Create(GetIncident)]
    );

    AgentResponse response = await agent.RunAsync(userPrompt);
    Console.WriteLine("[IncidentAgent Response]: " + response.Text);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    PrintIncident(GetIncident("INC-1042"));
}

void PrintIncident(IncidentRecord incident)
{
    Console.WriteLine($"Incident User: {incident.User}");
    Console.WriteLine($"Incident Status: {incident.Status}");
    Console.WriteLine($"Phishing Suspected: {incident.PhishingSuspected}");
    Console.WriteLine($"MFA Issue Suspected: {incident.MFAIssueSuspected}");
    Console.WriteLine($"Cloud Misconfigurations: {incident.CloudMisconfigurations}");
}

class IncidentRecord
{
    public string User { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool PhishingSuspected { get; set; } = false;
    public bool MFAIssueSuspected { get; set; } = false;
    public bool CloudMisconfigurations { get; set; } = false;
}

