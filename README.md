# ai-coding-agent

Build your own working AI coding agent. For educational purposes only.

## Step by step

<details>
<summary>(Optional) explore local LLM's with LM studio</summary>

This step is optional, but very much recommended if you want to understand how LLM's work. 
Download and install [LM Studio](https://lmstudio.ai/) and try to achieve the following tasks:

* Download a small local model (e.g. Qwen/Qwen2.5-Coder-7B-Instruct-GGUF).
* Try chatting with the downloaded model in LM studio. Play around with the settings (developer mode, temperature, top_p, max new tokens, etc).
* Try starting the LM studio server and sending a request to it via curl or postman. Play around with the settings (verbose logging, log incoming tokens)

</details>


<details>
<summary>Hello world!</summary>

Look for an openAI client for your language of choice.
You can use LM studio and your local model for the rest of this workshop. If your laptop struggles running tool-calling models you can use the OpenAI key provided by the facilitator.

Now let's build out your walking skeleton! 
Build a REPL loop that:

* asks for a user prompt
* forwards the prompt to the model
* prints the model response to console
* repeats

Acceptance test:
> Say "hello world"
> "Hello wold"

**note**: KISS aka keep it super simple, don't bother with any bells and whistles yet.
We have a lot to go through in the next 60 minutes 😅

If you feel comfortable in your coding skills and want to add _one_ super useful "bell and whistle", consider writing automated tests that run end-to-end acceptance tests for your AI agent.
It will save you from manually testing for regressions after each step.
</details>

<details>
<summary>Adding a system prompt</summary>

Different kinds of messages exist. We have seen the first two in hello world: user messages are the prompts we give to the model, assistant messages are messages returned by the model.
Now let's introduce a third kind of messages: system messages.
System messages are instructions that set the behavior of the model. Create a system prompt and load it into your conversation as the first message with role "system".

Acceptance test:
(create system message telling the model to talk like a pirate)
> Hi there!
> Ahoy matey! How can I be of service to ye today?
</details>

<details>
<summary>Let's a lengthy conversation</summary>

Now we want to keep track of the conversation. We can do this by keeping a list of messages and appending both user and system prompts to it.
Add this conversation history to future model calls.

Acceptance test:

> "My name is John"
> "Hello John, how can I help you today?"
> "What was my name again?"
> "Your name is John"

</details>

<details>
<summary>Tool calling basics</summary>

Now let's explore tool calls, the extension that allows LLM's to do more than just generate tokens.
We will start implementing a 'get_secret' tool that returns a secret value hardcoded in your code.

The LLM needs to be instructed which tools are available. We will use the OpenAI function calling format for this today.
Next, your agent code needs to be able to detect when the model is requesting a tool call and execute the actual tool (hardcode "42" or something clever as the result).
Both the LLM tool call request and the tool call response need to be added to the conversation history.

Acceptance test:
> What is the secret?
(tool call requested, tool call happens on your machine, LLM is presented with the result)
> 42

</details>


<details>
<summary>Real tools: reading files</summary>

Now let's some actually useful tools. A coding agent should be able to read a file, write a file and execute code.
Let's start with reading files. Implement a 'read_file' tool that takes a file path as input and returns the file contents as output.

Acceptance test:
(write a file "secret.txt" with a secret only you know e.g. "1234")
> Can your tell me the contents of the file secret.txt?
(tool call requested, tool call happens on your machine, LLM is presented with the result)
> 1234

</details>


<details>
<summary>Real tools: writing files</summary>

Implement a 'write_file' tool that takes a file path and contents as input and writes this to disk.

Acceptance test:

> Please come up with a poem and write it to poem.txt
(tool call requested, tool call happens on your machine)
(poem.txt is created and contains a poem)

</details>


<details>
<summary>Real tools: running bash scripts</summary>

Handle with care, dependening on how much access you give your agent to your machine this could brick your system.
</details>