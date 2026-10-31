namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Samus's firing tables as one bounded region (#1411): the muzzle origins, then the cooldowns, then the beam speeds and
/// the accelerations, by byte offset from the region's start. Out-of-range directions and beam combinations read past
/// one table into the next, so the readers address the region rather than each table.
/// </summary>
internal static class SamusFiringTableDefinitions
{
    /// <summary>Number of projectile directions each direction-indexed table holds.</summary>
    internal const int DirectionCount = 10;
    /// <summary>Number of beam combinations each combination-indexed table holds.</summary>
    internal const int CombinationCount = 12;
    /// <summary>Offset from the uncharged delays to the charged-shot delay row.</summary>
    internal const int ChargedRowOffset = 16;
    /// <summary>Offset from the uncharged rows to the echo rows of the beam-row-indexed tables.</summary>
    internal const int EchoRowOffset = 32;
    /// <summary>The speed table holds two speed words per beam combination.</summary>
    internal const int InitialSpeedRowBytes = 2 * sizeof(ushort);

    /// <summary>Ten direction-indexed X origins for non-running poses.</summary>
    internal const int DefaultXOrigins = 0;
    /// <summary>Ten direction-indexed Y origins for non-running poses.</summary>
    internal const int DefaultYOrigins = 20;
    /// <summary>Ten direction-indexed X origins for running poses.</summary>
    internal const int RunningXOrigins = 40;
    /// <summary>Ten direction-indexed Y origins for running poses.</summary>
    internal const int RunningYOrigins = 60;
    /// <summary>Uncharged and charged delays indexed by beam combination.</summary>
    internal const int UnchargedCooldowns = 80;
    /// <summary>Non-beam projectile delays, indexed by projectile kind.</summary>
    internal const int NonBeamCooldowns = 118;
    /// <summary>Held-Shot repeat delays indexed by beam combination.</summary>
    internal const int AutoFireCooldowns = 127;
    /// <summary>
    /// The byte the bounded SpaceTime Beam setup reads as its cooldown when it fires its corrupt beam word: three bytes
    /// beyond the ordinary cooldown table, the low byte <c>$0D</c> of the adjacent instruction operand.
    /// </summary>
    internal const int SpacetimeBeamCooldown = 141;
    /// <summary>Speed word for horizontal and vertical power-beam travel.</summary>
    internal const int HorizontalVerticalSpeeds = 205;
    /// <summary>Speed word for diagonal power-beam travel.</summary>
    internal const int DiagonalSpeeds = 207;
    /// <summary>Direction-indexed missile acceleration records.</summary>
    internal const int MissileAccelerations = 255;
    /// <summary>Direction-indexed Super Missile acceleration records.</summary>
    internal const int SuperMissileAccelerations = 295;
    /// <summary>Direction-indexed beam X acceleration words.</summary>
    internal const int BeamXAccelerations = 335;
    /// <summary>Direction-indexed beam Y acceleration words.</summary>
    internal const int BeamYAccelerations = 355;
}
