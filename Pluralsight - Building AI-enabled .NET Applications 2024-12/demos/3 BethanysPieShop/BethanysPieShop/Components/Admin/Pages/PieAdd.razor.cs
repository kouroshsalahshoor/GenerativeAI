using BethanysPieShop.Contracts.Services;
using BethanysPieShop.Model;
using BethanysPieShop.Util;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Images;
using System.ClientModel;

namespace BethanysPieShop.Components.Admin.Pages
{
    public partial class PieAdd
    {
        [SupplyParameterFromForm]
        public Pie Pie { get; set; }

        [Inject]
        public IPieDataService? PieDataService { get; set; }

        [Inject]
        public IOptions<ModelSettings> ModelSettings { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public OpenAIClient OpenAIClient { get; set; }

        private string Message = string.Empty;
        private bool Generating = false;
        private bool IsSaved = false;
        private string GeneratedImage = string.Empty;

        private ChatClient chatClient;
        private ImageClient imageClient;

        protected override void OnInitialized()
        {
            Pie ??= new();

            chatClient = OpenAIClient.GetChatClient(ModelSettings.Value.TextModelName);
            imageClient = OpenAIClient.GetImageClient(ModelSettings.Value.ImageModelName);
        }

        private async Task OnSubmit()
        {
            await PieDataService.AddPie(Pie);
            IsSaved = true;
            Message = "Pie added successfully";
        }

        private async Task GenerateShortDescription()
        {
            Pie.ShortDescription = string.Empty;//reset the short description

            string prompt = "Create a 20 word description for a " + Pie.Name + ". This is used on a website for Bethany's Pie Shop, an store that specializes in creating great pies.";

            AsyncCollectionResult<StreamingChatCompletionUpdate> completionUpdates = chatClient.CompleteChatStreamingAsync(prompt);

            await foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
            {
                foreach (ChatMessageContentPart contentPart in completionUpdate.ContentUpdate)
                {
                    Pie.ShortDescription += contentPart.Text;
                    StateHasChanged();
                }
            }
        }

        private async Task GenerateLongDescription()
        {
            Pie.LongDescription = string.Empty;//reset the long description

            string prompt = "Create a 100 word description for a " + Pie.Name + ". This is used on a website for Bethany's Pie Shop, an store that specializes in creating great pies.";

            AsyncCollectionResult<StreamingChatCompletionUpdate> completionUpdates = chatClient.CompleteChatStreamingAsync(prompt);

            await foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
            {
                foreach (ChatMessageContentPart contentPart in completionUpdate.ContentUpdate)
                {
                    Pie.LongDescription += contentPart.Text;
                    StateHasChanged();
                }
            }
        }

        private async Task GenerateImage()
        {
            Generating = true;

            Pie.ImageUrl = string.Empty;//reset the image url
            GeneratedImage = string.Empty;//reset the image in the UI

            StateHasChanged();

            string prompt = $"Create a highly detailed, photo-realistic and visually appealing image of a {Pie.Name} for a website that specializes in sellings pies. The pie should be the central focus, presented on a rustic wooden table with a soft, blurred background to emphasize the pie. Ensure the crust looks golden and flaky, with a slight sheen to convey freshness. For pies with fruit, like a cherry or apple pie, make sure the fruit filling appears juicy and glossy, with some filling visibly bubbling out. For pies like cheesecake, capture the smooth, creamy texture with a light dusting of powdered sugar or a drizzle of fruit syrup. The lighting should be warm and inviting, highlighting the textures and colors of the pie. Include subtle details like a small fork or a decorative napkin beside the pie for added realism. The overall composition should evoke a sense of homemade comfort and deliciousness, perfect for a website's food section. Also, take into account the generated long description that was already generated: {Pie.LongDescription}";

            ImageGenerationOptions imageGenerationOptions = new()
            {
                Quality = GeneratedImageQuality.High,
                Size = GeneratedImageSize.W1792xH1024,
                Style = GeneratedImageStyle.Vivid,
                ResponseFormat = GeneratedImageFormat.Bytes,
            };

            GeneratedImage generatedImage = await imageClient.GenerateImageAsync(prompt, imageGenerationOptions);
            BinaryData bytes = generatedImage.ImageBytes;
            var imageName = Guid.NewGuid().ToString();

            using FileStream stream = File.OpenWrite($"{Directory.GetCurrentDirectory()}/wwwroot/images/{imageName}.png");
            bytes.ToStream().CopyTo(stream);
            GeneratedImage = $"<img src='{NavigationManager.BaseUri}/images/{imageName}.png' width=500/>";
            Pie.ImageUrl = $"{NavigationManager.BaseUri}/images/{imageName}.png";

            Generating = false;
            StateHasChanged();
        }
    }
}
