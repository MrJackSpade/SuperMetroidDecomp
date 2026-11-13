namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>Powamp balloon motion and clockwise spike acceleration.</summary>
public static class PowampMotionDefinitions
{
    /// <summary>Twelve phases of the signed three-pixel triangle wave.</summary>
    public const int WigglePhaseCount = 12;

    /// <summary>The triangle wave's offset at each phase, centered at phases zero and six.</summary>
    internal static readonly int[] WiggleOffsets = [0, 1, 2, 3, 2, 1, 0, -1, -2, -3, -2, -1];

    /// <summary>The balloon's rising Y offsets: four pixels higher per inflation pose.</summary>
    internal static readonly int[] RisingBalloonOffsets = [-12, -16, -20];

    /// <summary>The balloon's sinking Y offsets: four pixels lower per inflation pose.</summary>
    internal static readonly int[] SinkingBalloonOffsets = [-20, -16, -12];

    /// <summary>Eight clockwise compass directions beginning up.</summary>
    public const int SpikeDirectionCount = 8;

    /// <summary>
    /// Each compass direction's signed 8.8 acceleration, ±32 on each
    /// axis the direction leans along. The Y component is the X component rotated two compass steps.
    /// </summary>
    internal static readonly EightEightVector<RoomPixels>[] SpikeAccelerations =
        [
            EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, -32),
            EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(32, -32),
            EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(32, 0),
            EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(32, 32),
            EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, 32),
            EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-32, 32),
            EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-32, 0),
            EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-32, -32),
        ];

    /// <summary>
    /// The inflate stage a sinking balloon restarts when Powamp dies, by the sinking poses it had shown: after two it
    /// restarts stage one, after three stage zero. One that had shown a single sinking pose keeps sinking.
    /// </summary>
    internal static readonly IReadOnlyDictionary<int, PowampProgram> SunkBalloonRestarts = new Dictionary<int, PowampProgram>
    {
        [2] = PowampProgram.BalloonInflate1,
        [3] = PowampProgram.BalloonInflate0,
    };
}
