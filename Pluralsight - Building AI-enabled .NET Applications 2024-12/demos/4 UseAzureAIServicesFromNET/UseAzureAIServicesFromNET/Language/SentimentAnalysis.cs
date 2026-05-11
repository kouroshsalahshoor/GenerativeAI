using Azure;
using Azure.AI.TextAnalytics;

namespace UseAzureAIServicesFromNET.Language
{
    public static class SentimentAnalysis
    {
        public static void AnalyzeCustomerSentiment()
        {
            string languageApiKey = Environment.GetEnvironmentVariable("LANGUAGE_KEY");
            string languageApiEndpoint = Environment.GetEnvironmentVariable("LANGUAGE_ENDPOINT");

            var keyCredentials = new AzureKeyCredential(languageApiKey);
            var endpointUri = new Uri(languageApiEndpoint);

            var textAnalyticsClient = new TextAnalyticsClient(endpointUri, keyCredentials);

            string feedback =

                    "The cheesecake from this website was a huge disappointment. The texture was grainy, not creamy, and the flavor was bland with an overpowering artificial taste. The crust was soggy and stale. The packaging was inadequate, resulting in a damaged cake. Not worth the price. One positive note: the delivery was on time."
                ;

            DocumentSentiment documentSentiment = textAnalyticsClient.AnalyzeSentiment(feedback, options: new AnalyzeSentimentOptions());

            Console.WriteLine($"General sentiment: {documentSentiment.Sentiment}\n");
            Console.WriteLine($"\tPositive score: {documentSentiment.ConfidenceScores.Positive:0.00}");
            Console.WriteLine($"\tNegative score: {documentSentiment.ConfidenceScores.Negative:0.00}");
            Console.WriteLine($"\tNeutral score: {documentSentiment.ConfidenceScores.Neutral:0.00}\n");

            foreach (var sentence in documentSentiment.Sentences)
            {
                Console.WriteLine($"\tSentence: \"{sentence.Text}\"");
                Console.WriteLine($"\tSentiment: {sentence.Sentiment}");
                Console.WriteLine($"\tPositive score: {sentence.ConfidenceScores.Positive:0.00}");
                Console.WriteLine($"\tNegative score: {sentence.ConfidenceScores.Negative:0.00}");
                Console.WriteLine($"\tNeutral score: {sentence.ConfidenceScores.Neutral:0.00}\n");
           
            }
            Console.WriteLine();
        }

    }
}
