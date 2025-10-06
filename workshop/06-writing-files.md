# Exercise 6: Writing Files

## Overview

Implement the `write_file` tool to allow the LLM to create and modify files. Combined with `read_file`, this gives your agent the ability to make actual code changes.

## Learning Objectives

- Implement a tool with multiple parameters
- Handle file writing operations safely
- Enable the LLM to generate and persist content

## Background

The ability to write files transforms your agent from read-only to interactive. The LLM can now:

- Generate code and save it
- Create configuration files
- Write documentation
- Modify existing files (by reading, then writing)

## Instructions

### 1. Define the write_file Tool

This tool requires two parameters: file path and content:

```csharp
ChatTool.CreateFunctionTool(
    name: "write_file",
    description: "Writes content to a file. Creates the file if it doesn't exist, overwrites if it does.",
    parameters: BinaryData.FromString("""
    {
        "type": "object",
        "properties": {
            "file_path": {
                "type": "string",
                "description": "The path where the file should be written"
            },
            "content": {
                "type": "string",
                "description": "The content to write to the file"
            }
        },
        "required": ["file_path", "content"]
    }
    """)
)
```

### 2. Implement the Tool Logic

```csharp
string ExecuteWriteFile(Dictionary<string, string> arguments)
{
    var filePath = arguments["file_path"];
    var content = arguments["content"];

    try
    {
        // Ensure directory exists
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(filePath, content);

        return $"Successfully wrote {content.Length} characters to {filePath}";
    }
    catch (Exception ex)
    {
        return $"Error writing file: {ex.Message}";
    }
}
```

### 3. Add to Tool Dispatcher

```csharp
string ExecuteTool(string toolName, string argumentsJson)
{
    var arguments = JsonSerializer.Deserialize<Dictionary<string, string>>(argumentsJson);

    return toolName switch
    {
        "get_secret" => "42",
        "read_file" => ExecuteReadFile(arguments),
        "write_file" => ExecuteWriteFile(arguments),
        _ => $"Unknown tool: {toolName}"
    };
}
```

## Acceptance Criteria

Your implementation should pass this test:

**Input:**
```
> Please come up with a poem and write it to poem.txt
```

**Expected Behavior:**
1. LLM generates a poem
2. LLM requests `write_file` tool call with:
   - `file_path`: "poem.txt"
   - `content`: [the generated poem]
3. Your code writes the file
4. Tool returns success message
5. LLM confirms completion

**Expected Output:**
```
I've written a poem to poem.txt
```

**Verification:**
- File `poem.txt` exists
- File contains a poem (multiple lines of text)

## Example Test

```csharp
// After running your agent with the prompt:
// "Please come up with a poem and write it to poem.txt"

Assert.True(File.Exists("poem.txt"));

var content = File.ReadAllText("poem.txt");
Assert.NotEmpty(content);
Assert.True(content.Split('\n').Length > 1); // Multi-line
```

## Security Considerations

⚠️ This tool can write ANYWHERE on your system. Risks include:

- **Overwriting system files**: Could break your OS
- **Writing to protected directories**: May require elevated permissions
- **Filling disk space**: Malicious/buggy prompts could write huge files

**Mitigations** (not required for workshop, but important for production):

```csharp
// Example: Restrict to a specific directory
string ExecuteWriteFile(Dictionary<string, string> arguments)
{
    var filePath = arguments["file_path"];
    var allowedDirectory = Path.GetFullPath("./workspace");
    var fullPath = Path.GetFullPath(filePath);

    if (!fullPath.StartsWith(allowedDirectory))
    {
        return "Error: Can only write files in ./workspace directory";
    }

    // ... proceed with writing
}
```

## Testing Tips

**Test different scenarios:**

**1. Simple text file:**
```
> Write "Hello World" to test.txt
```

**2. Code generation:**
```
> Create a Python function that calculates fibonacci numbers and save it to fib.py
```

**3. Structured content:**
```
> Generate a JSON configuration file with settings for a web server and save it to config.json
```

**4. Multi-step workflow:**
```
> Read README.md, add a new section about installation, and write it back
```

## Common Pitfalls

- **Directory doesn't exist**: Always create parent directories if needed
- **Path separators**: Handle both `/` and `\`
- **Encoding issues**: Use UTF-8 for text files (default for `File.WriteAllText`)
- **File permissions**: Ensure your process has write permissions
- **Overwriting important files**: Be careful during testing!

## Advanced: Combining Read and Write

Your agent can now edit files:

```
> Read calculator.py, add error handling for division by zero, and write it back
```

This requires:
1. LLM calls `read_file` to get current content
2. LLM modifies the content
3. LLM calls `write_file` with modified content

## Debugging Tips

Log tool calls to see exactly what the LLM is trying to write:

```csharp
Console.WriteLine($"[Writing file] {filePath}");
Console.WriteLine($"[Content length] {content.Length} characters");
Console.WriteLine($"[Preview] {content.Substring(0, Math.Min(100, content.Length))}...");
```

## Next Steps

Your agent can now read and write files! Proceed to [Exercise 7: Running Scripts](./07-running-scripts.md) to give it the ability to execute code.
