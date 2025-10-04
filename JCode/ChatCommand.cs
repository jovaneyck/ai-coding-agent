using System.ClientModel;
using System.Management.Automation;
using System.Text.Json;
using System.Text.RegularExpressions;
using OpenAI;
using OpenAI.Chat;
using Spectre.Console;
using Spectre.Console.Cli;

namespace JCode;

public partial class ChatCommand : ICommand<EmptyCommandSettings>
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

        var modelProviderUri = new Uri("<uri here>");
        var model = "<model deployment name here>";
        var key = "<key here>";
        _client = new ChatClient(
            model,
            new ApiKeyCredential(key),
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
         var response = await RunInference();
         if (response.ToolCall != null)
         {
             await HandleToolCallChainAsync();
         }
    }

    public record InferenceResponse(string? Content, ToolCall? ToolCall);
    
    private async Task<InferenceResponse> RunInference()
    {
        var options = new ChatCompletionOptions
        {
            ToolChoice = ChatToolChoice.CreateAutoChoice(),
            Tools =
            {
                ChatTool.CreateFunctionTool("get_secret", "gets the magical secret"),
                ChatTool.CreateFunctionTool("powershell", "executes a powershell script. You can use this to inspect files and directories, to create files, to change file contents and to run node scripts.",
                    BinaryData.FromBytes("""
                                         {
                                             "type": "object",
                                             "properties": {
                                                 "script": {
                                                     "type": "string",
                                                     "description": "The powershell script to execute"
                                                 }
                                             },
                                             "required": [ "script" ]
                                         }
                                         """u8.ToArray())),
            }
        };
        
        var update = (await _client.CompleteChatAsync(_conversation, options)).Value;
        UpdateStats(update.Usage);

        switch (update.FinishReason)
        {
            case ChatFinishReason.Stop:
            {
                _conversation.Add(ChatMessage.CreateAssistantMessage(update));
                
                var message = string.Join("", update.Content.Select(cu => cu.Text));
                var content = Markup.Escape(Emoji.Replace(message));
                _console.Clear();
                _console.Write(new Panel(content));
                return new InferenceResponse(message, null);
            }
            case ChatFinishReason.ToolCalls:
            {
                _conversation.Add(ChatMessage.CreateAssistantMessage(update));
                var toolCall = new ToolCall
                {
                    Name = update.ToolCalls[0].FunctionName,
                    Arguments = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(update.ToolCalls[0].FunctionArguments.ToString()) ?? new Dictionary<string, string>()
                };
                var result = await Call(toolCall);
                _conversation.Add(ChatMessage.CreateToolMessage(update.ToolCalls[0].Id, result));
                return new InferenceResponse(null, toolCall);
                
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task<string> Call(ToolCall toolCall)
    {
        return toolCall.Name switch
        {
            "get_secret" => "The secret is 'Key lime pie'.",
            "powershell" => await RunPowershell(toolCall.Arguments),
            _ => throw new ArgumentOutOfRangeException($"Unkown tool: {toolCall.Name}")
        };
    }

    private async Task<string> RunPowershell(Dictionary<string, string> toolCallInput)
    {
        var script = toolCallInput["script"];
        using var ps = PowerShell.Create();
        ps.AddScript(script);
        var result = await ps.InvokeAsync();
        return "Powershell script executed. Output: "+string.Join(Environment.NewLine, result.Select(r => r?.ToString()));
    }

    private ToolCall? ParseToolCall(string message)
    {
        //Model api's typically return "I'm doing a tool call here" but ollama+qwen coder aren't (yet)
        //so we have to dig a bit deeper than usual to recognize tool calls
        try
        {
            var sanitized = JsonBlobRegex().Replace(message, "$1").Trim();
            return JsonSerializer.Deserialize<ToolCall>(sanitized, _jsonSerializerOptions);
        }
        catch (JsonException e)
        {
            return null;
        }
    }

    private void UpdateStats(ChatTokenUsage update)
    {
        if (update != null)
            _stats = new Stats(
                _stats.InputTokenCount + update.InputTokenCount,
                _stats.OutputTokenCount + update.OutputTokenCount);
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

    [GeneratedRegex(@".*```(?:json)?\s*(.*?)\s*```", RegexOptions.Singleline)]
    private static partial Regex JsonBlobRegex();
}

public class ToolCall
{
    public required string Name { get; set; }
    public required Dictionary<string, string> Arguments { get; set; }
}