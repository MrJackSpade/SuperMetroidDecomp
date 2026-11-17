namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Fixed initialization definitions for the three escape Etecoon actors.</summary>
internal static class EscapeEtecoonDefinitions
{

    /// <summary>
    /// The position, action, animation, walking direction and speed each of the
    /// left walker, right walker and event-waiting roles starts with.
    /// </summary>
    internal static readonly Dictionary<EscapeEtecoonRole, EscapeEtecoonInitialization> Initializations = new()
    {
        [EscapeEtecoonRole.LeftWalker] = new(new(128, 200), EscapeEtecoonAction.Walking,
            EscapeEtecoonSegment.RunningLeftLowTide, HorizontalDirection.Left, SignedEightEight.FromTwoHundredFiftySixthsOfPixel(512)),
        [EscapeEtecoonRole.RightWalker] = new(new(160, 200), EscapeEtecoonAction.Walking,
            EscapeEtecoonSegment.RunningRightLowTide, HorizontalDirection.Right, SignedEightEight.FromTwoHundredFiftySixthsOfPixel(640)),
        [EscapeEtecoonRole.WaitForEscapeEvent] = new(new(232, 200), EscapeEtecoonAction.AwaitingEscape,
            EscapeEtecoonSegment.Stationary, HorizontalDirection.Right, default),
    };
}
