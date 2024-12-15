using System.Diagnostics;
using System.Text.Json;
using ImageMagick; 
using OpenAI;
using OpenAI.Chat;
using PhotoSearch.Data.Models;

namespace PhotoSearch.Worker.Clients;

public class OllamaPhotoSummaryClient([FromKeyedServices("ollamaConnection")]OpenAIClient client) : IPhotoSummaryClient
{
    private static readonly string[] SupportedModels =
        ["llava-phi3", "llava:7b", "llava:13b", "bakllava", "llava-llama3", "llama3.2-vision"];

    private const string SystemPrompt = """
                                        You are an expert image analyst tasked with providing detailed and accurate summaries of photos. For every photo, follow these steps:
                                          - Describe the photo in detail. This should be a coherent and detailed description of the photo. 
                                          - The user prompts will sometimes include address information to help you provide a more accurate description. 
                                            - You can use address information to verify the content of the photo.
                                          - Pay attention to the user prompts. For each photo there will be 2 distinct prompts to get the Summary first, then Objects and then Categories.
                                          - Do not include any symbols or special characters in your response.
                                        """;

    private const string PromptSummary =
        "Provide a detailed description of the attached photo. Please not that the location is as following : {0}. Someone who did not see this image should be able to visualise it using this description.";

    private const string PromptList =
        "Provide a list of categories the photo belongs to. Also provide the list of objects in the photo. Response should be in json format.";
    

    public bool CanHandle(string modelName)
    {
        return SupportedModels.Any(x => modelName.Contains(x, StringComparison.CurrentCultureIgnoreCase));
    }

    public async Task<PhotoSummary> SummarisePhoto(string modelName, string imagePath, string base64Image,
        string address)
    {
        var stopWath = new Stopwatch();
        stopWath.Start();
        // Convert base64 to image, resize, and convert back
        byte[] imageBytes;
        if (!string.IsNullOrWhiteSpace(base64Image))
        {
            imageBytes = Convert.FromBase64String(base64Image);
        }
        else
        {
            using var image = new MagickImage(imagePath);
            imageBytes = image.ToByteArray();
        }

        using var resizedImage = new MagickImage(imageBytes);
        using var memStream = new MemoryStream();
        await resizedImage.WriteAsync(memStream);

        var img = ChatMessageContentPart.CreateImagePart(new BinaryData(memStream.ToArray()), "image/jpeg",
            ChatImageDetailLevel.Auto);
        List<ChatMessage> messages =
        [
            new SystemChatMessage(SystemPrompt),
            new UserChatMessage(string.Format(PromptSummary, address), img)
        ];
        var options = new ChatCompletionOptions()
        {
            Temperature = 0.3f,
            ResponseFormat = ChatResponseFormat.CreateTextFormat()
        };

        var completion = await client.GetChatClient(modelName).CompleteChatAsync(messages, options);
        var summary = string.Join(" ", completion.Value.Content.Select(x => x.Text));
        
        messages =
        [
            new SystemChatMessage(SystemPrompt),
            new UserChatMessage(PromptList, img)
        ];
        options = new ChatCompletionOptions()
        {
            Temperature = 0.6f,
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(jsonSchemaFormatName: "image_summary_result",
                jsonSchema: BinaryData.FromString("""
                                                  {
                                                      "type": "object",
                                                      "properties": {
                                                         "Objects": { "type": "array", "items": { "type": "string" } },
                                                         "Categories": { "type": "array", "items": { "type": "string" } }
                                                      },
                                                     "required": ["Summary", "Objects", "Categories"],
                                                      "additionalProperties": false
                                                  }
                                                  """),
                jsonSchemaIsStrict: true)
        };
        completion = await client.GetChatClient(modelName).CompleteChatAsync(messages, options);
        using var structuredJson = JsonDocument.Parse(completion.Value.Content[0].Text);
        var objects = structuredJson.RootElement.GetProperty("Objects").EnumerateArray().Select(x => x.GetString())
            .ToList();
        var categories = structuredJson.RootElement.GetProperty("Categories").EnumerateArray()
            .Select(x => x.GetString()).ToList();

        return new PhotoSummary()
        {
            Description = summary!,
            ObjectClasses = objects!.Take(10).ToList()!,
            Categories = categories!.Take(10).ToList()!,
            DateGenerated = DateTimeOffset.Now,
            PromptSummary =
                new PromptSummary([PromptSummary, SystemPrompt], modelName, stopWath.Elapsed,
                    new Dictionary<string, object>()
                    {
                        [nameof(options.Temperature)] = options.Temperature
                    })
        };
    }

}