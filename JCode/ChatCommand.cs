using System.ClientModel;
using System.Text;
using OpenAI;
using OpenAI.Chat;
using Spectre.Console;
using Spectre.Console.Cli;

namespace JCode;

public class ChatCommand : Command
{
    private readonly ChatClient _client;
    private readonly List<ChatMessage> _conversation;
    private Stats _stats;
    private readonly IAnsiConsole _console;

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
        
        _conversation = new List<ChatMessage>();
        _stats = new Stats(0, 0);
    }

    public override int Execute(CommandContext context)
    {
        return ExecuteAsync().GetAwaiter().GetResult();
    }

    private async Task<int> ExecuteAsync()
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        var prompt = _console.Ask<string>(">");

        while (prompt != "exit")
        {
            if (prompt == "/clear")
            {
                _conversation.Clear();
                _stats = new Stats(0, 0);
            }
            else
            {
                await ProcessPromptAsync(prompt);
            }
            
            prompt = _console.Ask<string>(">");
        }

        return 0;
    }

    private async Task ProcessPromptAsync(string prompt)
    {
        var userChatMessage = ChatMessage.CreateUserMessage(prompt);
        _conversation.Add(userChatMessage);

        var streamingResult = _client.CompleteChatStreamingAsync(_conversation);
        var message = "";
        
        await foreach (var update in streamingResult)
        {
            if (update.Usage != null)
                _stats = new Stats(
                    _stats.InputTokenCount + update.Usage.InputTokenCount,
                    _stats.OutputTokenCount + update.Usage.OutputTokenCount);

            message += string.Join("", update.ContentUpdate.Select(cu => cu.Text));
            var content = Markup.Escape(Emoji.Replace(message));

            _console.Clear();
            _console.Write(new Panel(content) { Border = BoxBorder.None });
        }

        var assistantChatMessage = ChatMessage.CreateAssistantMessage(message);
        _conversation.Add(assistantChatMessage);
        _console.Write(new Panel(new Markup(
                $"{Emoji.Known.UpArrow}: {_stats.InputTokenCount} {Emoji.Known.DownArrow}: {_stats.OutputTokenCount}"))
            { Border = BoxBorder.Rounded });
    }
}

public record Stats(int InputTokenCount, int OutputTokenCount);