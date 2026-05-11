using BethanysPieShop.Contracts.Services;
using BethanysPieShop.Util;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace BethanysPieShop.Components.Admin.Pages
{
    public partial class PieCreator
    {
        private ChatClient chatClient;

        [Inject]
        public OpenAIClient OpenAIClient { get; set; }

        [Inject]
        private IPieRecipeDataService PieRecipeDataService{ get; set; }

        [Inject]
        public IOptions<ModelSettings> ModelSettings { get; set; }

        protected string Message = string.Empty;
        protected string Ingredients = string.Empty;
        protected bool IsSaved = false;

        override protected void OnInitialized()
        {
            chatClient = OpenAIClient.GetChatClient(ModelSettings.Value.TextModelName);
        }

        private async Task OnEnterIngredients()
        {
            Message = string.Empty;
            var json = await PieRecipeDataService.GetAllPieRecipesAsJson();

            var prompt = $$"""
                    You are an assistant to help the chefs at Bethany's Pie Shop, a store that sells the most delicious pies.
                    You will be given a list of recipes from Bethany, together with a list of ingredients for each recipe.

                    Your task is, based on the list of available ingredients, to check which pies can be made with the ingredients provided. All ingredients in the recipe need to be available, if one or more is missing, it's not an option to bake the pie. The amounts also need to be sufficient.
                    You can ONLY use the recipes that are available in the list. You can't make up other recipes. If you find one or more pies that can be made, list them. If you can't make any pies, say "I can't find anything we can bake with these ingredients". 

                    Here is the list of recipes and ingredients:

                    Recipes: {{json}}

                    The list of avaialble ingredients is: {{Ingredients}}
                    """;

            AsyncCollectionResult<StreamingChatCompletionUpdate> completionUpdates = chatClient.CompleteChatStreamingAsync(prompt);

            await foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
            {
                foreach (ChatMessageContentPart contentPart in completionUpdate.ContentUpdate)
                {
                    Message += contentPart.Text;
                    StateHasChanged();
                }
            }

        }
    }
}
