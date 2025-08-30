using System.ClientModel;
using System.Management.Automation;
using System.Text.Json;
using System.Text.RegularExpressions;
using OpenAI;
using OpenAI.Chat;
using Spectre.Console;
using Spectre.Console.Cli;

namespace JCode;

public class ChatCommand : ICommand<EmptyCommandSettings>
{
    private readonly ChatClient _client;
    private readonly List<ChatMessage> _conversation;
    private Stats _stats;
    private readonly IAnsiConsole _console;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ChatCommand(IAnsiConsole console)
    {
        _console = console;

        var modelProviderUri = new Uri("http://192.168.129.8:11434/v1");
        var model = "qwen2.5-coder:7b-instruct";

        _client = new ChatClient(
            model,
            new ApiKeyCredential(":unused:ollama:"),
            new OpenAIClientOptions
            {
                Endpoint = modelProviderUri
            });

        _conversation = [];
        _stats = new Stats(0, 0);

        FreshSession();
    }

    private async Task<int> ExecuteAsync()
    {
        var prompt = _console.Ask<string>(">");

        while (prompt != "exit")
        {
            if (prompt == "/clear")
                FreshSession();
            else
                await ProcessPromptAsync(prompt);

            prompt = _console.Ask<string>(">");
        }

        return 0;
    }

    private void FreshSession()
    {
        _conversation.Clear();
        _conversation.Add(ChatMessage.CreateSystemMessage(SystemPrompt()));
        _stats = new Stats(0, 0);
    }

    private string SystemPrompt()
    {
        return File.ReadAllText("prompts/system_prompt.md");
    }

    private async Task ProcessPromptAsync(string prompt)
    {
        var userChatMessage = ChatMessage.CreateUserMessage(prompt);
        _conversation.Add(userChatMessage);

        await HandleToolCallChainAsync();

        _console.Write(new Panel(new Markup(
                $"{Emoji.Known.UpArrow}: {_stats.InputTokenCount} {Emoji.Known.DownArrow}: {_stats.OutputTokenCount}"))
            { Border = BoxBorder.Rounded });
    }

    private async Task HandleToolCallChainAsync()
    {
        const int maxIterations = 100;
        var iterations = 0;

        while (iterations < maxIterations)
        {
            var message = await RunInference();
            var assistantChatMessage = ChatMessage.CreateAssistantMessage(message);
            _conversation.Add(assistantChatMessage);

            var toolCall = ParseToolCall(message);
            if (toolCall == null)
                break;

            var callId = $"{toolCall.Tool}-{Guid.NewGuid()}";
            var result = await Call(toolCall);
            _conversation.Add(ChatMessage.CreateToolMessage(callId, result));

            iterations++;
        }
    }

    private async Task<string> RunInference()
    {
        var streamingResult = _client.CompleteChatStreamingAsync(_conversation);
        var message = "";

        await foreach (var update in streamingResult)
        {
            UpdateStats(update);

            message += string.Join("", update.ContentUpdate.Select(cu => cu.Text));
            var content = Markup.Escape(Emoji.Replace(message));

            _console.Clear();
            _console.Write(new Panel(content));
        }

        return message;
    }

    private async Task<string> Call(ToolCall toolCall)
    {
        return toolCall.Tool switch
        {
            "weather_report" => "Sunny with a 30% chance of heavy rain.",
            "powershell" => await RunPowershell(toolCall.Input),
            _ => throw new ArgumentOutOfRangeException($"Unkown tool: {toolCall.Tool}")
        };
    }

    private async Task<string> RunPowershell(Dictionary<string, string> toolCallInput)
    {
        var script = toolCallInput["script"];
        using var ps = PowerShell.Create();
        ps.AddScript(script);
        var result = await ps.InvokeAsync();
        return string.Join(Environment.NewLine, result.Select(r => r?.ToString()));
    }

    private ToolCall? ParseToolCall(string message)
    {
        ToolCall? call;
        try
        {
            var sanitized = Regex.Replace(message, @".*```(?:json)?\s*(.*?)\s*```", "$1", RegexOptions.Singleline).Trim();
            call = JsonSerializer.Deserialize<ToolCall>(sanitized, _jsonSerializerOptions);
        }
        catch (JsonException e)
        {
            return null;
        }

        return call?.Action != "tool_call" ? null : call;
    }

    private void UpdateStats(StreamingChatCompletionUpdate update)
    {
        if (update.Usage != null)
            _stats = new Stats(
                _stats.InputTokenCount + update.Usage.InputTokenCount,
                _stats.OutputTokenCount + update.Usage.OutputTokenCount);
    }

    public Task<int> Execute(CommandContext context, EmptyCommandSettings settings)
    {
        return ExecuteAsync();
    }

    public ValidationResult Validate(CommandContext context, CommandSettings settings)
    {
        return ValidationResult.Success();
    }

    public Task<int> Execute(CommandContext context, CommandSettings settings)
    {
        return ExecuteAsync();
    }
}

public class ToolCall
{
    public required string Action { get; set; }
    public required string Tool { get; set; }
    public required Dictionary<string, string> Input { get; set; }
}