namespace SuperMetroid.Core.Rendering;

/// <summary>Draws only the sprite pixels whose frontmost sprite is at this depth.</summary>
/// <param name="Priority">The sprites' depth, 0 (rearmost) through <see cref="FrontmostDepth"/>.</param>
/// <param name="FixedColor">An optional tint added to the drawn pixels.</param>
public sealed record SpriteDepthLayer(int Priority, ScreenTintLayer? FixedColor = null) : DisplayLayer
{
    /// <summary>The frontmost of the four sprite depths.</summary>
    public const int FrontmostDepth = 3;

    /// <summary>The sprites' depth.</summary>
    public int Depth
    {
        get;
        init => field = Ensure.BetweenInclusive(0, FrontmostDepth, value);
    } = Ensure.BetweenInclusive(0, FrontmostDepth, Priority);
}
