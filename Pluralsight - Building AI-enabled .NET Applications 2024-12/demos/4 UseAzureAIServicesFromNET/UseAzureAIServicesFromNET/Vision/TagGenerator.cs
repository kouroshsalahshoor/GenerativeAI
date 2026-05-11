using Azure;
using Azure.AI.Vision.ImageAnalysis;

namespace UseAzureAIServicesFromNET.Vision
{
    public static class TagGenerator
    {
        public static void ExtractImageTags()
        {
            string visionApiKey = Environment.GetEnvironmentVariable("VISION_KEY");
            string visionApiEndpoint = Environment.GetEnvironmentVariable("VISION_ENDPOINT");

            var credentials = new AzureKeyCredential(visionApiKey);
            var serviceUri = new Uri(visionApiEndpoint);

            var imageAnalysisClient = new ImageAnalysisClient(serviceUri, credentials);

            using var imageStream = new FileStream("Images/Store.png", FileMode.Open);

            ImageAnalysisResult analysisResult = imageAnalysisClient.Analyze(BinaryData.FromStream(imageStream), VisualFeatures.Tags);

            foreach (var tag in analysisResult.Tags.Values)
            {
                Console.WriteLine($"Tag: '{tag.Name}', Confidence: {tag.Confidence:F4}");
            }
        }

    }
}
