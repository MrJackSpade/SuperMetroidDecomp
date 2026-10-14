namespace SuperMetroid.Desktop;

/// <summary>Host timing policy kept separate from the counter's functional state.</summary>
internal static class FrameTimingConfiguration
{
    /// <summary>Refresh often enough to see stalls without making the numbers unreadably noisy.</summary>
    public static readonly TimeSpan ReportingInterval = TimeSpan.FromSeconds(1);
}
