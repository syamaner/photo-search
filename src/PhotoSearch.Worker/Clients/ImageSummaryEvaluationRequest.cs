namespace PhotoSearch.Worker.Clients;

public record ImageSummaryEvaluationRequest(string Summary, List<string> ObjectClasses, List<string> Categories);