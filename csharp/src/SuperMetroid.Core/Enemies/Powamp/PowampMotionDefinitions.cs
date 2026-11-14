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

    /// <summary>
    /// The order a burst fires its eight spikes in, anticlockwise from up and left round to straight up; each spike takes
    /// the next free place in the projectile pool.
    /// </summary>
    internal static readonly IReadOnlyList<CompassDirection> SpikeFiringOrder =
    [
        CompassDirection.UpLeft, CompassDirection.Left, CompassDirection.DownLeft, CompassDirection.Down,
        CompassDirection.DownRight, CompassDirection.Right, CompassDirection.UpRight, CompassDirection.Up,
    ];

    /// <summary>
    /// The signed 1/256-pixel acceleration of a spike flying in each direction, ±32 on each axis the direction leans along.
    /// </summary>
    internal static readonly IReadOnlyDictionary<CompassDirection, EightEightVector<RoomPixels>> SpikeAccelerations =
        new Dictionary<CompassDirection, EightEightVector<RoomPixels>>
        {
            [CompassDirection.Up] = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, -32),
            [CompassDirection.UpRight] = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(32, -32),
            [CompassDirection.Right] = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(32, 0),
            [CompassDirection.DownRight] = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(32, 32),
            [CompassDirection.Down] = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, 32),
            [CompassDirection.DownLeft] = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-32, 32),
            [CompassDirection.Left] = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-32, 0),
            [CompassDirection.UpLeft] = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-32, -32),
        };

    /// <summary>
    /// The inflate stage a sinking balloon restarts when Powamp dies, by the sinking poses it had shown: after two it
    /// restarts inflating from halfway, after three from deflated. One that had shown a single sinking pose keeps sinking.
    /// </summary>
    internal static readonly IReadOnlyDictionary<int, PowampSegment> SunkBalloonRestarts = new Dictionary<int, PowampSegment>
    {
        [2] = PowampSegment.BalloonInflatingFromHalfway,
        [3] = PowampSegment.BalloonInflatingFromDeflated,
    };
}
