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
var prompt = await AnsiConsole.PromptAsync(new TextPrompt<string>(">"));

while (true)
{
    var completion = await client.CompleteChatAsync(
        ChatMessage.CreateUserMessage(prompt));

    var stats = completion.Value.Usage;
    var message = completion.Value.Content[0].Text;

    var content = Emoji.Replace(message);
    var ui = new Layout()
        .SplitRows(
            new Layout(new Panel(content) { Border = BoxBorder.Rounded }),
            new Layout(new Panel(new Markup(
                    $"{Emoji.Known.UpArrow}: {stats.InputTokenCount} {Emoji.Known.DownArrow}: {stats.OutputTokenCount}"))
                { Border = BoxBorder.Rounded }));
    AnsiConsole.Write(ui);
    prompt = await AnsiConsole.PromptAsync(new TextPrompt<string>(">"));
}