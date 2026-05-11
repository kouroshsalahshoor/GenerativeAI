using Azure;
using Azure.AI.TextAnalytics;
using static System.Net.Mime.MediaTypeNames;

namespace UseAzureAIServicesFromNET.Language
{
    public static class EntityRecognizer
    {
        public static void AnalyzeEntities()
        {
            string languageApiKey = Environment.GetEnvironmentVariable("LANGUAGE_KEY");
            string languageApiEndpoint = Environment.GetEnvironmentVariable("LANGUAGE_ENDPOINT");

            var keyCredentials = new AzureKeyCredential(languageApiKey);
            var endpointUri = new Uri(languageApiEndpoint);

            var textAnalyticsClient = new TextAnalyticsClient(endpointUri, keyCredentials);

            string description = "This cheesecake is a classic dessert with a rich, creamy filling and a buttery graham cracker crust. The smooth, velvety texture of the cream cheese blend is perfectly balanced with a hint of vanilla and a slight tang from sour cream, creating a delightful flavor profile. The crust, made from crushed graham crackers and melted butter, provides a sweet, crunchy base that complements the silky filling. Baked to perfection and chilled until set, this cheesecake is ideal for any occasion. Top it with fresh berries or a drizzle of fruit preserves for an added burst of flavor and a beautiful presentation.";

            CategorizedEntityCollection extractedEntities = textAnalyticsClient.RecognizeEntities(description);

            if (extractedEntities.Count > 0)
            {
                Console.WriteLine("\nExtracted Entities:");
                foreach (var entity in extractedEntities)
                {
                    Console.WriteLine($"Text: {entity.Text}");
                    Console.WriteLine($"Entity Length: {entity.Length}");
                    Console.WriteLine($"Category: {entity.Category}");
                    if (!string.IsNullOrWhiteSpace(entity.SubCategory))
                        Console.WriteLine($"Subcategory: {entity.SubCategory}");
                    Console.WriteLine($"Confidence: {entity.ConfidenceScore}");
                    Console.WriteLine();
                }
            }
        }


    }
}
