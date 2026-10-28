namespace SuperMetroid.Core.Rendering;

/// <summary>Inserts only the sprite pixels whose frontmost sprite has this priority.</summary>
/// <param name="Priority">The sprite priority tier, 0 through <see cref="FrontmostDepth"/>.</param>
/// <param name="FixedColor">Optional fixed color added to the inserted pixels.</param>
public sealed record SpriteDepthLayer(int Priority, ScreenTintLayer? FixedColor = null) : DisplayLayer
{
    /// <summary>The highest of the four OBJ priority tiers.</summary>
    public const int FrontmostDepth = 3;

    /// <summary>The OBJ priority tier.</summary>
    public int Depth
    {
        get;
        init => field = Ensure.BetweenInclusive(0, FrontmostDepth, value);
    } = Ensure.BetweenInclusive(0, FrontmostDepth, Priority);
}
