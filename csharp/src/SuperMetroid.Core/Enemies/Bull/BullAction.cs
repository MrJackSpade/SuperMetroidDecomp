namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// The behaviour Bull's main AI runs each frame.
/// Its values are arbitrary.
/// </summary>
public enum BullAction
{
    /// <summary>Waits for the sixteen-update activation countdown to reach zero, reloads it, and selects Samus targeting.</summary>
    MovementDelay,
    /// <summary>Takes the heading toward Samus and starts speeding up from the next update, without moving on this one.</summary>
    TargetSamus,
    /// <summary>Speeds up each acceleration interval while below the top speed, flies along the heading, and starts braking once Samus's bearing has turned <see cref="BullMovementDefinitions.OffTargetTurn"/> from it.</summary>
    Accelerating,
    /// <summary>Flies, then slows each braking interval; once the speed reaches zero or the braking runs out, the Bull comes to rest and waits out the movement delay again.</summary>
    Decelerating,
}
