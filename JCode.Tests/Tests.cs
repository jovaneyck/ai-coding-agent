using JCode;
using Spectre.Console.Testing;
using Xunit;

namespace JCode.Tests;

public class ChatCommandTests
{
    [Fact]
    public void I_can_echo_hello_world()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("hello world");
        console.Input.PushTextWithEnter("exit");

        var app = new CommandAppTester(null,null,console);
        app.SetDefaultCommand<ChatCommand>();
        var result = app.Run();

        Assert.Equal(0, result.ExitCode);
        var output = console.Output;
        Assert.Contains("hello", output.ToLowerInvariant());
    }

    [Fact]
    public void I_can_have_multi_prompt_conversations_and_both_user_and_assistant_chats_are_kept_in_context()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("My name is John");
        console.Input.PushTextWithEnter("x = 1 + 3");
        console.Input.PushTextWithEnter("What is my name?");
        console.Input.PushTextWithEnter("What is the value of x?");
        console.Input.PushTextWithEnter("exit");

        var app = new CommandAppTester(null,null,console);
        app.SetDefaultCommand<ChatCommand>();
        var result = app.Run();

        Assert.Equal(0, result.ExitCode);
        var output = console.Output;
        Assert.Contains("john", output.ToLowerInvariant());
        Assert.Contains("4", output.ToLowerInvariant());
    }

    [Fact]
    public void I_can_get_weather_report_via_tool_call()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("what is the weather in Geel?");
        console.Input.PushTextWithEnter("exit");

        var app = new CommandAppTester(null,null,console);
        app.SetDefaultCommand<ChatCommand>();
        var result = app.Run();

        Assert.Equal(0, result.ExitCode);
        var output = console.Output;
        Assert.Contains("Sunny with a 30% chance of heavy rain.", output);
    }
}