namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The post-shot's reveal of the logo (#1467, #1513): once Samus has faded, the post-shot takes one step of it per call,
/// and the logo plane shows the logo on the last.
/// </summary>
internal static class EndingPostShotParts
{
    /// <summary>The calls the post-shot's logo reveal takes, the last of which shows the logo.</summary>
    public const int Count = 6;
}
