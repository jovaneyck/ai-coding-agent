# Exercise 1: Hello World - Building Your First REPL

## Overview

In this exercise, you'll build the foundation of your AI coding agent: a REPL (Read-Eval-Print Loop) that can communicate with an LLM. This is your "walking skeleton": the simplest possible end-to-end implementation.

## Learning Objectives

- Set up an OpenAI-compatible client in your language of choice
- Create a basic conversation loop
- Send user input to the LLM and display responses

## Instructions

### 1. Choose and Install an OpenAI Client Library

Select a client library for your preferred programming language:

- **Python**: [openai-python](https://github.com/openai/openai-python)
- **JavaScript/TypeScript**: [openai-node](https://github.com/openai/openai-node)
- **C#**: [openai-dotnet](https://github.com/openai/openai-dotnet)
- **JVM**: [openai-java](https://github.com/openai/openai-java)
- **Go**: [openai-go](https://github.com/openai/openai-go)

Install the appropriate package for your language.

### 2. Configure Your LLM Connection

You have three options:

**Option A: Use LM Studio (Local)**
- Start LM Studio server (typically `http://localhost:1234/v1`)
- Configure your client to point to this endpoint
- No API key required (use a dummy key like "not-needed")
- 
**Option B: Use a key provided by the facilitator (Cloud)**
- Facilitator will hand out url, key and model names

**Option C: Use your own API key (Cloud)**

### 3. Build the REPL Loop

Create a program that:

1. **Prompts the user** for input (e.g., "You: " or "> ")
2. **Sends the user's message** to the LLM via the chat completion API
3. **Prints the LLM's response** to the console
4. **Repeats** until the user types "exit" or similar

**Keep it simple!** Don't worry about:
- Conversation history (yet)
- Error handling
- Fancy UI
- Streaming responses

### 4. Example Structure

Here's pseudocode for your REPL:

```
while true:
    user_input = read_input_from_console()

    if user_input == "exit":
        break

    response = send_to_llm(user_input)

    print(response)
```

## Acceptance Criteria

Your implementation should pass this test:

**Input:**
```
> Say "hello world"
```

**Expected Output:**
```
Hello world
```

(Note: The exact output may vary slightly depending on the model, but it should contain "hello world" or a close variant)

## Tips

- **KISS (Keep It Super Simple)**: Resist the urge to add features. You have plenty more exercises ahead!
- **Test as you go**: Manually verify it works before moving on
- **Optional enhancement**: If you're comfortable with testing, consider writing automated end-to-end tests. This will save you from manual regression testing after each exercise.

## Next Steps

Once your REPL is working, proceed to [Exercise 2: Adding a System Prompt](./02-system-prompt.md).
