namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>Facings, facing animations and activation actions for Sbug movement.</summary>
internal static partial class SbugMovementDefinitions
{
    /// <summary>
    /// The facing a heading shows for each eighth of the turn it lies in: counter-clockwise from right, the first eighth
    /// opening at a heading of 48 steps.
    /// </summary>
    internal static readonly TurnArcs<CompassDirection> Facings = new(new TrigonometryAngle(48),
        CompassDirection.Right, CompassDirection.UpRight, CompassDirection.Up, CompassDirection.UpLeft,
        CompassDirection.Left, CompassDirection.DownLeft, CompassDirection.Down, CompassDirection.DownRight);

    /// <summary>The action each of the three population-selected activation behaviors starts.</summary>
    internal static readonly IdentityTable<SbugActivationBehavior, SbugAction> ActivationActions = new(new Dictionary<SbugActivationBehavior, SbugAction>
    {
        [SbugActivationBehavior.MoveForward] = SbugAction.ActivateMoveForward,
        [SbugActivationBehavior.ZigZag] = SbugAction.ActivateZigZag,
        [SbugActivationBehavior.MoveForwardThenWait] = SbugAction.ActivateMoveForwardThenWait,
    });

    /// <summary>How long each forward-then-wait leg lasts, in updates, the last the one the Sbug stops on.</summary>
    internal const int LegDuration = 33;
}
