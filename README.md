# AI Coding Agent Workshop

Build your own working AI coding agent from scratch. For educational purposes only.

## Overview

This workshop guides you through building a functional AI coding agent that can:
- Can be fully tweaked by using a system prompt
- Maintain conversational context
- Use tool calling to interact with the system
- Read and write files
- Execute code and shell commands

By the end, you'll understand the core concepts behind AI assistants like GitHub Copilot, Cursor, and Claude Code.

## Prerequisites

- Advanced (networking, async, IO) programming knowledge in your chosen language (C#, Python, JavaScript, etc.)
- Familiarity with http APIs and JSON
- A development environment set up
- Either:
  - LM Studio with a local model (highly recommended for learning about LLM's), OR
  - An OpenAI API key provided by the facilitator

## Workshop Exercises

Follow these exercises in order. Each step builds upon the previous one:

### 0. [Explore Local LLMs with LM Studio](./workshop/00-optional-lm-studio.md) *(Optional)*
Get hands-on experience with local language models before building your agent.

### 1. [Hello World - Building Your First REPL](./workshop/01-hello-world.md)
Create a basic conversation loop that sends messages to an LLM and displays responses.

### 2. [Adding a System Prompt](./workshop/02-system-prompt.md)
Learn how to control LLM behavior using system messages.

### 3. [Conversation History](./workshop/03-conversation-history.md)
Implement memory so your agent can maintain context across multiple turns.

### 4. [Tool Calling Basics](./workshop/04-tool-calling-basics.md)
Understand the fundamentals of function calling with a simple `get_secret` tool.

### 5. [Reading Files](./workshop/05-reading-files.md)
Implement a practical tool that allows the LLM to read file contents.

### 6. [Writing Files](./workshop/06-writing-files.md)
Enable your agent to create and modify files on disk.

### 7. [Running Scripts](./workshop/07-running-scripts.md)
Add the ability to execute shell commands and scripts. ⚠️ **Use with caution!**

## Expected Time

- Core exercises (1-7): ~60-90 minutes
- With optional LM Studio exploration: +30 minutes

## Tips for Success

- **Work incrementally**: Complete each exercise before moving to the next
- **Test frequently**: Verify each feature works before adding complexity
- **Keep it simple**: Don't over-engineer early exercises, we'll be pressed for time already
- **Consider automated testing**: End-to-end automated tests save time on regression testing

## What You'll Learn

- How LLMs process conversations and maintain context
- The OpenAI chat completion API format
- Tool calling / function calling mechanisms
- Architecture of AI agents with tool calling capabilities
- Security considerations for AI agents with system access

## Reference Implementation

A complete C# implementation is available in this repository for reference. See `JCode/ChatCommand.cs` for the main implementation and `JCode.Tests/Tests.cs` for test examples.

## Safety Warning

⚠️ **Important**: Exercise 7 (Running Scripts) allows the AI to execute arbitrary commands on your system. This can be dangerous!

## Next Steps After the Workshop

Happy building! 🤖