namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>Facing-program and activation-function selection for Sbug movement.</summary>
internal static class SbugMovementDefinitions
{
    /// <summary>
    /// The random field that turns an Sbug starting a random movement leg: bits zero to seven, 0 through 255, less a
    /// quarter turn, added to its heading.
    /// </summary>
    internal static readonly RandomSampleField RandomTurn = new(0, 8);

    /// <summary>
    /// The facing a heading shows for each eighth of the turn it lies in: counter-clockwise from right, the first eighth
    /// opening at a heading of 48 steps.
    /// </summary>
    internal static readonly TurnArcs<SbugFacing> Facings = new(new TrigonometryAngle(48),
        SbugFacing.Right, SbugFacing.UpRight, SbugFacing.Up, SbugFacing.UpLeft,
        SbugFacing.Left, SbugFacing.DownLeft, SbugFacing.Down, SbugFacing.DownRight);

    /// <summary>The directional loop each facing shows.</summary>
    internal static readonly Dictionary<SbugFacing, SbugSegment> FacingAnimations = new()
    {
        [SbugFacing.Right] = SbugSegment.Right,
        [SbugFacing.UpRight] = SbugSegment.UpRight,
        [SbugFacing.Up] = SbugSegment.Up,
        [SbugFacing.UpLeft] = SbugSegment.UpLeft,
        [SbugFacing.Left] = SbugSegment.Left,
        [SbugFacing.DownLeft] = SbugSegment.DownLeft,
        [SbugFacing.Down] = SbugSegment.Down,
        [SbugFacing.DownRight] = SbugSegment.DownRight,
    };

    /// <summary>
    /// The facing a reversal of the custom heading shows: not the opposite facing, but the facing a quarter turn
    /// counter-clockwise, folded into the four facings from right to up-left, as the game does.
    /// </summary>
    internal static readonly Dictionary<SbugFacing, SbugFacing> ReversedFacings = new()
    {
        [SbugFacing.Right] = SbugFacing.Up,
        [SbugFacing.UpRight] = SbugFacing.UpLeft,
        [SbugFacing.Up] = SbugFacing.Right,
        [SbugFacing.UpLeft] = SbugFacing.UpRight,
        [SbugFacing.Left] = SbugFacing.Up,
        [SbugFacing.DownLeft] = SbugFacing.UpLeft,
        [SbugFacing.Down] = SbugFacing.Right,
        [SbugFacing.DownRight] = SbugFacing.UpRight,
    };

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
