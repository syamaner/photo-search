using Microsoft.SemanticKernel;

namespace PhotoSearch.API.Chat;

public static class PromptConstants
{

    private const string BasicChatPromptTemplate = """
                                                   You are a helpful AI assistant specialised in technical question.
                                                   You take pride on accuracy and you don't make things up.
                                                   You prefer a good summary over a long explanation but also provide clear justification for the answer.
                                                   Please do not include the question in the answer.
                                                   If you are not sure about the answer, say "I cannot find the answer in the provided context."
                                                   
                                                   Question:
                                                   {{$question}}                                                   
                                                   """;
    private const string RagPromptTemplate = """
                                             You are a helpful AI assistant specialised in technical questions and good at utilising additional technical resources provided to you as additional context.
                                             Use the following context to answer the question. Be truthful.
                                             You prefer a good summary over a long explanation but also provide clear justification for the answer.
                                             If the question has absolutely no relevance to the context, please answer "I don't know the answer."
                                             Please do not include the question in the answer.

                                             Context:
                                             {{$context}}

                                             Question:
                                             {{$question}}                                             
                                             """;
  
    /// <summary>
    /// To answer the question, the AI assistant will use the provided context.
    /// </summary>
    public static readonly PromptTemplateConfig RagPromptConfig = new()
    {
        Template = RagPromptTemplate,
        InputVariables =
        [
            new InputVariable { Name = "context" },
            new InputVariable { Name = "question" }
        ]
    };
    
    /// <summary>
    /// To answer the question, the AI assistant will not use any additional context.
    /// </summary>
    public static readonly PromptTemplateConfig BasicPromptConfig = new()
    {
        Template = BasicChatPromptTemplate,
        InputVariables =
        [
            new InputVariable { Name = "question" }
        ]
    };
}