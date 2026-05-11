using OpenAI;
using OpenAI.Chat;
using System.Text.Json;

namespace UseOpenAIFromNET
{
    //Demo 11

    public static class ChatBotWithFunctionCalling
    {
        private static readonly ChatTool getOrderStatusTool = ChatTool.CreateFunctionTool(
            functionName: nameof(GetOrderStatus),
            functionDescription: "Get the status of the order based on the order number.",
            functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "orderNumber": {
                    "type": "string",
                    "description": "The order number"
                }
            },
            "required": [ "orderNumber" ]
        }
        """)
        );

        private static readonly ChatTool sendEmailTool = ChatTool.CreateFunctionTool(
            functionName: nameof(SendEmail),
            functionDescription: "Send an email about the order to our support department.",
            functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "message": {
                    "type": "string",
                    "description": "The message to send"
                }
            },
            "required": [ "message" ]
        }
        """)
        );


        public async static Task OpenEndedChatAsync(string modelName)
        {
            OpenAIClient openAiClient = new(Environment.GetEnvironmentVariable("OPENAI_API_KEY"));

            ChatClient chatClient = openAiClient.GetChatClient(modelName);

            List<ChatMessage> messages =
            [
                new SystemChatMessage("You are a helpful chat bot for Bethany's Pie Shop, a pie store in Brussels, Belgium. You will try to help customers with their questions. " +
                //"If the user asks to send an email, ask for the content they want to include in the message to send. When they ask information about an order, ask them about the order number." +
                ""),
                new AssistantChatMessage("Welcome to Bethany's Pie Shop. How can I help you today?"),
            ];

            ChatCompletionOptions options = new()
            {
                Tools = { sendEmailTool, getOrderStatusTool},
            };

            while (true)
            {
                Console.WriteLine($"\n[ASSISTANT]: {messages[^1].Content[0].Text}\n");

                Console.Write("You: ");

                var input = Console.ReadLine()!;

                if (string.IsNullOrEmpty(input))
                {
                    break;
                }

                messages.Add(new UserChatMessage(input));

                ChatCompletion response = await chatClient.CompleteChatAsync(messages, options);


                switch (response.FinishReason)
                {
                    case ChatFinishReason.ToolCalls:
                        {
                            messages.Add(new AssistantChatMessage(response));

                            foreach (ChatToolCall toolCall in response.ToolCalls)
                            {
                                switch (toolCall.FunctionName)
                                {
                                    case nameof(SendEmail):
                                        {
                                            using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);
                                            bool hasMessage = argumentsJson.RootElement.TryGetProperty("message", out JsonElement message);

                                            if (!hasMessage)
                                            {
                                                throw new ArgumentNullException(nameof(message), "The message argument is required.");
                                            }

                                            SendEmail(message.GetString());
                                                
                                            messages.Add(new ToolChatMessage(toolCall.Id, "Email to customer support sent!"));
                                            break;
                                        }

                                    case nameof(GetOrderStatus):
                                        {
                                            using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);
                                            bool hasOrderNumber = argumentsJson.RootElement.TryGetProperty("orderNumber", out JsonElement orderNumber);

                                            if (!hasOrderNumber)
                                            {
                                                throw new ArgumentNullException(nameof(orderNumber), "The order number argument is required.");
                                            }

                                            string toolResult = GetOrderStatus(orderNumber.GetString());

                                            messages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                            break;
                                        }

                                    default:
                                        {
                                            throw new NotImplementedException();
                                        }
                                }
                            }

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

            //foreach (ChatMessage message in messages)
            //{
            //    if (message.Content.Count > 0)
            //        Console.WriteLine(message.Content[0].Text);
            //}
        }

        private static string GetOrderStatus(string orderNumber)
        {
            int orderId = int.Parse(orderNumber);

            //write a switch statement to return the order status based on the order number. If the order number is even, return "Delivered". If the order number is odd, return "Processing". If the order number is less than 100, return "Pending". If the order number is greater than 100 but less than 200, return "Shipped". If the order number is greater than 200, return "Cancelled".    
            switch (orderNumber) 
            {
                case var _ when orderId % 2 == 0:
                    return OrderStatus.Delivered.ToString();
                case var _ when orderId % 2 != 0:
                    return OrderStatus.Processing.ToString();
                case var _ when orderId < 100:
                    return OrderStatus.Pending.ToString();
                case var _ when orderId > 100 && orderId < 200:
                    return OrderStatus.Shipped.ToString();
                case var _ when orderId > 200:
                    return OrderStatus.Cancelled.ToString();
                default:
                    return OrderStatus.Pending.ToString();
            }
        }

        private static void SendEmail( string message)
        {
            //write code to send an email to the given email address with the given message
            Console.WriteLine($"Email sent to support with message: {message}");
        }

    }

    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled
    }
}
