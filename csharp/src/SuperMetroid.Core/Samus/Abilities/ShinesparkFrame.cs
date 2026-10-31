using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>What one shinespark step reads from the frame: clocks, projectiles, invincibility, timeout policy and the previous checkpoint (#633).</summary>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
public sealed record ShinesparkFrame
{
    /// <summary>
    /// The frame counter, 0 through 65535 as the frame clock wraps it; only its parity is read, selecting the
    /// vertical block-probe traversal order.
    /// </summary>
    public required int FrameCounter { get; init; }

    /// <summary>
    /// Ordinary projectile count used for crash-echo admission when no live <see cref="Projectiles"/> owner is
    /// supplied; excludes bomb slots. It is a count of live slots, never negative.
    /// </summary>
    public int ProjectileCounter { get; init; } = 0;

    /// <summary>Host cheat that bypasses the low-energy crash condition, but retains collision termination and drains energy no lower than one.</summary>
    public bool PlayerInvincibilityEnabled { get; init; } = false;

    /// <summary>Optional live ordinary-projectile owner supplying its count and accepting released echoes in fixed slots three/four.</summary>
    public SamusProjectileSystem? Projectiles { get; init; } = null;

    /// <summary>
    /// Gameplay-clock frame count, 0 through 65535 as the clock wraps it, used by echo sampling independently of
    /// the frame counter's collision parity.
    /// </summary>
    public int GameTimeFrames { get; init; } = 0;

    /// <summary>The runtime commits interrupted poses after animation; standalone movement callers may commit immediately.</summary>
    public bool DeferTimeoutPoseChange { get; init; } = false;

    /// <summary>Optional mutable camera-history point whose X/Y trailing distance is capped after the corresponding spark movement axis.</summary>
    public FixedPosition<RoomPixels>? PreviousCheckpoint { get; init; } = null;
}
