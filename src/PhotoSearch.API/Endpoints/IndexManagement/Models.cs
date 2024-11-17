namespace PhotoSearch.API.Endpoints.IndexManagement;

public record IndexPhotosRequest(string Directory);

public record SummarisePhotosRequest(string ModelName);

public record BatchSummarisePhotosRequest(List<string> ModelNames);