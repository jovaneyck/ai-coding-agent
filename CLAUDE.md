# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is an educational AI coding agent built with C# and .NET 9.0. It's a workshop project designed to teach how to build a functional AI coding agent that can interact with LLMs, handle tool calling, and execute commands. The agent uses OpenAI's chat completion API format (compatible with Ollama and other providers) and implements a REPL-style interface with tool calling capabilities.

## Development Commands

### Build
```
dotnet build
```

### Run Tests
```
dotnet test
```

### Run the Application
```
dotnet run --project JCode
```

### Run a Single Test
```
dotnet test --filter "FullyQualifiedName~JCode.Tests.ChatCommandTests.Hello_world"
```

## Architecture

### Core Components

**ChatCommand (JCode\ChatCommand.cs)**: The main command handler that orchestrates the entire chat flow
- Manages conversation history as a list of ChatMessage objects
- Implements a tool call chain with max 10 iterations to prevent infinite loops
- Uses streaming chat completions for real-time response display
- Parses tool calls from model output using regex-based JSON extraction (required for Ollama/Qwen compatibility)
- Tracks token usage statistics via Stats record

**Tool Call Flow**:
1. User provides prompt → added to conversation as user message
2. HandleToolCallChainAsync() iterates up to 10 times:
   - RunInference() calls the LLM with current conversation + available tools
   - Response is added as assistant message
   - ParseToolCall() checks if response contains a tool call request
   - If tool call found: execute it via Call(), add result as tool message, continue loop
   - If no tool call: break and display final response

**Available Tools**:
- `get_secret`: Returns hardcoded secret ("Key lime pie")
- `powershell`: Executes PowerShell scripts for file operations, directory inspection, and running scripts

**System Prompt**: Loaded from `prompts/system_prompt.md` at session start

**Test Strategy (JCode.Tests\Tests.cs)**:
- End-to-end acceptance tests that verify agent behavior through Spectre.Console.Testing
- Tests use TestConsole to simulate user input and capture output
- Tests verify: basic prompts, conversation history retention, tool calling (get_secret), file operations (read/write via PowerShell), and code execution

### Key Technical Details

- Uses Spectre.Console for rich terminal UI with panels, emoji support, and markup
- Model responses are streamed token-by-token with live display updates
- JSON tool call parsing handles Ollama/Qwen format quirks (extracts JSON from markdown code blocks)
- PowerShell integration via System.Management.Automation for executing scripts
- Conversation can be reset with `/clear` command
- Currently configured for Ollama endpoint at `http://192.168.129.27:11434/v1` with `qwen2.5-coder:7b-instruct`

### Test Patterns

Tests follow a pattern of:
1. Create TestConsole and queue input commands via PushTextWithEnter()
2. Create CommandAppTester with ChatCommand
3. Run the app
4. Assert on exit code and output content

All tests end with "exit" command to terminate the REPL loop.

## Workshop Progression

The README.md describes a step-by-step workshop to build this agent:
1. Basic REPL with model interaction
2. System prompts
3. Conversation history
4. Tool calling basics
5. File reading/writing tools
6. Bash/PowerShell execution

Tests correspond to these workshop steps and serve as acceptance criteria.

## Configuration

- Model endpoint configured in ChatCommand constructor (currently hardcoded for Ollama)
- System prompt loaded from `prompts/system_prompt.md`
- User secrets supported (UserSecretsId configured in .csproj)
