using OpenAI.Chat;

namespace UseOpenAIFromNET
{
    public static class Vision
    {
        public static void DescribeImage(string modelName)
        {
            ChatClient client = new(modelName, Environment.GetEnvironmentVariable("OPENAI_API_KEY"));

            string path = Path.Combine("Images", "waterfall.png");
            using Stream stream = File.OpenRead(path);

            BinaryData imageBytes = BinaryData.FromStream(stream);

            List<ChatMessage> messages = [new UserChatMessage(ChatMessageContentPart.CreateTextPart("Describe what you see in this image."),
                ChatMessageContentPart.CreateImagePart(imageBytes, "image/png"))
            ];

            ChatCompletion chatCompletion = client.CompleteChat(messages);

            Console.WriteLine($"[ASSISTANT]:");
            Console.WriteLine($"{chatCompletion.Content[0].Text}");
        }
    }
}
