namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Fixed posture and walking movement definitions for Bomb Torizo.</summary>
internal static class BombTorizoMovementDefinitions
{
    /// <summary>
    /// The sixteen X values and eight Y values, in pixels, of the posture
    /// movement. The sitting-down copies
    /// are identical and are subtracted by the caller. Left-facing per-frame
    /// displacements are authored to the posture animation, including its uneven steps;
    /// right-facing movement reflects their horizontal sign rather than storing a second table.
    /// </summary>
    internal static readonly int[] LeftPostureX = [-9, -6, -7, 5, -16, -7, 0, 0];

    /// <summary>Eight authored vertical posture offsets repeat for both Bomb Torizo facings.</summary>
    internal static readonly int[] PostureY =
        [0, -6, -6, -7, 0, 0, 0, 0];

    /// <summary>
    /// The twenty velocities used by the normal and faceless walking steps.
    /// These ten authored stride magnitudes share one table because facing only changes their sign.
    /// </summary>
    internal static readonly int[] LeftWalkVelocities = [-5, 0, -5, -19, -16, -7, 0, -7, -17, -18];
}
