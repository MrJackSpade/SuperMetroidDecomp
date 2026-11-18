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
    /// <summary>Initializes the 32-update forward segment without moving yet.</summary>
    ActivateMoveForwardThenWait,
    /// <summary>Adds the precomputed forward magnitudes with signs derived from the spawn angle.</summary>
    MoveForward,
    /// <summary>Selects side velocity and facing by whether the enemy frame counter is in the second half of each 32-frame cycle.</summary>
    ZigZag,
    /// <summary>Counts the segment down before moving and returns to proximity waiting on the update it runs out.</summary>
    MoveForwardThenWait,
}
