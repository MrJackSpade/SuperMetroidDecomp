namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// One step of the path Draygon's opening dance moves its Evirs along: a per-update movement, or the step that ends an
/// Evir's dance by deleting it.
/// </summary>
/// <param name="Delta">The room-pixel movement the step makes; none for the delete step.</param>
/// <param name="DeletesSprite">True for the delete step.</param>
[IndependentFlag("whether an authored intro step deletes the sprite, a fact of the data")]
internal readonly record struct DraygonIntroMovement(
    Offset16<RoomPixels> Delta,
    bool DeletesSprite)
{
    /// <summary>The step that ends an Evir's dance by deleting it.</summary>
    internal static DraygonIntroMovement Delete => new(Offset16<RoomPixels>.Zero, DeletesSprite: true);

    /// <summary>A step that moves the Evir <paramref name="x"/> pixels right and <paramref name="y"/> pixels down.</summary>
    /// <param name="x">The rightward movement, negative to move left.</param>
    /// <param name="y">The downward movement, negative to move up.</param>
    internal static DraygonIntroMovement Move(short x, short y) => new(new(x, y), DeletesSprite: false);
}
