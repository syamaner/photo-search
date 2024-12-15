using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using ImageMagick;
using OpenAI;
using OpenAI.Chat;
using PhotoSearch.Data.Models;

namespace PhotoSearch.Worker.Clients;

public class OpenAiPhotoSummaryEvaluationClient([FromKeyedServices("openaiConnection")] OpenAIClient client)
    : IPhotoSummaryEvaluator
{
    private const string SystemPrompt =
        "You are a highly accurate and fair image summarization evaluation model. "
        + " Your job is to evaluate the quality of summaries generated from images by different computer vision models. \n\n"
        + " When evaluating a summary of the provided image:\n\n"
        + " - Provide a single score ranging between 0 and 100 combining the following properties: \n\n"
        + "    - Quality and accuracyof the summary.\n\n"
        + "    - Quality and accuracy of the categories predicted for the image.\n\n"
        + "    - Quality and accuracy of the objects predicted to be in the image.\n\n"
        + "  - Be fair and consistent when evaluating. \n\n";

    private const string PromptSummary =
        "Please score the provided image summary based on the quality and accuracy of the summary, categories, and objects predicted in the image.";

    public async Task<PhotoSummaryScore> EvaluatePhotoSummary(string base64Image, ImageSummaryEvaluationRequest summary)
    {
        var imageBytes = Convert.FromBase64String(base64Image);
        using var resizedImage = new MagickImage(imageBytes);
        resizedImage.Resize(new MagickGeometry(256, 256)
        {
            IgnoreAspectRatio = false,
            Greater = false
        });
        using var memStream = new MemoryStream();
        await resizedImage.WriteAsync(memStream);
        var img = ChatMessageContentPart.CreateImagePart(new BinaryData(memStream.ToArray()), "image/jpeg",
            ChatImageDetailLevel.Auto);
        List<ChatMessage> messages =
        [
            new UserChatMessage(PromptSummary, JsonSerializer.Serialize(summary), img),
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
                                                         "Score": { "type": "number"},
                                                         "Justification": { "type": "string"}
                                                      },
                                                     "required": ["Score", "Justification"],
                                                      "additionalProperties": false
                                                  }
                                                  """),
                jsonSchemaIsStrict: true)
        };
        var completion = await client.GetChatClient("gpt-4o").CompleteChatAsync(messages, options);
        using var structuredJson = JsonDocument.Parse(completion.Value.Content[0].Text);
        var score = structuredJson.RootElement.GetProperty("Score").GetDouble();
        var justification = structuredJson.RootElement.GetProperty("Justification").GetString();
        return new PhotoSummaryScore(score, justification!, "OpenAI");
    }
}