namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>Exact bounded motion policies for the Landing Site gunship.</summary>
internal static class GunshipMotionDefinitions
{

    /// <summary>The landing brake's signed whole-pixel Y delta for each of its 17 frames: +1 for frames 0..5, zero for 6..10, and -1 for 11..16.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against all NTSC J/U v1.0 words and pinned
    /// bank_A2.asm. These are explicit six/five/six braking intervals, not a sampled
    /// physical acceleration curve. Callers add the signed whole-pixel delta with word wrapping, then advance.
    /// </remarks>
    internal static readonly short[] LandingBrakeYDeltas = [1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, -1, -1, -1, -1, -1, -1];

    /// <summary>The idle bob's four phases: timer 16 and signed delta +1,-1,-1,+1.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against each of the two byte fields in
    /// NTSC J/U v1.0 and pinned ProcessShipHover. Timer and delta have separate
    /// original-data proofs. Endpoint phases move down, middle phases move up.
    /// The caller owns expiration and phase wrap.
    /// </remarks>
    internal static readonly GunshipIdleBobDefinition[] IdleBobs = [new(16, 1), new(16, -1), new(16, -1), new(16, 1)];

    /// <summary>Above this Y the descending ship moves $4.8000 pixels per call; below it, $2.8000.</summary>
    internal const ushort SlowDescentY = 768;

    /// <summary>The descending ship's speed above <see cref="SlowDescentY"/>, 16.16 pixels per call.</summary>
    internal const uint FastDescentSpeed = 294912u;

    /// <summary>The descending ship's speed below <see cref="SlowDescentY"/>, 16.16 pixels per call.</summary>
    internal const uint SlowDescentSpeed = 163840u;

    /// <summary>The landed top hull's Y, where the descent clamps.</summary>
    internal const ushort LandedTopY = 1119;

    /// <summary>The landed bottom hull's Y.</summary>
    internal const ushort LandedBottomY = 1159;

    /// <summary>The landed entrance pad's Y.</summary>
    internal const ushort LandedPadY = 1118;

    /// <summary>The landed ship's X, where Samus stands to board.</summary>
    internal const ushort LandedX = 1152;

    /// <summary>The escaping ship's starting 8.8 upward velocity.</summary>
    internal const ushort EscapeStartVelocity = 512;

    /// <summary>The escaping ship's 8.8 acceleration per call.</summary>
    internal const ushort EscapeAcceleration = 64;

    /// <summary>The escaping velocity whose whole byte reaching ten caps it.</summary>
    internal const ushort EscapeVelocityCapTrigger = 2560;

    /// <summary>The escaping ship's capped 8.8 velocity.</summary>
    internal const ushort EscapeVelocityCap = 2304;
}
