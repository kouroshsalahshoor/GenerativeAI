using Microsoft.Extensions.Configuration;
using UseSemanticKernelFromNET;

var builder = new ConfigurationBuilder();
builder.SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

IConfiguration config = builder.Build();

var modelName = config["modelName"];
var imageModel = config["imageModelName"];


//1. Basics of SK
//await new BasicsOfSK().SimplestPromptLoop(modelName);
//await new BasicsOfSK().AddingMessageHistory(modelName);
//await new BasicsOfSK().SimpleContentStreaming(modelName);
//await new BasicsOfSK().ChangeOpenAISettings(modelName);

//2. Image generation
//await new ImageGeneration().GenerateBasicImage(imageModel);

//3. Plugins
//await new UsingPlugins().GetDaysUntilChristmas(modelName);
await new UsingPlugins().ChatWithMultiplePlugins(modelName);

