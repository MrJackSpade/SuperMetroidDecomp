namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Compiled physical definitions for Mother Brain's eight escape-door fragments.</summary>
internal static class MotherBrainDoorFragmentDefinitions
{
    /// <summary>
    /// The eight fragments the exploding escape door throws, in the order they spawn: each starts on the door's centre
    /// line, eight pixels below the one before from 32 above the door, and flies right five pixels an update at its own
    /// vertical scatter speed.
    /// </summary>
    internal static IReadOnlyList<MotherBrainDoorFragmentDefinition> Fragments { get; } =
    [
        Fragment(-32, SignedEightEight.FromPixels(-2)),
        Fragment(-24, SignedEightEight.FromPixels(-1)),
        Fragment(-16, SignedEightEight.FromPixels(-1)),
        Fragment(-8, SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-128)),
        Fragment(0, SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-128)),
        Fragment(8, SignedEightEight.FromTwoHundredFiftySixthsOfPixel(128)),
        Fragment(16, SignedEightEight.FromPixels(-1)),
        Fragment(24, SignedEightEight.FromPixels(2)),
    ];

    /// <summary>A fragment starting <paramref name="y"/> pixels below the door that flies right at <paramref name="ySpeed"/>.</summary>
    private static MotherBrainDoorFragmentDefinition Fragment(int y, SignedEightEight ySpeed) =>
        new(new Offset16<RoomPixels>(0, y), new EightEightVector<RoomPixels>(SignedEightEight.FromPixels(5), ySpeed));
}
