using Microsoft.CognitiveServices.Speech;

namespace UseAzureAIServicesFromNET.Speech
{
    public static class SpeechFromMicrophone
    {
        public static async Task CaptureSpeechAsync()
        {
            string apiKey = Environment.GetEnvironmentVariable("SPEECH_KEY");
            string apiRegion = Environment.GetEnvironmentVariable("SPEECH_REGION");

            var speechConfig = SpeechConfig.FromSubscription(apiKey, apiRegion);
            speechConfig.SpeechRecognitionLanguage = "en-US";

            using var speechRecognizer = new SpeechRecognizer(speechConfig);

            Console.WriteLine("Please speak into the microphone...");

            var recognitionResult = await speechRecognizer.RecognizeOnceAsync().ConfigureAwait(false);

            switch (recognitionResult.Reason)
            {
                case ResultReason.RecognizedSpeech:
                    Console.WriteLine($"Recognized speech: \"{recognitionResult.Text}\"");
                    break;

                case ResultReason.NoMatch:
                    Console.WriteLine("Sorry, I couldn't recognize any speech.");
                    break;

                case ResultReason.Canceled:
                    var cancelDetails = CancellationDetails.FromResult(recognitionResult);
                    Console.WriteLine($"Operation canceled: {cancelDetails.Reason}");
                    break;

                default:
                    Console.WriteLine("Unexpected outcome during speech recognition.");
                    break;
            }
        }

    }
}
