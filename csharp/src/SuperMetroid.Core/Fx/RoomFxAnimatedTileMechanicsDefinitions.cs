namespace SuperMetroid.Core.Fx;

/// <summary>
/// Immutable engine-owned header and control data for the simple retail room-FX
/// animated-tile objects.
/// </summary>
/// <remarks>
/// Where each frame's artwork comes from is deliberately not stored here: the frames are replaceable
/// installed character artwork. This catalog owns
/// only each object's script position, artwork size, duration, and loop control required
/// to schedule that artwork.
/// </remarks>
public static class RoomFxAnimatedTileMechanicsDefinitions
{
    /// <summary>The seven supported objects' headers, by object (<see cref="RoomFxAnimatedTiles"/>).</summary>
    /// <remarks>Seven named spike/sand/liquid/weather objects select script starts,
    /// transfer sizes and encoded destinations. Each field and the full ushort
    /// identity domain are independently verified against NTSC J/U v1.0 and pinned
    /// reference sources (362be646929cf8e483f692b73a6561cfc2dc1d0d).
    /// Frame schedules use the separately verified arithmetic view; neither headers
    /// nor frames are retained in a generated table.</remarks>
    internal static readonly IReadOnlyDictionary<AnimatedTileObject, RoomFxAnimatedTileLoop> Objects =
        new Dictionary<AnimatedTileObject, RoomFxAnimatedTileLoop>
    {
        [AnimatedTileObject.HorizontalSpikes] = new(AnimatedTileObject.HorizontalSpikes,
            frameArtworkSize: 128,
            frameCount: 4, frameDuration: 8),
        [AnimatedTileObject.MaridiaSandCeiling] = new(AnimatedTileObject.MaridiaSandCeiling,
            frameArtworkSize: 64,
            frameCount: 4, frameDuration: 10),
        [AnimatedTileObject.MaridiaSandFalling] = new(AnimatedTileObject.MaridiaSandFalling,
            frameArtworkSize: 32,
            frameCount: 4, frameDuration: 10),
        [AnimatedTileObject.Lava] = new(AnimatedTileObject.Lava,
            frameArtworkSize: 64,
            frameCount: 5, frameDuration: 13),
        [AnimatedTileObject.Acid] = new(AnimatedTileObject.Acid,
            frameArtworkSize: 64,
            frameCount: 5, frameDuration: 10),
        [AnimatedTileObject.Rain] = new(AnimatedTileObject.Rain,
            frameArtworkSize: 80,
            frameCount: 5, frameDuration: 10),
        [AnimatedTileObject.Spores] = new(AnimatedTileObject.Spores,
            frameArtworkSize: 48,
            frameCount: 3, frameDuration: 10),
    };

}
