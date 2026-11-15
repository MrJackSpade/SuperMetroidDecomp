namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Fixed initialization definitions for the three escape Etecoon actors.</summary>
internal static class EscapeEtecoonDefinitions
{

    /// <summary>
    /// The position, action, animation and horizontal speed each of the
    /// left walker, right walker and event-waiting roles starts with.
    /// </summary>
    internal static readonly Dictionary<EscapeEtecoonRole, EscapeEtecoonInitialization> Initializations = new()
    {
        [EscapeEtecoonRole.LeftWalker] = new(new(128, 200), EscapeEtecoonAction.Walking,
            EscapeEtecoonSegment.RunningLeftLowTide, EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-512, 0)),
        [EscapeEtecoonRole.RightWalker] = new(new(160, 200), EscapeEtecoonAction.Walking,
            EscapeEtecoonSegment.RunningRightLowTide, EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(640, 0)),
        [EscapeEtecoonRole.WaitForEscapeEvent] = new(new(232, 200), EscapeEtecoonAction.AwaitingEscape,
            EscapeEtecoonSegment.Stationary, default),
    };
}
