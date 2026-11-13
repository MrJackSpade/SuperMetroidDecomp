namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>Facing animations and activation actions for Sbug movement.</summary>
internal static class SbugMovementDefinitions
{
    /// <summary>
    /// The random field that turns an Sbug starting a random movement leg: the sample modulo 256, 0 through 255, less a
    /// quarter turn, added to its heading.
    /// </summary>
    internal static readonly RandomSampleField RandomTurn = new(0, 8);

    /// <summary>
    /// The facing a heading shows for each eighth of the turn it lies in: counter-clockwise from right, the first eighth
    /// opening at a heading of 48 steps.
    /// </summary>
    internal static readonly TurnArcs<CompassDirection> Facings = new(new TrigonometryAngle(48),
        CompassDirection.Right, CompassDirection.UpRight, CompassDirection.Up, CompassDirection.UpLeft,
        CompassDirection.Left, CompassDirection.DownLeft, CompassDirection.Down, CompassDirection.DownRight);

    /// <summary>The directional loop each facing shows.</summary>
    internal static readonly Dictionary<CompassDirection, SbugSegment> FacingAnimations = new()
    {
        [CompassDirection.Right] = SbugSegment.Right,
        [CompassDirection.UpRight] = SbugSegment.UpRight,
        [CompassDirection.Up] = SbugSegment.Up,
        [CompassDirection.UpLeft] = SbugSegment.UpLeft,
        [CompassDirection.Left] = SbugSegment.Left,
        [CompassDirection.DownLeft] = SbugSegment.DownLeft,
        [CompassDirection.Down] = SbugSegment.Down,
        [CompassDirection.DownRight] = SbugSegment.DownRight,
    };

    /// <summary>
    /// The facing a reversal of the custom heading shows: not the opposite facing, but the facing a quarter turn
    /// counter-clockwise, folded into the four facings from right to up-left, as the game does.
    /// </summary>
    internal static readonly Dictionary<CompassDirection, CompassDirection> ReversedFacings = new()
    {
        [CompassDirection.Right] = CompassDirection.Up,
        [CompassDirection.UpRight] = CompassDirection.UpLeft,
        [CompassDirection.Up] = CompassDirection.Right,
        [CompassDirection.UpLeft] = CompassDirection.UpRight,
        [CompassDirection.Left] = CompassDirection.Up,
        [CompassDirection.DownLeft] = CompassDirection.UpLeft,
        [CompassDirection.Down] = CompassDirection.Right,
        [CompassDirection.DownRight] = CompassDirection.UpRight,
    };

    /// <summary>The action each of the seven population-selected activation behaviors starts.</summary>
    internal static readonly Dictionary<SbugActivationBehavior, SbugAction> ActivationActions = new()
    {
        [SbugActivationBehavior.MoveForward] = SbugAction.ActivateMoveForward,
        [SbugActivationBehavior.ZigZag] = SbugAction.ActivateZigZag,
        [SbugActivationBehavior.MoveTowardSamus] = SbugAction.ActivateMoveTowardSamus,
        [SbugActivationBehavior.RandomUntilCollision] = SbugAction.ActivateRandomUntilCollision,
        [SbugActivationBehavior.RandomAndReverseWhenFar] = SbugAction.ActivateRandomAndReverseWhenFar,
        [SbugActivationBehavior.MoveForwardThenWait] = SbugAction.ActivateMoveForwardThenWait,
        [SbugActivationBehavior.MoveAwayFromSamus] = SbugAction.ActivateMoveAwayFromSamus,
    };
}
