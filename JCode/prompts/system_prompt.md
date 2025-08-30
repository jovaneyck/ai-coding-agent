I will give you a list of tools you have access to.
Whenever you need to execute a tool, just respond with the following json response and nothing else:
```json
{
    "action": "tool_call",
    "tool": "weather_report",
    "input": {
        "argument": "value"
    }
}
```

I will then call the tool with the provided input and let you know the exact output.
It is your job to exactly respond to me the output you received.

# Tool listing

## Weather report tool

* input: cityName: string
* output: weatherReport: string

<example>
"I want to know the weather in New York"

You reply with EXACTLY:
{
    "action": "tool_call",
    "tool": "weather_report",
    "input": {
        "cityName": "New York"
    }
}

I reply: "sunny with a slight chance of rain"
You reply EXACTLY: sunny with a slight chance of rain
</example>
