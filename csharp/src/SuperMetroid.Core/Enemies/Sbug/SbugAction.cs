namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>
/// The behaviour a Sbug enemy's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum SbugAction
{
    /// <summary>Selects an activation action only when both axis distances are strictly below the spawn radius.</summary>
    WaitForSamus,
    /// <summary>Starts moving straight ahead from the next update.</summary>
    ActivateMoveForward,
    /// <summary>Starts zig-zagging from the next update.</summary>
    ActivateZigZag,
    /// <summary>Reseeds the room's random draws for the random wander and chooses the wander's first heading on the next update; each leg ends on a collision.</summary>
    ActivateRandomUntilCollision,
    /// <summary>Starts the 512-update long countdown, reseeds the room's random draws for the random wander and chooses the wander's first heading on the next update.</summary>
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
    /// <summary>Moves along the spawn heading for its leg, then waits for Samus to come near again.</summary>
    MoveForwardThenWait,
    /// <summary>Moves along the custom heading for its leg, waiting for Samus on a collision or choosing another heading when the leg runs out.</summary>
    MoveRandomlyUntilCollision,
    /// <summary>Moves along the custom heading and, until the long countdown passes zero, chooses another heading when the leg runs out and reverses when Samus is at least 96 pixels away on both axes.</summary>
    MoveStraightAndReverseWhenFar,
    /// <summary>Perturbs the custom angle using shared RNG, installs velocity and facing, and starts a 32-update reversal-aware segment.</summary>
    ChooseRandomDirectionAndReverseWhenFar,
    /// <summary>Perturbs the custom angle using shared RNG, installs velocity and facing, and starts a 32-update collision-aware segment.</summary>
    ChooseRandomDirectionUntilCollision,
}
