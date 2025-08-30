using System.ClientModel;
using System.Text;
using Microsoft.Extensions.Configuration;
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

var completion = await client.CompleteChatAsync(
    ChatMessage.CreateUserMessage(
        "Reply with the following message where you fill in your own model name: \'hello, my name is {name} :rocket:\'."));
var logprobs = completion.Value.ContentTokenLogProbabilities;
var stats = completion.Value.Usage;
var message = completion.Value.Content[0].Text;

var content = Emoji.Replace(message);
var ui = new Layout().SplitRows(
    new Layout(new Panel(content) { Border = BoxBorder.Rounded }),
    new Layout(new Markup($"last message tokens in: {stats.InputTokenCount} out: {stats.OutputTokenCount}")));
AnsiConsole.Write(ui);
