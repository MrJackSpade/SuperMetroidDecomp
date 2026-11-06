namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>Facing-program and activation-function selection for Sbug movement.</summary>
internal static class SbugMovementDefinitions
{
    /// <summary>The eight directional loops, by facing counter-clockwise from right.</summary>
    internal static readonly SbugProgram[] FacingPrograms =
    [
        SbugProgram.Right, SbugProgram.UpRight, SbugProgram.Up, SbugProgram.UpLeft,
        SbugProgram.Left, SbugProgram.DownLeft, SbugProgram.Down, SbugProgram.DownRight,
    ];

    /// <summary>The function each of the seven population-selected activation behaviors starts.</summary>
    internal static readonly Dictionary<SbugActivationBehavior, SbugEnemyFunction> ActivationFunctions = new()
    {
        [SbugActivationBehavior.MoveForward] = SbugEnemyFunction.ActivateMoveForward,
        [SbugActivationBehavior.ZigZag] = SbugEnemyFunction.ActivateZigZag,
        [SbugActivationBehavior.MoveTowardSamus] = SbugEnemyFunction.ActivateMoveTowardSamus,
        [SbugActivationBehavior.RandomUntilCollision] = SbugEnemyFunction.ActivateRandomUntilCollision,
        [SbugActivationBehavior.RandomAndReverseWhenFar] = SbugEnemyFunction.ActivateRandomAndReverseWhenFar,
        [SbugActivationBehavior.MoveForwardThenWait] = SbugEnemyFunction.ActivateMoveForwardThenWait,
        [SbugActivationBehavior.MoveAwayFromSamus] = SbugEnemyFunction.ActivateMoveAwayFromSamus,
    };
}
