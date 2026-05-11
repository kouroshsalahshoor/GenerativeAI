using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextToImage;

namespace UseSemanticKernelFromNET
{
    public class ImageGeneration
    {
        public async Task GenerateBasicImage(string modelName)
        {
            Kernel kernel = Kernel.CreateBuilder().AddOpenAITextToImage(modelId:modelName, apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")).Build();

            ITextToImageService imageService = kernel.GetRequiredService<ITextToImageService>();

            string prompt =
               """
                   A cozy and inviting pie shop that blends the warmth of rustic charm with the elegance of modern design, creating a space that feels both nostalgic and contemporary. The ambiance is centered around a palette of warm, earthy tones—think rich browns, soft creams, and muted greens—complemented by natural wood finishes that exude a sense of comfort and tradition.

               The shop features handcrafted wooden tables with subtle details, surrounded by comfortable seating that encourages customers to linger and enjoy their treats. Large windows allow natural light to flood the space, creating an airy and welcoming environment.

               On the walls, you’ll find a mix of vintage-inspired artwork and shelves displaying artisanal products and fresh flowers, which add a personal touch and a connection to nature. The counter, where pies are displayed, is the focal point, with a clean, minimalist design that highlights the vibrant colors and textures of the baked goods.

               Incorporating soft textiles, such as woven rugs and cushions in natural fabrics, adds warmth and coziness, making the space feel like a home away from home. The overall atmosphere invites customers to relax, savor their pie, and enjoy a moment of simple pleasure in a beautifully crafted setting.
               """;

            var image = await imageService.GenerateImageAsync(prompt, 1792, 1024);

            Console.WriteLine("Image URL: " + image);


        }
    }
}
 