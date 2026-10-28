namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Yard's native eight-direction crawling sign selection.</summary>
public static class YardVelocityDefinitions
{
    /// <summary>The sign of each direction's horizontal crawl, by direction number.</summary>
    internal static readonly int[] XSigns = [-1, -1, 1, 1, -1, 1, -1, 1];

    /// <summary>The sign of each direction's vertical crawl, by direction number.</summary>
    internal static readonly int[] YSigns = [-1, 1, -1, 1, -1, -1, 1, 1];
}
