using System.Text;
using Spectre.Console;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

var content = Emoji.Replace("[underline green]Hello[/] World! :rocket:");
var ui = new Panel(content) { Border = BoxBorder.Rounded };
AnsiConsole.Write(ui);