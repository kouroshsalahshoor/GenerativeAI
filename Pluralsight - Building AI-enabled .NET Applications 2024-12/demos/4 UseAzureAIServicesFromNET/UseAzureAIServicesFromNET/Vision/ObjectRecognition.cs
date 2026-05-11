using Azure;
using Azure.AI.Vision.ImageAnalysis;

namespace UseAzureAIServicesFromNET.Vision
{
    public static class ObjectRecognition
    {
        public static void DetectObjectsInImage()
        {
            string visionApiKey = Environment.GetEnvironmentVariable("VISION_KEY");
            string visionApiEndpoint = Environment.GetEnvironmentVariable("VISION_ENDPOINT");

            var credentials = new AzureKeyCredential(visionApiKey);
            var serviceUri = new Uri(visionApiEndpoint);

            var imageAnalysisClient = new ImageAnalysisClient(serviceUri, credentials);

            using var imageStream = new FileStream("Images/Store.png", FileMode.Open);

            ImageAnalysisResult analysisResult = imageAnalysisClient.Analyze(BinaryData.FromStream(imageStream), VisualFeatures.Objects);

            foreach (var detectedObject in analysisResult.Objects.Values)
            {
                Console.WriteLine($"Object: '{detectedObject.Tags.First().Name}', Bounding box: {detectedObject.BoundingBox}");
            }
        }

    }
}