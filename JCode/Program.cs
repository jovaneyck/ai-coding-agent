using System.ClientModel;
using System.Text;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using Spectre.Console;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

var cb = new ConfigurationBuilder();
var config = cb.AddUserSecrets<Program>().Build();
var apiKey = new ApiKey(config[Constants.OPENROUTER_API_KEY]!);
var modelProviderUri = new Uri("https://openrouter.ai/api/v1");

var client = new ChatClient(
    "moonshotai/kimi-k2:free",
    new ApiKeyCredential(apiKey.Key),
    new OpenAIClientOptions
    {
        Endpoint = modelProviderUri
    });

var completion = await client.CompleteChatAsync(
    ChatMessage.CreateUserMessage(
        "Hello kimi! How are you today?"));
var message = completion.Value.Content[0].Text;

var content = Emoji.Replace(message);
var ui = new Panel(content) { Border = BoxBorder.Rounded };
AnsiConsole.Write(ui);
