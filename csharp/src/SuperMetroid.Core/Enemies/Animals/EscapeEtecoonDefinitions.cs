namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Fixed initialization definitions for the three escape Etecoon actors.</summary>
internal static class EscapeEtecoonDefinitions
{

    /// <summary>
    /// The position, action, program and horizontal speed each of the
    /// left walker, right walker and event-waiting roles starts with.
    /// </summary>
    internal static readonly Dictionary<EscapeEtecoonRole, EscapeEtecoonInitialization> Initializations = new()
    {
        [EscapeEtecoonRole.LeftWalker] = new(new(128, 200), EscapeEtecoonFrameBehavior.WalkAndFall,
            EscapeEtecoonProgram.RunningLeftLowTide, EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-512, 0)),
        [EscapeEtecoonRole.RightWalker] = new(new(160, 200), EscapeEtecoonFrameBehavior.WalkAndFall,
            EscapeEtecoonProgram.RunningRightLowTide, EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(640, 0)),
        [EscapeEtecoonRole.WaitForEscapeEvent] = new(new(232, 200), EscapeEtecoonFrameBehavior.WaitForEscapeEvent,
            EscapeEtecoonProgram.Stationary, default),
    };
}
