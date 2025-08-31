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

        var app = new CommandAppTester(null, null, console);
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
        console.Input.PushTextWithEnter("calculate the value of x = 1 + 3 without tool calls");
        console.Input.PushTextWithEnter("What is my name?");
        console.Input.PushTextWithEnter("What is the value of x?");
        console.Input.PushTextWithEnter("exit");

        var app = new CommandAppTester(null, null, console);
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

        var app = new CommandAppTester(null, null, console);
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
            console.Input.PushTextWithEnter(
                $"Read the contents in the file at {tempFile} and echo EXACTLY what you find.");
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
            console.Input.PushTextWithEnter(
                $"First concatenate \'ab\' and \'cd\' by yourself. Next, write ONLY the result ot that concatenation in a new text file at {tempFile}.");
            console.Input.PushTextWithEnter("exit");

            var app = new CommandAppTester(null, null, console);
            app.SetDefaultCommand<ChatCommand>();
            var result = app.Run();

            var output = console.Output;
            outputHelper.WriteLine(output);
            Assert.Equal(0, result.ExitCode);
            var contents = File.ReadAllText(tempFile);
            Assert.Contains("abcd", contents);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }


    [Fact]
    public void Writes_And_Runs_Code()
    {
        var tempFile = Path.GetTempFileName().Replace(".tmp", ".js");
        
        try
        {
            var console = new TestConsole();
            console.Input.PushTextWithEnter(
                $"Today we will be programming in JAVASCRIPT. Create a self-contained ONELINER (semi-colons allowed) that calculates fac(10) and save it to {tempFile}.");
            console.Input.PushTextWithEnter("Now run that script using NODE and tell me the result.");
            console.Input.PushTextWithEnter("exit");

            var app = new CommandAppTester(null, null, console);
            app.SetDefaultCommand<ChatCommand>();
            var result = app.Run();

            var output = console.Output;
            outputHelper.WriteLine(output);
            Assert.Equal(0, result.ExitCode);
            var contents = File.ReadAllText(tempFile);
            outputHelper.WriteLine("The script we came up with:");
            outputHelper.WriteLine(contents);
            Assert.NotEmpty(contents);
            Assert.DoesNotContain("3628800", contents.Replace(".","").Replace(",",""));
            Assert.Contains("3628800", output.Replace(".","").Replace(",",""));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}