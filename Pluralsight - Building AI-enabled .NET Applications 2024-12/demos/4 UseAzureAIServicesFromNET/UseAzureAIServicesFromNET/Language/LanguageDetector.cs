using Azure;
using Azure.AI.TextAnalytics;

namespace UseAzureAIServicesFromNET.Language
{
    public static class LanguageDetector
    {
        public static void DetectLanguage()
        {
            string languageApiKey = Environment.GetEnvironmentVariable("LANGUAGE_KEY");
            string languageApiEndpoint = Environment.GetEnvironmentVariable("LANGUAGE_ENDPOINT");

            var keyCredentials = new AzureKeyCredential(languageApiKey);
            var endpointUri = new Uri(languageApiEndpoint);

            var textAnalyticsClient = new TextAnalyticsClient(endpointUri, keyCredentials);

            string descriptionText = "Un cheesecake crémeux et onctueux, délicatement posé sur une base croustillante de biscuits Graham dorés et légèrement sucrés. Sa garniture riche et veloutée est confectionnée à partir de fromage frais, offrant une texture fondante et un goût subtilement acidulé. Pour parfaire cette douceur, le gâteau est nappé d'un coulis de fruits rouges frais, ajoutant une touche de fraîcheur et d'acidité.";

            DetectedLanguage identifiedLanguage = textAnalyticsClient.DetectLanguage(descriptionText);

            Console.WriteLine($"Provided Text: {descriptionText}");

            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.WriteLine($"Detected language: {identifiedLanguage.Name}");
        }
    }
}
