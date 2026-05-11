using BethanysPieShop.Contracts.Services;
using BethanysPieShop.Util;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.Text.Json;

namespace BethanysPieShop.Components.Admin.Pages
{
    public partial class SalesBot
    {

        private ChatClient chatClient;

        [Inject]
        public OpenAIClient OpenAIClient { get; set; }

        [Inject]
        private IOrderDataService OrderDataService { get; set; }

        [Inject]
        public IOptions<ModelSettings> ModelSettings { get; set; }

        protected string Message = string.Empty;
        protected string Question = string.Empty;
        protected bool IsSaved = false;

        override protected void OnInitialized()
        {
            chatClient = OpenAIClient.GetChatClient(ModelSettings.Value.TextModelName);
        }

        private async Task OnEnterQuestion()
        {
            bool requiresAction = true;

            List<ChatMessage> messages = [new SystemChatMessage("You are a sales bot, helping the people at Bethany's Pie Shop understand their sales and you are able to provide insights about sales of pies over time."),
            new UserChatMessage(Question)
            ];

            ChatCompletionOptions options = new()
            {
                Tools = { getNumberOfPiesSoldInGivenMonthAndYearTool },
            };

            while (requiresAction)
            {
                requiresAction = false;
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
                                    case nameof(GetNumberOfPiesSoldInGivenMonthAndYear):
                                        {
                                            using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);
                                            bool hasMonth = argumentsJson.RootElement.TryGetProperty("month", out JsonElement month);
                                            bool hasYear = argumentsJson.RootElement.TryGetProperty("year", out JsonElement year);

                                            if (!hasMonth)
                                            {
                                                throw new ArgumentNullException(nameof(month), "The month argument is required.");
                                            }

                                            if (!hasYear)
                                            {
                                                throw new ArgumentNullException(nameof(year), "The year argument is required.");
                                            }

                                            int toolResult = await GetNumberOfPiesSoldInGivenMonthAndYear(month.GetString(), year.GetString());

                                            messages.Add(new ToolChatMessage(toolCall.Id, toolResult.ToString()));
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

                    default:
                        throw new NotImplementedException(response.FinishReason.ToString());
                }
            }

            foreach (ChatMessage message in messages)
            {
                if (message.Content.Count > 0)
                {
                    if (message is AssistantChatMessage)
                        Message += message.Content[0].Text;
                }
            }
        }

        //todo: move to a separate class
        private static readonly ChatTool getNumberOfPiesSoldInGivenMonthAndYearTool = ChatTool.CreateFunctionTool(
            functionName: nameof(GetNumberOfPiesSoldInGivenMonthAndYear),
            functionDescription: "Get the sales of pies in absolute number of items sold at Bethany's Pie Shop for a given month (as an number between 1 and 12) and year as a number for all pies taken together. If you need to compare months, you need to call this function multiple times with different value for month and year so that you get the sales for the given period. If -1 is returned, tell the user that we couldn't find sales for the given period.",
            functionParameters: BinaryData.FromString("""
            {
                "type": "object",
                "properties": {
                    "month": {
                        "type": "string",
                        "description": "The month we want to get the sales from"
                    },
                    "year": {
                        "type": "string",
                        "description": "The year we want to get the sales from."
                    }
                },
                "required": [ "month", "year" ]
            }
            """)
        );

        private async Task<int> GetNumberOfPiesSoldInGivenMonthAndYear(string month, string year)
        {
            var orders = (await OrderDataService.GetAllOrdersForMonthAndYear(int.Parse(month), int.Parse(year))).ToList();

            if (orders.Count == 0)
            {
                return -1;
            }

            int sum = 0;
            foreach (var orderLine in orders.SelectMany(order => order.OrderLines))
            {
                sum += orderLine.Amount;
            }

            return sum;
        }
    }
}
