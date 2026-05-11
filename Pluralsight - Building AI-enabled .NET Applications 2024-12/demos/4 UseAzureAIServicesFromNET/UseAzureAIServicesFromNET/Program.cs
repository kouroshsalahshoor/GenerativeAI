using Microsoft.Extensions.Configuration;
using UseAzureAIServicesFromNET.Language;
using UseAzureAIServicesFromNET.Speech;
using UseAzureAIServicesFromNET.Translations;
using UseAzureAIServicesFromNET.Vision;

var builder = new ConfigurationBuilder();
builder.SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

//Language
//LanguageDetector
//LanguageDetector.DetectLanguage();

//SentimentAnalysis
//SentimentAnalysis.AnalyzeCustomerSentiment();

//EntityRecognition
//EntityRecognizer.AnalyzeEntities();

//SummarizeText
//await SummarizeText.SummarizeContentAsync();


//Vision
//Image caption generation
//ImageCaptionGenerator.CreateImageDescription();

//Tag generation
//TagGenerator.ExtractImageTags();

//Object recognition
//ObjectRecognition.DetectObjectsInImage();


//Speech
//Speech from file
//await SpeechFromFile.ExecuteSpeechRecognitionAsync();

//Speech from microphone
//await SpeechFromMicrophone.CaptureSpeechAsync();


//Translation
await TextTranslator.PerformTextTranslationAsync();

Console.ReadLine();
