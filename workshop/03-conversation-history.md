# Exercise 3: Conversation History

## Overview

Make your agent remember previous interactions by maintaining conversation history. This allows for natural, multi-turn conversations where the model can reference earlier messages.

## Learning Objectives

- Understand how LLMs use conversation history for context
- Implement message history tracking
- Pass conversation context to the LLM on each turn

## Background

LLMs are stateless; they don't inherently remember previous messages. To create the illusion of memory, we must send the entire conversation history with each API call. The model can then use this context to maintain coherent, contextual responses.

## Instructions

### 1. Create a Conversation History List

Initialize a list to store all messages (system, user, and assistant):

```csharp
var conversation = new List<ChatMessage>();
```

### 2. Add System Message at Startup

Add your system prompt as the first message:

```csharp
conversation.Add(ChatMessage.CreateSystemMessage(systemPrompt));
```

### 3. Update Your REPL Loop

Modify your loop to:

1. **Add user message** to the conversation list
2. **Send entire conversation** to the LLM
3. **Add assistant response** to the conversation list
4. **Print** the assistant's response

## Acceptance Criteria

Your implementation should pass this test:

**Input (First Message):**
```
> My name is John
```

**Expected Output:**
```
Hello John, how can I help you today?
```

**Input (Second Message):**
```
> What was my name again?
```

**Expected Output:**
```
Your name is John
```

The model should remember "John" from the first message.

## Understanding Token Usage

Important consideration: Conversation history grows with each message, increasing token usage and API costs. Each turn includes:

- System message
- All previous user messages
- All previous assistant messages
- Current user message

For long conversations, you may need to implement:
- **Token limits**: Truncate old messages when history gets too long
- **Summarization/compaction**: Replace old messages with an LLM-generated summary
- **Sliding window**: Keep only the last N messages

For this workshop, you can ignore these optimizations.

## Testing Tips

Test multi-turn conversations:

```
> My favorite color is blue
> I have a dog named Max
> What is my favorite color?
> What is my dog's name?
```

The model should correctly answer both questions.

## Common Pitfalls

- **Not adding assistant responses**: Both user AND assistant messages must be stored
- **Clearing history accidentally**: Make sure your list persists across loop iterations
- **Wrong message order**: Messages should be chronological

## Next Steps

Now that your agent can maintain context, proceed to [Exercise 4: Tool Calling Basics](./04-tool-calling-basics.md) to give it real capabilities.
