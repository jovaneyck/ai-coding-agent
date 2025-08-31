using JCode;
using Spectre.Console.Testing;
using Xunit;
using Xunit.Abstractions;

namespace JCode.Tests;

public class ChatCommandTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public void Hello_world()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("Just reply with \'hello world\'");
        console.Input.PushTextWithEnter("exit");

        var app = new CommandAppTester(null,null,console);
        app.SetDefaultCommand<ChatCommand>();
        var result = app.Run();

        var output = console.Output;
        outputHelper.WriteLine(output);
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("hello", output.ToLowerInvariant());
    }

    [Fact]
    public void Multi_prompt_conversations_and_both_user_and_assistant_chats_are_kept_in_context()
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

        var output = console.Output;
        outputHelper.WriteLine(output);
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("john", output.ToLowerInvariant());
        Assert.Contains("4", output.ToLowerInvariant());
    }

    [Fact]
    public void Get_secret_via_tool_call()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("what is the secret?");
        console.Input.PushTextWithEnter("exit");

        var app = new CommandAppTester(null,null,console);
        app.SetDefaultCommand<ChatCommand>();
        var result = app.Run();
        
        var output = console.Output;
        outputHelper.WriteLine(output);
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("key lime pie", output.ToLower());
    }

    [Fact]
    public void Read_file_contents()
    {
        var secret = Guid.NewGuid().ToString();
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, secret);

        try
        {
            var console = new TestConsole();
            console.Input.PushTextWithEnter($"Read the contents in the file at {tempFile} and echo EXACTLY what you find.");
            console.Input.PushTextWithEnter("exit");

            var app = new CommandAppTester(null, null, console);
            app.SetDefaultCommand<ChatCommand>();
            var result = app.Run();

            var output = console.Output;
            outputHelper.WriteLine(output);
            Assert.Equal(0, result.ExitCode);
            Assert.Contains(secret, output);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
    
    [Fact]
    public void Write_files()
    {
        var tempFile = Path.GetTempFileName();

        try
        {
            var console = new TestConsole();
            console.Input.PushTextWithEnter($"Write the result of 1300 + 37 in a new file at {tempFile}.");
            console.Input.PushTextWithEnter("exit");

            var app = new CommandAppTester(null, null, console);
            app.SetDefaultCommand<ChatCommand>();
            var result = app.Run();
            
            var output = console.Output;
            outputHelper.WriteLine(output);
            Assert.Equal(0, result.ExitCode);
            var contents = File.ReadAllText(tempFile);
            Assert.Contains("1337",contents);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}