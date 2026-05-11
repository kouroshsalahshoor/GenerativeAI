using UseOpenAIFromNET;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder();
builder.SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

IConfiguration config = builder.Build();

var modelName = config["modelName"];
var imageModel = config["imageModelName"];
var audioModel = config["audioModelName"];

//1. Connect with OpenAI using Http
//await ConnectWithHttp.Run();

//2. Chat completions
//ChatCompletions.SimpleChat(modelName);
//ChatCompletions.SimpleChatUsingOpenAIClient(modelName);
//await ChatCompletions.SimpleChatAsync(modelName);
//await ChatCompletions.SimpleChatStreamingAsync(modelName);
//await ChatCompletions.SimpleChatUsingOpenAIClientWithMessages(modelName);
//ChatCompletions.SimpleChatStreamingUsingOpenAIClientWithMessages(modelName);
// await ChatCompletions.OpenEndedChatPromptOptimizedStreamingAsync(modelName);
//ChatCompletions.CreatePieDescription(modelName);
//ChatCompletions.CreateBetterPieName(modelName);
//ChatCompletions.CreatePoshAndFancyPieDescription(modelName);
//ChatCompletions.CreateIngredientTable(modelName);
//ChatCompletions.CreatePieDescriptionWithOptions(modelName);
//await ChatCompletions.OpenEndedChatStreamingAsync(modelName);

//3. Image generations
//ImageGenerations.GenerateImage(imageModel);
//await ImageGenerations.GenerateImageEdit(imageModel);
//ImageGenerations.GenerateImageVariation(imageModel);

//4. Audio transcriptions
//AudioTransciptions.TranscribeAudio(audioModel);

//5. Function calling
FunctionCalling.SimpleFunctionCalling(modelName);

//6. Chatbot with function calling  
//await ChatBotWithFunctionCalling.OpenEndedChatAsync(modelName);

//7. Vision chat completions
//Vision.DescribeImage(modelName);

//8. Image variations
//ImageGenerations.GenerateImageVariation(imageModel);

//9. Assistants
//Assistants.WorkWithAssistant(modelName);

//AssistantsChat.OpenEndedChatAsync(modelName);

Console.WriteLine();

Console.WriteLine("All done");

Console.WriteLine();
