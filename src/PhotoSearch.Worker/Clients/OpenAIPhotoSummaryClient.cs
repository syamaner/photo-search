using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using ImageMagick;
using OpenAI;
using OpenAI.Chat;
using PhotoSearch.Data.Models;

namespace PhotoSearch.Worker.Clients;

public class OpenAiPhotoSummaryClient: IPhotoSummaryClient
{
    private static readonly string[] SupportedModels = ["gpt-4o-mini", "gpt-3.5-turbo-instruct","gpt-4o-mini-2024-07-18","gpt-3.5-turbo"];

    private static readonly ConcurrentDictionary<string, OpenAIClient> OpenAiClients = new();
    private const string PromptSummary = "Please provide a detailed description of the attached photo as base64 string. When forming json response:"
        +" Summary should include the detailed summery of photo. "
        +" Objects property should be a list of identified objects or things in the photo. "
        +" Finally Categories should be a list of categories representing the given photo. \r\n";
    public async Task<PhotoSummary> SummarisePhoto(string modelName, string imagePath, string address)
    {
        
        var stopwath = new Stopwatch();
        if(!SupportedModels.Contains(modelName, StringComparer.OrdinalIgnoreCase))
        {
            throw new NotSupportedException($"Model {modelName} is not supported by this client");
        }
        using var image = new MagickImage(imagePath);
        var imageBytes = image.ToByteArray();
        var base64Image = Convert.ToBase64String(imageBytes);
        var client = GetClient(modelName);
        
        List<ChatMessage> messages = 
        [
            new UserChatMessage(PromptSummary + base64Image),
        ];
        var options = new ChatCompletionOptions()
        {
            Temperature = 0.1f,
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(jsonSchemaFormatName: "image_summary_result",
                jsonSchema: BinaryData.FromString("""
                                                  {
                                                      "type": "object",
                                                      "properties": {
                                                         "Summary": { "type": "string" },
                                                         "Objects": { "type": "array", "items": { "type": "string" } },
                                                         "Categories": { "type": "array", "items": { "type": "string" } }
                                                      },
                                                      "additionalProperties": false
                                                  }
                                                  """),
                jsonSchemaIsStrict: true)
        };
    
        var completion = await client.GetChatClient(modelName).CompleteChatAsync(messages,options);
        using var structuredJson = JsonDocument.Parse(completion.Value.Content[0].Text);
        var summary = structuredJson.RootElement.GetProperty("Summary").GetString();
        var objects = structuredJson.RootElement.GetProperty("Objects").EnumerateArray().Select(x => x.GetString()).ToList();
        var categories = structuredJson.RootElement.GetProperty("Categories").EnumerateArray().Select(x => x.GetString()).ToList();
        return new PhotoSummary()
        {
            Description = summary!,
            //Model = modelName,
            ObjectClasses = objects!,
            Categories = categories!,
            DateGenerated = DateTimeOffset.Now,
            PromptSummary =
                new PromptSummary([PromptSummary], modelName, stopwath.Elapsed,
                    new Dictionary<string, object>()
                    {
                        [nameof(options.Temperature)] = options.Temperature
                    })
        };
    }

    private OpenAIClient GetClient(string modelName)
    {
        return OpenAiClients.GetOrAdd(modelName, name => new OpenAIClient(name));
    }
        /*if (_openAiClients.TryGetValue(modelName, out var client))
        {
            return client;
        }

        client = new OpenAIClient(modelName);

        _openAiClients.TryAdd(modelName, client);
        return client;
    }*/

    public bool CanHandle(string modelName)
    {
        return SupportedModels.Contains(modelName, StringComparer.OrdinalIgnoreCase);
    }
}