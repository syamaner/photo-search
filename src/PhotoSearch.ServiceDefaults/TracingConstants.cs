using System.Diagnostics;

namespace PhotoSearch.ServiceDefaults;

public static class TracingConstants
{
    public static ActivitySource WorkerActivitySource = new ActivitySource("PhotoSummary.Worker");
    public static ActivitySource ApiActivitySource = new ActivitySource("PhotoSummary.APi");
}