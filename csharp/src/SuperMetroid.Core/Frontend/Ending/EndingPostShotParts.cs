namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The parts the post-shot shows one per call once Samus has faded (#1467): the subtitle, the logo's four quarters, then its map.</summary>
internal static class EndingPostShotParts
{
    /// <summary>The parts the post-shot shows.</summary>
    public const int Count = 6;

    /// <summary>The part that shows the logo's first quarter; the subtitle comes before it.</summary>
    public const int FirstLogoPart = 1;

    /// <summary>The logo's quarters, each shown by its own part.</summary>
    public const int LogoParts = 4;
}
