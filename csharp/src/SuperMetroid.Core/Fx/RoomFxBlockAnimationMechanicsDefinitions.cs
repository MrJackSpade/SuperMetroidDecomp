namespace SuperMetroid.Core.Fx;

/// <summary>
/// The looping animations the room effects, the horizontal spikes and the Maridia sand show: how many frames each loop
/// has and how many passes each frame is shown for.
/// </summary>
/// <remarks>
/// What each frame pictures is installed artwork, which asset extraction pictures under the frame's name; this catalog
/// owns only each loop's frames and their durations.
/// </remarks>
public static class RoomFxBlockAnimationMechanicsDefinitions
{
    /// <summary>The seven loops, by animation (<see cref="RoomFxBlockAnimations"/>).</summary>
    internal static readonly IReadOnlyDictionary<BlockAnimation, RoomFxBlockAnimationLoop> Objects =
        new Dictionary<BlockAnimation, RoomFxBlockAnimationLoop>
    {
        [BlockAnimation.HorizontalSpikes] = new(BlockAnimation.HorizontalSpikes, frameCount: 4, frameDuration: 8),
        [BlockAnimation.MaridiaSandCeiling] = new(BlockAnimation.MaridiaSandCeiling, frameCount: 4, frameDuration: 10),
        [BlockAnimation.MaridiaSandFalling] = new(BlockAnimation.MaridiaSandFalling, frameCount: 4, frameDuration: 10),
        [BlockAnimation.Lava] = new(BlockAnimation.Lava, frameCount: 5, frameDuration: 13),
        [BlockAnimation.Acid] = new(BlockAnimation.Acid, frameCount: 5, frameDuration: 10),
        [BlockAnimation.Rain] = new(BlockAnimation.Rain, frameCount: 5, frameDuration: 10),
        [BlockAnimation.Spores] = new(BlockAnimation.Spores, frameCount: 3, frameDuration: 10),
    };
}
