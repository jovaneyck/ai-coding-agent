# Exercise 4: Tool Calling Basics

## Overview

Learn about tool calling (also known as function calling), the mechanism that allows LLMs to interact with external systems. You'll implement a simple `get_secret` tool to understand the complete tool calling flow.

## Learning Objectives

- Understand how tool calling extends LLM capabilities
- Learn the OpenAI function calling format
- Implement tool definition, detection, execution, and result handling

## Background: What is Tool Calling?

LLMs can only generate text. Tool calling allows them to:

1. **Request** to execute a function (e.g., "call get_secret()"). Function execution happens on the client side.
2. **Receive** the function's result as a new message type in the conversation.
3. **Use** that result to formulate a response

This enables LLMs to perform actions like reading files, querying databases, calling APIs, and more.

## The Tool Calling Flow

```
User: "What is the secret?"
   ↓
LLM: [requests tool call: get_secret()]
   ↓
Assistant: executes get_secret() → returns "42" → puts result in conversation
   ↓
LLM: generates response based on result in conversation
   ↓
Assistant: "The secret is 42"
```

## Instructions

### 1. Define the Tool

Tell the LLM what tools are available using the OpenAI function format:

```csharp
var tools = new List<ChatTool>
{
    ChatTool.CreateFunctionTool(
        name: "get_secret",
        description: "Gets the magical secret value"
    )
};
```

### 2. Include Tools in API Call

Pass the tools to the chat completion:

```csharp
var options = new ChatCompletionOptions
{
    Tools = { tools }
};

var response = await client.CompleteChatAsync(conversation, options);
```

### 3. Detect Tool Call Requests

Check if the LLM is requesting a tool call instead of generating a normal response:

```csharp
// The response may contain tool calls instead of text
if (response.Value.ToolCalls.Count > 0)
{
    // Handle tool call
}
else
{
    // Handle normal text response
}
```

Note: different models have different tool calling capabilities and sometimes request tool calls differently.
Ensure your model supports it and read up on how your openai client supports function calls.

### 4. Execute the Tool

When a tool call is detected, execute the corresponding function:

```csharp
foreach (var toolCall in response.Value.ToolCalls)
{
    if (toolCall.FunctionName == "get_secret")
    {
        var result = "42"; // Your hardcoded secret

        // Add tool result to conversation
        conversation.Add(ChatMessage.CreateToolMessage(toolCall.Id, result));
    }
}
```

### 5. Continue the Conversation

After adding the tool result, call the LLM again so it can use the result:

```csharp
// LLM will now see the tool result and generate a final response
var finalResponse = await client.CompleteChatAsync(conversation, options);
```

### 6. Handle the Loop

You may need to call the LLM multiple times:

```csharp
while (true)
{
    var response = await client.CompleteChatAsync(conversation, options);

    if (response.Value.ToolCalls.Count > 0)
    {
        // Execute tools and continue loop
        foreach (var toolCall in response.Value.ToolCalls)
        {
            var result = ExecuteTool(toolCall.FunctionName, toolCall.FunctionArguments);
            conversation.Add(ChatMessage.CreateToolMessage(toolCall.Id, result));
        }
    }
    else
    {
        // No more tool calls, we have the final response
        Console.WriteLine(response.Value.Content[0].Text);
        break;
    }
}
```

## Acceptance Criteria

Your implementation should pass this test:

**Input:**
```
> What is the secret?
```

**Expected Behavior:**
1. LLM requests `get_secret` tool call
2. Your code executes the tool and returns "42" (or your chosen secret)
3. LLM receives the result
4. LLM generates final response

**Expected Output:**
```
The secret is 42
```

(Or similar phrasing that includes the secret value)

## Common Pitfalls

- **Not adding tool calls to conversation**: Both the tool call request AND the tool result must be added to conversation history
- **Forgetting to loop**: After adding tool results, you must call the LLM again
- **Missing tool IDs**: Tool results must include the correct `tool_call_id`
- **Not handling assistant messages with tool calls**: The assistant's tool call request is also a message that should be added to history

## Testing Tips

Try these prompts to verify tool calling works:

```
> What is the secret?
> Can you look up the secret for me?
> I need to know the secret value, please retrieve it
```

All should trigger the tool call.

## Understanding Tool Call Messages

A complete tool calling exchange adds these messages to your conversation:

1. User message: "What is the secret?"
2. Assistant message with tool call request
3. Tool message with result: "42"
4. Assistant message with final response: "The secret is 42"

## Next Steps

Now that you understand tool calling, proceed to [Exercise 5: Reading Files](./05-reading-files.md) to implement a practical tool.
