using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The order the import walks the Samus projectiles' animation frames in (#1301, #1517): the frame bindings file, the sprite
/// locations and the cartridge's record table each list every frame in this order.
/// </summary>
internal static class SamusProjectileFrameOrder
{
    /// <summary>Every frame, animation by animation in segment order.</summary>
    internal static IReadOnlyList<SamusProjectileFrame> Frames { get; } =
        [.. Enum.GetValues<SamusProjectileSegment>().SelectMany(program =>
            Enumerable.Range(0, SamusProjectileFrameDefinitions.ByAnimation[program].Length).Select(show => new SamusProjectileFrame(program, show)))];
}
