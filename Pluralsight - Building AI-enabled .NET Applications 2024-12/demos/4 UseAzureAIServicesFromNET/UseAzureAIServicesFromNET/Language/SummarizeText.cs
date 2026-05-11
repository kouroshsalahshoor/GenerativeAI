using Azure.AI.TextAnalytics;
using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UseAzureAIServicesFromNET.Language
{
    public static class SummarizeText
    {
        public static async Task SummarizeContentAsync()
        {
            string languageApiKey = Environment.GetEnvironmentVariable("LANGUAGE_KEY");
            string languageApiEndpoint = Environment.GetEnvironmentVariable("LANGUAGE_ENDPOINT");

            var keyCredentials = new AzureKeyCredential(languageApiKey);
            var endpointUri = new Uri(languageApiEndpoint);

            var textAnalyticsClient = new TextAnalyticsClient(endpointUri, keyCredentials);

            var storyContent = "Nestled in the heart of Brussels, Belgium, Bethany's Pie Shop has been a beloved local gem since its founding in 1997. The story of this charming pie store begins with Bethany DeWitt, a passionate baker with a dream of bringing her family's cherished pie recipes to life in a bustling European city. Growing up in a small village in Flanders, Bethany learned the art of pie-making from her grandmother, whose pies were legendary in their community. Inspired by the rich flavors and time-honored techniques passed down through generations, Bethany decided to share her family's culinary heritage with the world.\r\n\r\nWith a suitcase full of recipes and a heart full of hope, Bethany moved to Brussels and opened her quaint shop on a picturesque cobblestone street near the Grand Place. From day one, the shop was a hit. Locals and tourists alike were drawn to the shop's warm, inviting atmosphere and the mouthwatering aroma of freshly baked pies wafting through the air. Bethany's dedication to using only the finest, locally sourced ingredients set her pies apart. She made everything from scratch, from the flaky, buttery crusts to the rich, decadent fillings. Her menu featured a delightful mix of sweet and savory pies, from classic apple and cherry to innovative creations like Belgian chocolate pecan and leek and gruyère.\r\n\r\nOver the years, Bethany's Pie Shop has grown from a small, humble bakery into a cherished institution. Bethany herself has become a local legend, known for her warm hospitality and unwavering commitment to quality. Today, the shop remains family-owned, with Bethany's daughter, Clara, taking the reins to carry on her mother's legacy. The shop continues to delight customers with its delicious pies and welcoming atmosphere, making every visit to Bethany's Pie Shop a truly memorable experience in the heart of Brussels.";

            var documents = new List<string> { storyContent };

            ExtractiveSummarizeOperation summaryOperation = textAnalyticsClient.ExtractiveSummarize(WaitUntil.Completed, documents);

            await foreach (var summaryPage in summaryOperation.Value)
            {
                foreach (var summary in summaryPage)
                {
                    Console.WriteLine($"Summary extracted with {summary.Sentences.Count} sentence(s):");
                    Console.WriteLine();

                    foreach (var sentence in summary.Sentences)
                    {
                        Console.WriteLine($" - {sentence.Text}");
                    }
                }
            }
        }


    }
}
