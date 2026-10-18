namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The Pirate/Mother Brain laser and Ninja Pirate claw programs (#641), in authored order.</summary>
internal enum SpacePirateProjectileProgram
{
    /// <summary>The left-moving laser: muzzle poses, then it starts moving.</summary>
    LaserLeft,

    /// <summary>The left-moving laser's two-pose loop.</summary>
    LaserLeftLoop,

    /// <summary>The right-moving laser: muzzle poses, then it starts moving.</summary>
    LaserRight,

    /// <summary>The right-moving laser's two-pose loop.</summary>
    LaserRightLoop,

    /// <summary>The claw thrown left: it starts moving.</summary>
    ClawLeft,

    /// <summary>The left claw's eight-pose spin.</summary>
    ClawLeftLoop,

    /// <summary>The claw thrown right: it starts moving.</summary>
    ClawRight,

    /// <summary>The right claw's eight-pose spin.</summary>
    ClawRightLoop,
}
