namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The Pirate/Mother Brain laser's animation segments (#641, #2554), in authored order.</summary>
internal enum SpacePirateLaserSegment
{
    /// <summary>The left-moving laser: muzzle poses, then it starts moving.</summary>
    LaserLeft,

    /// <summary>The left-moving laser's full-length beam flickering as it flies, looped (#2276).</summary>
    LaserLeftFlicker,

    /// <summary>The right-moving laser: muzzle poses, then it starts moving.</summary>
    LaserRight,

    /// <summary>The right-moving laser's full-length beam flickering as it flies, looped (#2276).</summary>
    LaserRightFlicker,
}
