namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>
/// The behaviour a Sbug enemy's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum SbugAction
{
    /// <summary>Selects an activation handler only when both axis distances are strictly below the spawn radius.</summary>
    WaitForSamus = 1,
    /// <summary>Spends one AI invocation selecting continuous forward movement.</summary>
    ActivateMoveForward,
    /// <summary>Selects alternating side movement without moving on this invocation.</summary>
    ActivateZigZag,
    /// <summary>Resets shared RNG to 11 and selects the collision-aware random-direction chooser.</summary>
    ActivateRandomUntilCollision,
    /// <summary>Sets the 512-update long countdown, resets shared RNG to 11, and selects its direction chooser.</summary>
    ActivateRandomAndReverseWhenFar,
    /// <summary>Initializes the 32-update forward segment without moving yet.</summary>
    ActivateMoveForwardThenWait,
    /// <summary>Computes and installs a fixed velocity and facing toward Samus without moving yet.</summary>
    ActivateMoveTowardSamus,
    /// <summary>Computes and installs the opposite velocity and facing without moving yet.</summary>
    ActivateMoveAwayFromSamus,
    /// <summary>Adds the precomputed forward magnitudes with signs derived from the spawn angle.</summary>
    MoveForward,
    /// <summary>Selects side velocity and facing by whether the enemy frame counter is in the second half of each 32-frame cycle.</summary>
    ZigZag,
    /// <summary>Applies the captured custom velocity without retargeting or block collision checks.</summary>
    MoveTowardSamus,
    /// <summary>Applies the captured away velocity without retargeting or block collision checks.</summary>
    MoveAwayFromSamus,
    /// <summary>Decrements the segment timer before moving and returns to proximity waiting once it passes zero.</summary>
    MoveForwardThenWait,
    /// <summary>Runs a timed, collision-aware segment, waiting on collision or choosing another angle when the timer passes zero.</summary>
    MoveRandomlyUntilCollision,
    /// <summary>Applies custom movement and, while the long countdown is nonnegative, checks segment expiry and the both-axis 96-pixel reversal condition.</summary>
    MoveStraightAndReverseWhenFar,
    /// <summary>Perturbs the custom angle using shared RNG, installs velocity and facing, and starts a 32-update reversal-aware segment.</summary>
    ChooseRandomDirectionAndReverseWhenFar,
    /// <summary>Perturbs the custom angle using shared RNG, installs velocity and facing, and starts a 32-update collision-aware segment.</summary>
    ChooseRandomDirectionUntilCollision,
}
