using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The order the import walks the Samus projectiles' animation frames in (#1301, #1517): the frame bindings file, the sprite
/// locations and the cartridge's record table each list every frame in this order.
/// </summary>
internal static class SamusProjectileFrameOrder
{
    /// <summary>
    /// Every frame, animation by animation in segment order, each animation's in the order it first shows them: a frame
    /// shown again is listed once (#2274).
    /// </summary>
    internal static IReadOnlyList<SamusProjectileFrameId> Frames { get; } =
        [.. Enum.GetValues<SamusProjectileSegment>().SelectMany(program => SamusProjectileAnimations.Segments.Beats(program)
            .Select(beat => beat.ShownFrame).OfType<SamusProjectileFrameId>()).Distinct()];
}
