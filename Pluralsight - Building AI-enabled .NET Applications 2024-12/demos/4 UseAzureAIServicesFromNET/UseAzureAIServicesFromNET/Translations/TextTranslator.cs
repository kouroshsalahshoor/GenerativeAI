using Azure;
using Azure.AI.Translation.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UseAzureAIServicesFromNET.Translations
{
    public static class TextTranslator
    {
        public static async Task PerformTextTranslationAsync()
        {
            string translationApiKey = Environment.GetEnvironmentVariable("TRANSLATION_KEY");
            string translationApiRegion = Environment.GetEnvironmentVariable("TRANSLATION_REGION");

            var credentials = new AzureKeyCredential(translationApiKey);

            var translationClient = new TextTranslationClient(credentials, translationApiRegion);

            string targetLanguage = "nl";
            string sourceText = "This cheesecake is a classic dessert with a rich, creamy filling and a buttery graham cracker crust. The smooth, velvety texture of the cream cheese blend is perfectly balanced with a hint of vanilla and a slight tang from sour cream, creating a delightful flavor profile. The crust, made from crushed graham crackers and melted butter, provides a sweet, crunchy base that complements the silky filling. Baked to perfection and chilled until set, this cheesecake is ideal for any occasion. Top it with fresh berries or a drizzle of fruit preserves for an added burst of flavor and a beautiful presentation.";

            var translationResponse = await translationClient.TranslateAsync(targetLanguage, sourceText);
            var translatedItems = translationResponse.Value;
            var translationResult = translatedItems.FirstOrDefault();

            Console.WriteLine($"Detected original language: {translationResult?.DetectedLanguage?.Language} with confidence level {translationResult?.DetectedLanguage?.Confidence}.");
            Console.WriteLine($"Translated text (Dutch): '{translationResult?.Translations?.FirstOrDefault()?.Text}'.");
        }

    }
}
