namespace SuperMetroid.Core.Fx;

/// <summary>
/// Immutable engine-owned header and control data for the simple retail room-FX
/// animated-tile objects in bank <c>$87</c>.
/// </summary>
/// <remarks>
/// Frame source pointers deliberately are not stored here. They identify replaceable
/// installed character artwork. This catalog owns
/// only the instruction cursor, transfer geometry, duration, and loop control required
/// to schedule that artwork.
/// </remarks>
public static class RoomFxAnimatedTileMechanicsDefinitions
{
    /// <summary>The seven supported objects' headers, by object (<see cref="RoomFxAnimatedTiles"/>).</summary>
    /// <remarks>Seven named spike/sand/liquid/weather objects select native instruction starts,
    /// transfer sizes and encoded VRAM destinations. Each field and the full ushort
    /// identity domain are independently verified against NTSC J/U v1.0 and pinned
    /// bank_87.asm (362be646929cf8e483f692b73a6561cfc2dc1d0d).
    /// Frame schedules use the separately verified arithmetic view; neither headers
    /// nor frames are retained in a generated table.</remarks>
    internal static readonly IReadOnlyDictionary<AnimatedTileObject, RoomFxAnimatedTileLoop> Objects =
        new Dictionary<AnimatedTileObject, RoomFxAnimatedTileLoop>
    {
        [AnimatedTileObject.HorizontalSpikes] = new(AnimatedTileObject.HorizontalSpikes,
            frameByteCount: 128,
            frameCount: 4, frameDuration: 8, imageCount: 3),
        [AnimatedTileObject.MaridiaSandCeiling] = new(AnimatedTileObject.MaridiaSandCeiling,
            frameByteCount: 64,
            frameCount: 4, frameDuration: 10, imageCount: 4),
        [AnimatedTileObject.MaridiaSandFalling] = new(AnimatedTileObject.MaridiaSandFalling,
            frameByteCount: 32,
            frameCount: 4, frameDuration: 10, imageCount: 4),
        [AnimatedTileObject.Lava] = new(AnimatedTileObject.Lava,
            frameByteCount: 64,
            frameCount: 5, frameDuration: 13, imageCount: 5),
        [AnimatedTileObject.Acid] = new(AnimatedTileObject.Acid,
            frameByteCount: 64,
            frameCount: 5, frameDuration: 10, imageCount: 5),
        [AnimatedTileObject.Rain] = new(AnimatedTileObject.Rain,
            frameByteCount: 80,
            frameCount: 5, frameDuration: 10, imageCount: 5),
        [AnimatedTileObject.Spores] = new(AnimatedTileObject.Spores,
            frameByteCount: 48,
            frameCount: 3, frameDuration: 10, imageCount: 3),
    };

}
