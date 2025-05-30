using FastEndpoints;

namespace PhotoSearch.Common.Contracts;

public record ImportPhotos(string Directory);
public record SummarisePhotos(List<string> ImagePaths, string ModelName);

public record BatchSummarisePhotos(List<string> ModelNames);

public record SummarisePhotosFromDirectory(string Directory, string ModelName);

public record EvaluateModelSummaries();