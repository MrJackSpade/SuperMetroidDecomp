namespace SuperMetroid.Core.Game;

/// <summary>Facing-program and activation-function selection for Sbug movement.</summary>
internal static class SbugMovementDefinitions
{
    /// <summary>
    /// $A3:A111: eight directional programs, each four duration/frame records plus
    /// a goto pair (twenty bytes). Native odd reversal indices discard bit zero.
    /// </summary>
    internal static ushort FacingInstructionList(ushort directionIndex)
    {
        if (directionIndex > 0x000f)
            throw new InvalidDataException(
                $"Sbug direction index ${directionIndex:X4} is outside the eight native selectors.");
        return (ushort)(SbugInstructionProgramDefinitions.Right + 20 * (directionIndex >> 1));
    }

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
            $"Sbug activation behavior {(byte)behavior} is outside the seven native selectors."),
    };
}