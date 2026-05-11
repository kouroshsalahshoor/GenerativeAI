using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace UseSemanticKernelFromNET
{
    public class BasicsOfSK
    {
        public async Task SimplestPromptLoop(string modelName)
        {
            Kernel kernel = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId: modelName, apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")).Build();

            string response = string.Empty;

            while (response != "quit")
            {
                Console.WriteLine("Enter your message:");
                response = Console.ReadLine();
                Console.WriteLine(await kernel.InvokePromptAsync(response));
            }
        }

        public async Task AddingMessageHistory(string modelName)
        {
            Kernel kernel = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId: modelName, apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")).Build();

            string response = string.Empty;

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            ChatHistory chatHistory = new();

            while (response != "quit")
            {
                Console.WriteLine("Enter your message:");
                response = Console.ReadLine();
                chatHistory.AddUserMessage(response);

                var assistantMessage = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
                Console.WriteLine(assistantMessage);
                chatHistory.Add(assistantMessage);
            }
        }

        public async Task SimpleContentStreaming(string modelName)
        {
            Kernel kernel = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId: modelName, apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")).Build();

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            ChatHistory chatHistory = new("You are a pie creator and you should come up with magnificient and delicious recipes for pies.");

            chatHistory.AddAssistantMessage("Welcome to the pie creator chat. How can I help you today?");
            var message = chatHistory.Last();
            Console.WriteLine($"{message.Role}: {message.Content}");

            chatHistory.AddUserMessage("I want to create a pumpkin pie.");
            message = chatHistory.Last();
            Console.WriteLine($"{message.Role}: {message.Content}");

            await foreach (StreamingChatMessageContent chatUpdate in chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory))
            {
                Console.Write(chatUpdate.Content);
            }

        }

        public async Task ChangeOpenAISettings(string modelName)
        {

            Kernel kernel = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId: modelName, apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")).Build();

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            KernelArguments arguments = new(new OpenAIPromptExecutionSettings { MaxTokens = 500, Temperature = 0 });
            Console.WriteLine("Temperature 0:");
            Console.WriteLine(await kernel.InvokePromptAsync("Tell me a story about Bethany's Pie Shop, a pie store located in Brussels which is known for its delicious pies", arguments));

            arguments = new(new OpenAIPromptExecutionSettings { MaxTokens = 500, Temperature = 1 });
            Console.WriteLine("Temperature 1:");
            Console.WriteLine(await kernel.InvokePromptAsync("Tell me a story about Bethany's Pie Shop, a pie store located in Brussels which is known for its delicious pies", arguments));
        }
    }
}
