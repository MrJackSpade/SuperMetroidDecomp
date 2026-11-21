using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>Powamp balloon motion and clockwise spike acceleration.</summary>
internal static class PowampMotionDefinitions
{
    /// <summary>
    /// The wiggle's twelve sideways offsets in the order the Powamp takes them (<see cref="PowampWiggle"/>), a signed
    /// three-pixel triangle wave centred at the first and the seventh.
    /// </summary>
    internal static readonly int[] WiggleOffsets = [0, 1, 2, 3, 2, 1, 0, -1, -2, -3, -2, -1];

    /// <summary>The pose each of the balloon's frames shows it in.</summary>
    internal static readonly IdentityTable<EnemyFrameId, PowampBalloonPose> PosesShownBy = new(new Dictionary<EnemyFrameId, PowampBalloonPose>
    {
        [EnemyFrameId.PowampBalloonSmall] = PowampBalloonPose.Deflated,
        [EnemyFrameId.PowampBalloonHalfInflated] = PowampBalloonPose.Halfway,
        [EnemyFrameId.PowampBalloonInflated] = PowampBalloonPose.Full,
    });

    /// <summary>How far above the body the balloon sits in each pose: four pixels higher per pose it inflates.</summary>
    internal static readonly IdentityTable<PowampBalloonPose, int> BalloonHeights = new(new Dictionary<PowampBalloonPose, int>
    {
        [PowampBalloonPose.Deflated] = -12,
        [PowampBalloonPose.Halfway] = -16,
        [PowampBalloonPose.Full] = -20,
    });

    /// <summary>
    /// The balloon inflating from deflated (#1845): each start below is one of the balloon animation's starts, with where it
    /// puts the balloon's sequence.
    /// </summary>
    internal static PowampBalloonStart InflatingFromDeflated { get; } =
        new(PowampSegment.BalloonInflatingFromDeflated, new(PowampBalloonSequence.Inflating, PowampBalloonPose.Deflated));

    /// <summary>The balloon inflating from halfway, standing deflated until its halfway frame shows.</summary>
    internal static PowampBalloonStart InflatingFromHalfway { get; } =
        new(PowampSegment.BalloonInflatingFromHalfway, new(PowampBalloonSequence.Inflating, PowampBalloonPose.Deflated));

    /// <summary>The balloon starting to sink from full.</summary>
    internal static PowampBalloonStart StartSinking { get; } =
        new(PowampSegment.BalloonStartSinking, new(PowampBalloonSequence.Sinking, PowampBalloonPose.Full));

    /// <summary>The balloon resting deflated at the end of its sink, standing halfway until its deflated frame shows.</summary>
    internal static PowampBalloonStart Deflated { get; } =
        new(PowampSegment.BalloonDeflated, new(PowampBalloonSequence.Sinking, PowampBalloonPose.Halfway));

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
    /// The acceleration of a spike flying in each direction: an eighth of a pixel an update, each update, along each axis
    /// the direction leans along.
    /// </summary>
    internal static readonly IdentityTable<CompassDirection, PixelVelocity<RoomPixels>> SpikeAccelerations = new(new Dictionary<CompassDirection, PixelVelocity<RoomPixels>>
        {
            [CompassDirection.Up] = Leaning(new(0, -1)),
            [CompassDirection.UpRight] = Leaning(new(1, -1)),
            [CompassDirection.Right] = Leaning(new(1, 0)),
            [CompassDirection.DownRight] = Leaning(new(1, 1)),
            [CompassDirection.Down] = Leaning(new(0, 1)),
            [CompassDirection.DownLeft] = Leaning(new(-1, 1)),
            [CompassDirection.Left] = Leaning(new(-1, 0)),
            [CompassDirection.UpLeft] = Leaning(new(-1, -1)),
        });

    /// <summary>
    /// The inflate stage a sinking balloon restarts when Powamp dies, by the pose it has sunk to (#1839): sunk halfway it
    /// restarts inflating from halfway, sunk deflated from deflated. One still full keeps sinking.
    /// </summary>
    internal static readonly IReadOnlyDictionary<PowampBalloonPose, PowampBalloonStart> SunkBalloonRestarts = new Dictionary<PowampBalloonPose, PowampBalloonStart>
    {
        [PowampBalloonPose.Halfway] = InflatingFromHalfway,
        [PowampBalloonPose.Deflated] = InflatingFromDeflated,
    };

    /// <summary>How far an ungrappled Powamp's body rises from its balloon's spawn height, in pixels.</summary>
    internal const int UngrappledRise = 64;

    /// <summary>How long the body rests underground before its balloon inflates again, in updates.</summary>
    internal const int RestDuration = 60;

    /// <summary>How long the balloon takes to inflate or deflate, in updates.</summary>
    internal const int InflationDuration = 10;

    /// <summary>How long after its fatal hit the balloon bursts into spikes and is deleted, in updates.</summary>
    internal const int DeathDelay = 32;

    /// <summary>How long the body and balloon hold each sideways wiggle offset, in updates.</summary>
    internal const int WiggleStepDuration = 5;

    /// <summary>The updates a placed body waits for its first wiggle step: one, so its first wiggle update steps.</summary>
    internal const int FirstWiggleStepWait = 1;

    /// <summary>The speed the body rises at once its balloon has inflated: half a pixel an update, upward.</summary>
    internal static SignedSixteenSixteen RiseVelocity { get; } = SignedSixteenSixteen.FromPixelFraction(-1, 2);

    /// <summary>The speed the body sinks back to its spawn height at once its balloon has deflated: a pixel an update.</summary>
    internal static SignedSixteenSixteen SinkVelocity { get; } = SignedSixteenSixteen.FromWholePixels(1);

    /// <summary>
    /// A spike's acceleration leaning <paramref name="lean"/>, each axis -1, 0 or 1: an eighth of a pixel an update, each
    /// update, along each axis it leans along.
    /// </summary>
    private static PixelVelocity<RoomPixels> Leaning(Offset16<RoomPixels> lean) =>
        new(SignedEightEight.FromPixelFraction(lean.X, 8), SignedEightEight.FromPixelFraction(lean.Y, 8));
}
