using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using ImageMagick;
using OpenAI;
using OpenAI.Chat;
using PhotoSearch.Data.Models;

namespace PhotoSearch.Worker.Clients;

public class OpenAiPhotoSummaryClient ([FromKeyedServices("openaiConnection")]OpenAIClient client ): IPhotoSummaryClient
{
    private static readonly string[] SupportedModels =
    [
        "o1-preview", "o1-preview-2024-09-12", "o1-mini", "o1-mini-2024-09-12", "gpt-4o-mini", "gpt-3.5-turbo-instruct",
        "gpt-4o","gpt-4o-2024-11-20", "gpt-4o-mini-2024-07-18", "gpt-3.5-turbo"
    ];
 
    private const string SystemPrompt = 
        "You are a highly accurate image summarization model. When summarizing an image:\n\n"
        + "Base your description strictly on visible elements: Only describe what you can directly see in the image. Do not infer details that are not clearly present.\n"
        + "Avoid assumptions and speculation: If something is unclear or uncertain, explicitly state that uncertainty rather than guessing.\n"
        + "Be factual and concise: Accurately mention colors, objects, people, backgrounds, and any other identifiable features. Do not invent elements or interpret beyond what is visually evident.\n"
        + "Maintain neutrality and precision: Use neutral language, focus on describing the scene, and avoid adding opinions or emotions not directly conveyed by the image."
        + "Include details: Provide a detailed description where possible.";

    private const string PromptSummary =
        "Please provide a detailed description of the attached photo";

    public async Task<PhotoSummary> SummarisePhoto(string modelName, string imagePath, string base64Image,
        string address)
    {
        var stopwath = new Stopwatch();
        if (!SupportedModels.Contains(modelName, StringComparer.OrdinalIgnoreCase))
        {
            throw new NotSupportedException($"Model {modelName} is not supported by this client");
        }

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
        resizedImage.Resize(new MagickGeometry(200, 200)
        {
            IgnoreAspectRatio = false,
            Greater = false // Only shrink if larger than dimensions
        });
        
        using var memStream = new MemoryStream();
        await resizedImage.WriteAsync(memStream);
        
        var img = ChatMessageContentPart.CreateImagePart(new BinaryData(memStream.ToArray()), "image/jpeg",
            ChatImageDetailLevel.Auto);
        List<ChatMessage> messages =
        [
            new UserChatMessage( PromptSummary, img),
            new SystemChatMessage(SystemPrompt)
        ];
        var options = new ChatCompletionOptions()
        {
            Temperature = 0.1f,
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(jsonSchemaFormatName: "image_summary_result",
                jsonSchema: BinaryData.FromString("""
                                                  {
                                                      "type": "object",
                                                      "properties": {
                                                         "Summary": { "type": "string"},
                                                         "Objects": { "type": "array", "items": { "type": "string" } },
                                                         "Categories": { "type": "array", "items": { "type": "string" } }
                                                      },
                                                     "required": ["Summary", "Objects", "Categories"],
                                                      "additionalProperties": false
                                                  }
                                                  """),
                jsonSchemaIsStrict: true)
        };

        var completion = await client.GetChatClient(modelName).CompleteChatAsync(messages, options);
        using var structuredJson = JsonDocument.Parse(completion.Value.Content[0].Text);
        var summary = structuredJson.RootElement.GetProperty("Summary").GetString();
        var objects = structuredJson.RootElement.GetProperty("Objects").EnumerateArray().Select(x => x.GetString())
            .ToList();
        var categories = structuredJson.RootElement.GetProperty("Categories").EnumerateArray()
            .Select(x => x.GetString()).ToList();
        await Task.Delay(TimeSpan.FromSeconds(20));
        return new PhotoSummary()
        {
            Description = summary!,
            ObjectClasses = objects!,
            Categories = categories!,
            DateGenerated = DateTimeOffset.Now,
            PromptSummary =
                new PromptSummary([PromptSummary, SystemPrompt], modelName, stopwath.Elapsed,
                    new Dictionary<string, object>()
                    {
                        [nameof(options.Temperature)] = options.Temperature
                    })
        };
    }
    public bool CanHandle(string modelName)
    {
        return SupportedModels.Contains(modelName, StringComparer.OrdinalIgnoreCase);
    }
}