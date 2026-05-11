using BethanysPieShop.Contracts.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;

namespace BethanysPieShop.Components.Admin.Pages
{
    public partial class SalesBot
    {
        [Inject]
        public Kernel Kernel { get; set; }

        protected string Message = string.Empty;
        protected string Question = string.Empty;
        protected bool IsSaved = false;
        private IChatCompletionService chatCompletionService;
        private OpenAIPromptExecutionSettings settings;

        override protected void OnInitialized()
        {
            chatCompletionService = Kernel.GetRequiredService<IChatCompletionService>();

            Kernel.ImportPluginFromType<SalesInformationPlugin>();
            settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
        }

        private async Task OnEnterQuestion()
        {
            ChatHistory chatHistory = new();

            chatHistory.AddSystemMessage("You are a sales bot, helping the people at Bethany's Pie Shop understand their sales and you are able to provide insights about sales of pies over time.");
            chatHistory.AddUserMessage(Question);

            var assistantMessage = await chatCompletionService.GetChatMessageContentAsync(chatHistory, settings, Kernel);
            Message = assistantMessage.Content;
            StateHasChanged();
        }
    }

    public class SalesInformationPlugin
    {
        private IOrderDataService _orderDataService;

        public SalesInformationPlugin(IOrderDataService orderDataService)
        {
            _orderDataService = orderDataService;
        }

        [KernelFunction]
        [Description("Get the sales of pies in absolute number of items sold at Bethany's Pie Shop for a given month (as an number between 1 and 12) and year as a number for all pies taken together. If you need to compare months, you need to call this function multiple times with different value for month and year so that you get the sales for the given period. If -1 is returned, tell the user that we couldn't find sales for the given period.")]
        public async Task<int> GetNumberOfPiesSoldInGivenMonthAndYear(string month, string year)
        {
            var orders = (await _orderDataService.GetAllOrdersForMonthAndYear(int.Parse(month), int.Parse(year))).ToList();

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
