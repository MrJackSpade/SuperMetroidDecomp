using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>What one shinespark step reads from the frame: clocks, projectiles, whether Samus cannot die and the previous checkpoint (#633).</summary>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
public sealed partial record ShinesparkFrame
{
    /// <summary>
    /// The update count, whose oddness picks the direction of the vertical block scan.
    /// </summary>
    public required int FrameCounter { get; init; }

    /// <summary>
    /// Whether Samus cannot die, as the cannot-die assist sets it: the spark ignores its low-energy
    /// cutoff and drains energy no lower than one, while a collision still ends it.
    /// </summary>
    public required bool SamusCannotDie { get; init; }

    /// <summary>Samus's shots, whose count admits the crash's echoes as it finishes and which carry them.</summary>
    public required SamusProjectileSystem Projectiles { get; init; }

    /// <summary>
    /// The gameplay clock's frames within its second, used by echo sampling independently of
    /// the frame counter's collision parity.
    /// </summary>
    public required int GameTimeFrames { get; init; }

    /// <summary>Samus's camera point carried across the frame boundary, whose X/Y trailing distance is capped after the corresponding spark movement axis.</summary>
    public required FixedPosition<RoomPixels> PreviousCheckpoint { get; init; }
}
