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

    /// <summary>A step that moves the Evir by <paramref name="delta"/>: right and down are positive.</summary>
    /// <param name="delta">The room-pixel movement the step makes.</param>
    internal static DraygonIntroMovement Move(Offset16<RoomPixels> delta) => new(delta, DeletesSprite: false);
}
