namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>One axis of a room state's background scroll: how it follows the camera, and its parallax factor.</summary>
/// <param name="Kind">How the axis follows the camera.</param>
/// <param name="Factor">The parallax multiplier over 256, an even number from 2 through 254; zero for the other kinds.</param>
public readonly record struct BackgroundAxisScroll(BackgroundScrollKind Kind, int Factor)
{
    /// <summary>The largest parallax factor: just under the foreground's own speed.</summary>
    private const int LargestParallaxFactor = 254;

    /// <summary>An axis that copies the foreground's scroll.</summary>
    public static readonly BackgroundAxisScroll FollowsForeground = new(BackgroundScrollKind.FollowsForeground, 0);

    /// <summary>An axis that stays at zero.</summary>
    public static readonly BackgroundAxisScroll Fixed = new(BackgroundScrollKind.Fixed, 0);

    /// <summary>An axis that scrolls at <paramref name="factor"/> 256ths of the foreground's speed.</summary>
    /// <param name="factor">The parallax multiplier over 256, an even number from 2 through 254.</param>
    public static BackgroundAxisScroll Parallax(int factor)
    {
        Ensure.BetweenInclusive(2, LargestParallaxFactor, factor);
        return factor % 2 == 0
            ? new(BackgroundScrollKind.Parallax, factor)
            : throw new ArgumentOutOfRangeException(nameof(factor), factor, "A parallax factor is an even number of 256ths.");
    }
}
