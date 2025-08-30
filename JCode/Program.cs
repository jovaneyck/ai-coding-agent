using System.Text;
using JCode;
using Spectre.Console.Cli;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

var app = new CommandApp<ChatCommand>();
return app.Run(args);