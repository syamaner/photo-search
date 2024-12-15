using System.Diagnostics.Metrics;

namespace PhotoSearch.ServiceDefaults;

public class ConsoleMetrics
{
    private readonly Counter<int> _photosSummariesCounter;
    private readonly Histogram<double> _photosSummaryHistogram;

    public ConsoleMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("PhotoSummary.Worker");
        _photosSummariesCounter = meter.CreateCounter<int>("photosummary.summary.generated");
        _photosSummaryHistogram = meter.CreateHistogram<double>("photosummary.summary.durationseconds");
    }

    public void PhotoSummarised(string model, int quantity)
    {
        _photosSummariesCounter.Add(quantity,
            new KeyValuePair<string, object?>("photosummary.summary.model", model));
    }
    
    public void PhotoSummaryTiming(string model,string photo, double durationSeconds)
    {
        _photosSummaryHistogram.Record(durationSeconds,
            new KeyValuePair<string, object?>("photosummary.summary.model", model),
            new KeyValuePair<string, object?>("photosummary.summary.photo", photo));
    }
}