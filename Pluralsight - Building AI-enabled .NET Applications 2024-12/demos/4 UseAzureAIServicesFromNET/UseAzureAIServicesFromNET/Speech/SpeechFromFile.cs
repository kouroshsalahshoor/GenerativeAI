using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace UseAzureAIServicesFromNET.Speech
{
    public static class SpeechFromFile
    {
        public static async Task ExecuteSpeechRecognitionAsync()
        {
            string apiKey = Environment.GetEnvironmentVariable("SPEECH_KEY");
            string apiRegion = Environment.GetEnvironmentVariable("SPEECH_REGION");

            RecognitionResult? recognitionOutcome = null;
            var speechConfig = SpeechConfig.FromSubscription(apiKey, apiRegion);
            speechConfig.SpeechRecognitionLanguage = "en-US";

            using (var audioSource = AudioConfig.FromWavFileInput("gill.wav"))
            {
                using (var speechRecognizer = new SpeechRecognizer(speechConfig, audioSource))
                {
                    recognitionOutcome = await speechRecognizer.RecognizeOnceAsync().ConfigureAwait(false);
                }
            }

            if (recognitionOutcome != null)
            {
                Console.WriteLine($"Speech recognized: {recognitionOutcome.Text}");
            }
            else
            {
                Console.WriteLine("No speech could be recognized from the audio.");
            }
        }


    }
}
