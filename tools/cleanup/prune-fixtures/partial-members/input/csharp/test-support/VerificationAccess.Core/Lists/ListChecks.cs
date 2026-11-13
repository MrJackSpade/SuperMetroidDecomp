using SuperMetroid.Desktop.Lists;

namespace SuperMetroid.Verification.Lists;

/// <summary>The counts and levels the checks compare.</summary>
internal static class ListChecks
{
    /// <summary>The checked layout counted twice over.</summary>
    internal static int Recounted => ListPings.Recount(new ListLayout([4, 12]));

    /// <summary>The checked meter's doubled and tripled levels.</summary>
    internal static int Levels => new ListMeter().Doubled() + new ListMeter().Tripled;
}
