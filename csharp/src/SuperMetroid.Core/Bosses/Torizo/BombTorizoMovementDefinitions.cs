namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Fixed posture and walking movement definitions for Bomb Torizo.</summary>
internal static class BombTorizoMovementDefinitions
{
    /// <summary>
    /// The sixteen X words at <c>$AA:C3EE-$AA:C40D</c> and eight wrapping Y words at
    /// <c>$AA:C40E-$AA:C41D</c>. The sitting-down copies at <c>$AA:C440-$AA:C46F</c>
    /// are byte-identical and are subtracted by the caller. Left-facing per-frame
    /// displacements are authored to the posture animation, including its uneven steps;
    /// right-facing movement reflects their horizontal sign rather than storing a second table.
    /// </summary>
    internal static readonly short[] LeftPostureX = [-9, -6, -7, 5, -16, -7, 0, 0];

    /// <summary>Eight authored vertical posture offsets repeat for both Bomb Torizo facings.</summary>
    internal static readonly short[] PostureY =
        [0, -6, -6, -7, 0, 0, 0, 0];

    /// <summary>
    /// The twenty velocities duplicated at <c>$AA:C4BD-$AA:C4E4</c> and
    /// <c>$AA:C532-$AA:C559</c> by the normal and faceless walking instructions.
    /// These ten authored stride magnitudes share one table because facing only changes their sign.
    /// </summary>
    internal static readonly short[] LeftWalkVelocities = [-5, 0, -5, -19, -16, -7, 0, -7, -17, -18];

    /// <summary>The posture byte offset bits that wrap the eight vertical offsets.</summary>
    internal const int PostureYOffsetMask = 0x000f;
}
