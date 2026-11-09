namespace SuperMetroid.Core.Fx;

/// <summary>
/// The looping animations the room effects, the horizontal spikes and the Maridia sand show: how many frames each loop
/// has and how many passes each frame is shown for.
/// </summary>
/// <remarks>
/// What each frame pictures is installed artwork, which asset extraction pictures under the frame's name; this catalog
/// owns only each loop's frames and their durations.
/// </remarks>
public static class RoomFxAnimatedTileMechanicsDefinitions
{
    /// <summary>The seven loops, by animation (<see cref="RoomFxAnimatedTiles"/>).</summary>
    internal static readonly IReadOnlyDictionary<AnimatedTileObject, RoomFxAnimatedTileLoop> Objects =
        new Dictionary<AnimatedTileObject, RoomFxAnimatedTileLoop>
    {
        [AnimatedTileObject.HorizontalSpikes] = new(AnimatedTileObject.HorizontalSpikes, frameCount: 4, frameDuration: 8),
        [AnimatedTileObject.MaridiaSandCeiling] = new(AnimatedTileObject.MaridiaSandCeiling, frameCount: 4, frameDuration: 10),
        [AnimatedTileObject.MaridiaSandFalling] = new(AnimatedTileObject.MaridiaSandFalling, frameCount: 4, frameDuration: 10),
        [AnimatedTileObject.Lava] = new(AnimatedTileObject.Lava, frameCount: 5, frameDuration: 13),
        [AnimatedTileObject.Acid] = new(AnimatedTileObject.Acid, frameCount: 5, frameDuration: 10),
        [AnimatedTileObject.Rain] = new(AnimatedTileObject.Rain, frameCount: 5, frameDuration: 10),
        [AnimatedTileObject.Spores] = new(AnimatedTileObject.Spores, frameCount: 3, frameDuration: 10),
    };
}
