namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The Pirate/Mother Brain laser and Ninja Pirate claw animations (#641), in authored order.</summary>
internal enum SpacePirateProjectileSegment
{
    /// <summary>The left-moving laser: muzzle poses, then it starts moving.</summary>
    LaserLeft,

    /// <summary>The left-moving laser's full-length beam flickering as it flies, looped (#2276).</summary>
    LaserLeftFlicker,


    /// <summary>The right-moving laser: muzzle poses, then it starts moving.</summary>
    LaserRight,

    /// <summary>The right-moving laser's full-length beam flickering as it flies, looped (#2276).</summary>
    LaserRightFlicker,


    /// <summary>The claw thrown left: it starts moving.</summary>
    ClawLeft,

    /// <summary>The claw thrown left spinning as it flies, looped (#2276).</summary>
    ClawLeftSpin,


    /// <summary>The claw thrown right: it starts moving.</summary>
    ClawRight,

    /// <summary>The claw thrown right spinning as it flies, looped (#2276).</summary>
    ClawRightSpin,

}
