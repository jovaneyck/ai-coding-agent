# Exercise 7: Running Scripts

## Overview

Implement the ability to execute shell commands or scripts. This is the most powerful (and dangerous) capability, allowing the agent to run code, execute tests, and interact with the system.

## Learning Objectives

- Execute shell commands programmatically
- Capture and return command output
- Understand security implications of code execution
- Handle command failures gracefully

## Background

A fully functional coding agent needs to:

- Run tests to verify code works
- Execute scripts (Python, Node.js, etc.)
- Use command-line tools (git, npm, etc.)
- Build and compile projects

The ability to execute commands completes your agent's toolkit.

## ⚠️ CRITICAL WARNING

**This is extremely dangerous!**

The LLM can execute ANY command your user account has permission to run, including:

- Deleting files (`rm -rf /` on Linux, `del /F /S /Q C:\` on Windows)
- Installing malware
- Exfiltrating data
- Modifying system files
- Running arbitrary code

**For this workshop:**
- Use a test environment or VM if possible
- Don't point it at production systems
- Be prepared to stop the process if it does something unexpected
- Consider implementing safety checks (command whitelists, user confirmation, etc.)

## Instructions

### 1. Define the Tool

Choose a shell based on your OS:

**Option A: Bash/Shell (Linux/Mac)**

```csharp
ChatTool.CreateFunctionTool(
    name: "execute_bash",
    description: "Executes a bash command and returns the output. Use this to run scripts, check files, execute programs, etc.",
    parameters: BinaryData.FromString("""
    {
        "type": "object",
        "properties": {
            "command": {
                "type": "string",
                "description": "The bash command to execute"
            }
        },
        "required": ["command"]
    }
    """)
)
```

**Option B: PowerShell (Windows)**

```csharp
ChatTool.CreateFunctionTool(
    name: "execute_powershell",
    description: "Executes a PowerShell script and returns the output. Use this to run scripts, check files, execute programs, etc.",
    parameters: BinaryData.FromString("""
    {
        "type": "object",
        "properties": {
            "script": {
                "type": "string",
                "description": "The PowerShell script to execute"
            }
        },
        "required": ["script"]
    }
    """)
)
```

### 2. Implement Command Execution (Bash Example)

```csharp
using System.Diagnostics;

string ExecuteBash(Dictionary<string, string> arguments)
{
    var command = arguments["command"];

    try
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{command.Replace("\"", "\\\"")}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(processInfo);
        if (process == null)
        {
            return "Error: Failed to start process";
        }

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            return $"Command failed with exit code {process.ExitCode}\nError: {error}\nOutput: {output}";
        }

        return string.IsNullOrEmpty(output) ? "Command executed successfully (no output)" : output;
    }
    catch (Exception ex)
    {
        return $"Error executing command: {ex.Message}";
    }
}
```

### 3. Implement PowerShell Execution (Windows Alternative)

```csharp
using System.Management.Automation;

async Task<string> ExecutePowerShell(Dictionary<string, string> arguments)
{
    var script = arguments["script"];

    try
    {
        using var ps = PowerShell.Create();
        ps.AddScript(script);

        var results = await ps.InvokeAsync();

        var output = string.Join(Environment.NewLine, results.Select(r => r?.ToString()));

        if (ps.HadErrors)
        {
            var errors = string.Join(Environment.NewLine, ps.Streams.Error.Select(e => e.ToString()));
            return $"Script executed with errors:\n{errors}\n\nOutput:\n{output}";
        }

        return string.IsNullOrEmpty(output) ? "Script executed successfully (no output)" : output;
    }
    catch (Exception ex)
    {
        return $"Error executing PowerShell: {ex.Message}";
    }
}
```

### 4. Add to Tool Dispatcher

```csharp
string ExecuteTool(string toolName, string argumentsJson)
{
    var arguments = JsonSerializer.Deserialize<Dictionary<string, string>>(argumentsJson);

    return toolName switch
    {
        "get_secret" => "42",
        "read_file" => ExecuteReadFile(arguments),
        "write_file" => ExecuteWriteFile(arguments),
        "execute_bash" => ExecuteBash(arguments),
        // or "execute_powershell" => await ExecutePowerShell(arguments),
        _ => $"Unknown tool: {toolName}"
    };
}
```

## Acceptance Criteria

Your implementation should pass this test:

**Setup:**
Create a simple test script:

**JavaScript (test.js):**
```javascript
function factorial(n) {
    if (n <= 1) return 1;
    return n * factorial(n - 1);
}
console.log(factorial(10));
```

**Input:**
```
> Run the JavaScript file test.js using Node and tell me the result
```

**Expected Behavior:**
1. LLM requests command execution: `node test.js`
2. Your code executes the command
3. Returns output: "3628800"
4. LLM includes this in its response

**Expected Output:**
```
The result of running test.js is 3628800
```

## Alternative Test (Simpler)

**Input:**
```
> What is the current date and time? Use a shell command to find out.
```

**Expected:**
- LLM calls `date` (Linux) or `Get-Date` (PowerShell)
- Returns current date/time
- LLM formats and displays it

## Testing Tips

**Safe commands to test:**

```
> List all files in the current directory
> What's my current working directory?
> Check if Python is installed and what version
> Create a test file with echo and read it back
```

**Complex workflow:**

```
> Create a JavaScript file that calculates the sum of 1 to 100, save it to sum.js, then run it with Node
```

This tests: file writing + command execution + multi-step reasoning.

## Common Pitfalls

- **Not capturing stderr**: Errors may only appear in standard error stream
- **Infinite processes**: Some commands never terminate (e.g., web servers) - consider timeouts
- **Escaping issues**: Special characters in commands can break execution
- **Path problems**: Commands may not have access to your PATH environment variable
- **Async execution**: PowerShell requires async/await

## Safety Enhancements (Optional)

### 1. Command Whitelist

```csharp
var allowedCommands = new[] { "ls", "pwd", "cat", "echo", "node", "python" };

string ExecuteBash(Dictionary<string, string> arguments)
{
    var command = arguments["command"];
    var firstWord = command.Split(' ')[0];

    if (!allowedCommands.Contains(firstWord))
    {
        return $"Error: Command '{firstWord}' is not allowed";
    }

    // ... proceed with execution
}
```

### 2. User Confirmation

```csharp
Console.WriteLine($"LLM wants to execute: {command}");
Console.Write("Allow? (y/n): ");

if (Console.ReadLine()?.ToLower() != "y")
{
    return "Command execution denied by user";
}
```

### 3. Timeout

```csharp
process.WaitForExit(5000); // 5 second timeout

if (!process.HasExited)
{
    process.Kill();
    return "Error: Command timed out after 5 seconds";
}
```

## Advanced: Multi-Step Workflows

Your agent can now:

1. Generate code
2. Write it to a file
3. Execute it
4. Analyze the results
5. Fix bugs and retry

Example prompt:
```
> Create a Python script that fetches weather data from an API, save it to weather.py, run it, and show me the output
```

## Debugging Tips

Log all command executions:

```csharp
Console.WriteLine($"[Executing] {command}");
Console.WriteLine($"[Exit Code] {process.ExitCode}");
Console.WriteLine($"[Output] {output}");
```

This helps you understand what the LLM is trying to do.

## Next Steps

Congratulations! You've built a complete AI coding agent with:

- ✅ Conversation memory
- ✅ System prompts for behavior control
- ✅ File reading
- ✅ File writing
- ✅ Code execution

### Ideas for Further Enhancement

1. **Better error handling**: Catch and explain errors more gracefully
2. **Streaming responses**: Show output as it's generated
3. **Multiple tools per turn**: Allow calling several tools in one response
4. **Web search**: Add internet access via an API
5. **Git integration**: Add tools for `git add`, `git commit`, `git push`
6. **Safety layers**: Implement sandboxing, rate limits, confirmation dialogs
7. **Memory**: Persist conversation across sessions

You now have the foundation to build increasingly sophisticated AI agents!
