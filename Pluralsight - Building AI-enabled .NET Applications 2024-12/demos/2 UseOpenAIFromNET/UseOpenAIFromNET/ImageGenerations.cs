using OpenAI.Images;

namespace UseOpenAIFromNET
{
    public static class ImageGenerations
    {
        public static void GenerateImage(string modelName)
        {
            ImageClient client = new(modelName, Environment.GetEnvironmentVariable("OPENAI_API_KEY"));


            string prompt =
                """
                A cozy and inviting pie shop that blends the warmth of rustic charm with the elegance of modern design, creating a space that feels both nostalgic and contemporary. The ambiance is centered around a palette of warm, earthy tones—think rich browns, soft creams, and muted greens—complemented by natural wood finishes that exude a sense of comfort and tradition.

                The shop features handcrafted wooden tables with subtle details, surrounded by comfortable seating that encourages customers to linger and enjoy their treats. Large windows allow natural light to flood the space, creating an airy and welcoming environment.

                On the walls, you’ll find a mix of vintage-inspired artwork and shelves displaying artisanal products and fresh flowers, which add a personal touch and a connection to nature. The counter, where pies are displayed, is the focal point, with a clean, minimalist design that highlights the vibrant colors and textures of the baked goods.

                Incorporating soft textiles, such as woven rugs and cushions in natural fabrics, adds warmth and coziness, making the space feel like a home away from home. The overall atmosphere invites customers to relax, savor their pie, and enjoy a moment of simple pleasure in a beautifully crafted setting.
               
               """;

            Console.WriteLine("Generating image...");

            ImageGenerationOptions imageGenerationOptions = new()
            {
                Quality = GeneratedImageQuality.High,
                Size = GeneratedImageSize.W1792xH1024,
                Style = GeneratedImageStyle.Vivid,
                ResponseFormat = GeneratedImageFormat.Bytes,
            };

            GeneratedImage generatedImage = client.GenerateImage(prompt, imageGenerationOptions);
            BinaryData bytes = generatedImage.ImageBytes;

            using FileStream stream = File.OpenWrite($"{Guid.NewGuid()}.png");
            bytes.ToStream().CopyTo(stream);
        }

        //Use Dalle-2 for this one since we generate 2 images
        public async static Task GenerateImageAsync(string modelName)
        {
            ImageClient client = new(modelName, Environment.GetEnvironmentVariable("OPENAI_API_KEY"));


            string prompt =
                """
                  A cozy and inviting pie shop that blends the warmth of rustic charm with the elegance of modern design, creating a space that feels both nostalgic and contemporary. The ambiance is centered around a palette of warm, earthy tones—think rich browns, soft creams, and muted greens—complemented by natural wood finishes that exude a sense of comfort and tradition. The shop features handcrafted wooden tables with subtle details, surrounded by comfortable seating that encourages customers to linger and enjoy their treats. Large windows allow natural light to flood the space, creating an airy and welcoming environment. On the walls, you’ll find a mix of vintage-inspired artwork and shelves displaying artisanal products and fresh flowers, which add a personal touch and a connection to nature. The counter, where pies are displayed, is the focal point, with a clean, minimalist design that highlights the vibrant colors and textures of the baked goods.
               """;

            Console.WriteLine("Generating image...");

            ImageGenerationOptions imageGenerationOptions = new()
            {
                Quality = GeneratedImageQuality.High,
                Size = GeneratedImageSize.W1024xH1024,
                Style = GeneratedImageStyle.Vivid,
                ResponseFormat = GeneratedImageFormat.Bytes,
            };

            var imageCollection = await client.GenerateImagesAsync(prompt, 2, imageGenerationOptions);

            foreach (var image in imageCollection.Value)
            {
                BinaryData bytes = image.ImageBytes;

                using FileStream stream = File.OpenWrite($"{Guid.NewGuid()}.png");
                bytes.ToStream().CopyTo(stream);
            }
        }

        public async static Task GenerateImageEdit(string modelName)
        {
            ImageClient client = new(modelName, Environment.GetEnvironmentVariable("OPENAI_API_KEY"));

            FileStream fileStream = File.OpenRead("street2.png");
            string prompt =
                """
                    Add a pumpkin pie to the image  
               """;

            Console.WriteLine("Generating image...");

            ImageEditOptions imageEditOptions = new()
            {
                ResponseFormat = GeneratedImageFormat.Bytes
            };

            var imageEdit = await client.GenerateImageEditAsync(fileStream, "street2.png", prompt, imageEditOptions);

            BinaryData bytes = imageEdit.Value.ImageBytes;

            using FileStream stream = File.OpenWrite($"{Guid.NewGuid()}.png");
            bytes.ToStream().CopyTo(stream);
        }

        public static void GenerateImageVariation(string modelName)
        {
            ImageClient client = new(modelName, Environment.GetEnvironmentVariable("OPENAI_API_KEY"));

            string image = "images/waterfall.png";

            ImageVariationOptions imageVariationOptions = new()
            {
                Size = GeneratedImageSize.W1024xH1024,
                ResponseFormat = GeneratedImageFormat.Bytes,
            };

            GeneratedImage variation = client.GenerateImageVariation(image, imageVariationOptions);
            BinaryData bytes = variation.ImageBytes;

            using FileStream stream = File.OpenWrite($"{Guid.NewGuid()}.png");
            bytes.ToStream().CopyTo(stream);
        }

    }
}
