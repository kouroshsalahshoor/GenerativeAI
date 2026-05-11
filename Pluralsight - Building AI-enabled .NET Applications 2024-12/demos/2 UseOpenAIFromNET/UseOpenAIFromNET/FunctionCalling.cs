using OpenAI;
using OpenAI.Chat;
using System.Text.Json;

namespace UseOpenAIFromNET
{
    public static class FunctionCalling
    {
        private static readonly ChatTool getClosestBethanysPieShopStoreTool = ChatTool.CreateFunctionTool(
            functionName: nameof(GetClosestBethanysPieShopStore),
            functionDescription: "Get the closest Bethany's Pie Shop store based on the user's location"
        );

        private static readonly ChatTool getWeatherAtBethanysPieShopStoreTool = ChatTool.CreateFunctionTool(
            functionName: nameof(GetWeatherAtBethanysPieShopStore),
            functionDescription: "Get the weather at the given Bethany's Pie Shop store",
            functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "location": {
                    "type": "string",
                    "description": "The city and country"
                },
                "unit": {
                    "type": "string",
                    "enum": [ "celsius", "fahrenheit" ],
                    "description": "The temperature unit to use."
                }
            },
            "required": [ "location" ]
        }
        """)
        );

        public static void SimpleFunctionCalling(string modelName)
        {

            bool requiresAction = true;

            OpenAIClient openAiClient = new(Environment.GetEnvironmentVariable("OPENAI_API_KEY"));

            ChatClient chatClient = openAiClient.GetChatClient(modelName);

            //List<ChatMessage> messages = [new UserChatMessage("I want to go to the nearest Bethany's Pie Shop. What will the weather be like there?")];
            List<ChatMessage> messages = [new UserChatMessage("I want to go to the nearest Bethany's Pie Shop. What should I wear in terms of clothes?")];

            ChatCompletionOptions options = new()
            {
                Tools = { getClosestBethanysPieShopStoreTool, getWeatherAtBethanysPieShopStoreTool },
            };

            while (requiresAction)
            {
                requiresAction = false;
                ChatCompletion response = chatClient.CompleteChat(messages, options);

                switch (response.FinishReason)
                {
                    case ChatFinishReason.ToolCalls:
                        {
                            messages.Add(new AssistantChatMessage(response));

                            foreach (ChatToolCall toolCall in response.ToolCalls)
                            {
                                switch (toolCall.FunctionName)
                                {
                                    case nameof(GetClosestBethanysPieShopStore):
                                        {
                                            string toolResult = GetClosestBethanysPieShopStore();
                                            messages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                            break;
                                        }

                                    case nameof(GetWeatherAtBethanysPieShopStore):
                                        {
                                            using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);
                                            bool hasLocation = argumentsJson.RootElement.TryGetProperty("location", out JsonElement location);
                                            bool hasUnit = argumentsJson.RootElement.TryGetProperty("unit", out JsonElement unit);

                                            if (!hasLocation)
                                            {
                                                throw new ArgumentNullException(nameof(location), "The location argument is required.");
                                            }

                                            string toolResult = hasUnit
                                                ? GetWeatherAtBethanysPieShopStore(location.GetString(), unit.GetString())
                                                : GetWeatherAtBethanysPieShopStore(location.GetString());

                                            messages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                            break;
                                        }

                                    default:
                                        {
                                            throw new NotImplementedException();
                                        }
                                }
                            }

                            requiresAction = true;
                            break;
                        }

                    case ChatFinishReason.Stop:
                        {
                            messages.Add(new AssistantChatMessage(response));
                            break;
                        }

                    case ChatFinishReason.Length:
                        throw new NotImplementedException("Incomplete model output due to MaxTokens parameter or token limit exceeded.");

                    case ChatFinishReason.ContentFilter:
                        throw new NotImplementedException("Omitted content due to a content filter flag.");

                    case ChatFinishReason.FunctionCall:
                        throw new NotImplementedException("Deprecated in favor of tool calls.");

                    default:
                        throw new NotImplementedException(response.FinishReason.ToString());
                }
            }

            foreach (ChatMessage message in messages)
            {
                if (message.Content.Count > 0)
                    Console.WriteLine(message.Content[0].Text);
            }
        }

        private static string GetClosestBethanysPieShopStore()
        {
            //this application can now get the current location of the user and return the closest Bethany's Pie Shop store
            //for now, we will return a hardcoded location
            return "Brussels, Belgium";
        }

        private static string GetWeatherAtBethanysPieShopStore(string location, string unit = "celsius")
        {
            //this could come from an external weather API
            return $"25 {unit} and raining. It's Belgium.";
        }
    }
}
