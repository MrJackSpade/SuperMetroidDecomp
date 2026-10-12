using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>What one shinespark step reads from the frame: clocks, projectiles, invincibility, timeout policy and the previous checkpoint (#633).</summary>
public sealed record ShinesparkFrame
{
    /// <summary>Native NMI frame word whose parity selects the vertical block-probe traversal order.</summary>
    public required ushort NmiFrameCounter { get; init; }

    /// <summary>Ordinary projectile count used for crash-echo admission when no live <paramref name="projectiles"/> owner is supplied; excludes bomb slots.</summary>
    public ushort ProjectileCounter { get; init; } = 0;

    /// <summary>Host cheat that bypasses the low-energy crash condition, but retains collision termination and drains energy no lower than one.</summary>
    public bool PlayerInvincibilityEnabled { get; init; } = false;

    /// <summary>Optional live ordinary-projectile owner supplying its count and accepting released echoes in fixed slots three/four.</summary>
    public SamusProjectileSystem? Projectiles { get; init; } = null;

    /// <summary>Native gameplay-clock word used by echo sampling, independent of NMI collision parity.</summary>
    public ushort GameTimeFrames { get; init; } = 0;

    /// <summary>The runtime commits interrupted poses after animation; standalone movement callers may commit immediately.</summary>
    public bool DeferTimeoutPoseChange { get; init; } = false;

    /// <summary>Optional mutable camera-history point whose X/Y trailing distance is capped after the corresponding spark movement axis.</summary>
    public FixedPosition<RoomPixels>? PreviousCheckpoint { get; init; } = null;
}
