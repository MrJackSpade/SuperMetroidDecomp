namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>Facing-program and activation-function selection for Sbug movement.</summary>
internal static class SbugMovementDefinitions
{
    /// <summary>The seven activation behaviours in the order the authored parameter-two high byte counts them.</summary>
    internal static readonly IdentitySequence<SbugActivationBehavior> ActivationBehaviors = new(
        SbugActivationBehavior.MoveForward, SbugActivationBehavior.ZigZag, SbugActivationBehavior.MoveTowardSamus,
        SbugActivationBehavior.RandomUntilCollision, SbugActivationBehavior.RandomAndReverseWhenFar,
        SbugActivationBehavior.MoveForwardThenWait, SbugActivationBehavior.MoveAwayFromSamus);

    /// <summary>$A3:A111: the eight directional loops, selected by facing counter-clockwise from right.</summary>
    internal static SbugProgram FacingProgram(int facing) => facing switch
    {
        0 => SbugProgram.Right,
        1 => SbugProgram.UpRight,
        2 => SbugProgram.Up,
        3 => SbugProgram.UpLeft,
        4 => SbugProgram.Left,
        5 => SbugProgram.DownLeft,
        6 => SbugProgram.Down,
        7 => SbugProgram.DownRight,
        _ => throw new InvalidDataException(
            $"Sbug facing {facing} is outside the eight native selectors."),
    };

    /// <summary>$A3:A121: dispatches the seven population-selected activation behaviors.</summary>
    internal static SbugEnemyFunction ActivationFunction(SbugActivationBehavior behavior) => behavior switch
    {
        SbugActivationBehavior.MoveForward => SbugEnemyFunction.ActivateMoveForward,
        SbugActivationBehavior.ZigZag => SbugEnemyFunction.ActivateZigZag,
        SbugActivationBehavior.MoveTowardSamus => SbugEnemyFunction.ActivateMoveTowardSamus,
        SbugActivationBehavior.RandomUntilCollision => SbugEnemyFunction.ActivateRandomUntilCollision,
        SbugActivationBehavior.RandomAndReverseWhenFar => SbugEnemyFunction.ActivateRandomAndReverseWhenFar,
        SbugActivationBehavior.MoveForwardThenWait => SbugEnemyFunction.ActivateMoveForwardThenWait,
        SbugActivationBehavior.MoveAwayFromSamus => SbugEnemyFunction.ActivateMoveAwayFromSamus,
        _ => throw new InvalidDataException(
            $"Sbug activation behavior {behavior} is outside the seven native selectors."),
    };
}