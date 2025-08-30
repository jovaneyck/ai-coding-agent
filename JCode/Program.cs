using System.ClientModel;
using System.Text;
using OpenAI;
using OpenAI.Chat;
using Spectre.Console;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

// var modelProviderUri = new Uri("https://openrouter.ai/api/v1");
// var model = "moonshotai/kimi-k2:free";

var modelProviderUri = new Uri("http://192.168.129.8:11434/v1");
var model = "qwen2.5-coder:7b-instruct";

var client = new ChatClient(
    model,
    new ApiKeyCredential(":unused:ollama:"),
    new OpenAIClientOptions
    {
        Endpoint = modelProviderUri
    });
var conversation = new List<ChatMessage>();
var stats = new Stats(0, 0);

var ui = new Layout("Root")
    .SplitRows(
        new Layout("Response"),
        new Layout("Stats") );
var prompt = await AnsiConsole.AskAsync<string>(">");

while (true)
{
    await AnsiConsole.Live(ui).StartAsync(async ctx =>
    {
        var userChatMessage = ChatMessage.CreateUserMessage(prompt);
        conversation.Add(userChatMessage);

        var streamingResult = client.CompleteChatStreamingAsync(conversation);
        var message = "";
        await foreach (var update in streamingResult)
        {
            if (update.Usage != null)
                stats = new Stats(
                    stats.InputTokenCount + update.Usage.InputTokenCount,
                    stats.OutputTokenCount + update.Usage.OutputTokenCount);

            message += string.Join("", update.ContentUpdate.Select(cu => cu.Text));
            var content = Emoji.Replace(message);
            ui["Response"].Update(new Panel(content) { Border = BoxBorder.Rounded });
            ui["Stats"].Update(new Panel(new Markup(
                    $"{Emoji.Known.UpArrow}: {stats.InputTokenCount} {Emoji.Known.DownArrow}: {stats.OutputTokenCount}"))
                { Border = BoxBorder.Rounded });
            ctx.Refresh();
        }

        var assistantChatMessage = ChatMessage.CreateAssistantMessage(message);
        conversation.Add(assistantChatMessage);
    });

    prompt = await AnsiConsole.AskAsync<string>(">");
}


public record Stats(int InputTokenCount, int OutputTokenCount);