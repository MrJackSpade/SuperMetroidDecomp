namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The motion profile of each piece of Crocomire's falling spike wall, by its place from the top piece.</summary>
internal static class CrocomireSpikeMotionDefinitions
{
    /// <summary>The pieces the wall breaks into, the top piece first.</summary>
    internal const int PieceCount = 8;

    /// <summary>The row a falling piece lands on and crumbles, inclusive (#2182).</summary>
    internal const int LandingY = 168;

    /// <summary>The speed a falling piece gains downward each update: three sixteenths of a pixel per update.</summary>
    internal static readonly SignedSixteenSixteen PieceGravity = SignedSixteenSixteen.FromSixteenthsOfPixel(3);

    /// <summary>
    /// How much each piece's sideways acceleration grows each update, in pixels per update per update, by its place from
    /// the top (#2181): a ramp rising by the same step from the bottom piece up, the top piece's held just below a
    /// sixteenth of a pixel instead of taking the ramp's next step.
    /// </summary>
    internal static readonly SignedSixteenSixteen[] AccelerationDeltas =
    [
        SignedSixteenSixteen.FromParts(0, 4080), SignedSixteenSixteen.FromParts(0, 3808),
        SignedSixteenSixteen.FromParts(0, 3264), SignedSixteenSixteen.FromParts(0, 2720),
        SignedSixteenSixteen.FromParts(0, 2176), SignedSixteenSixteen.FromParts(0, 1632),
        SignedSixteenSixteen.FromParts(0, 1088), SignedSixteenSixteen.FromParts(0, 544),
    ];

    /// <summary>
    /// Each piece's acceleration limit is this many times its acceleration's growth. The top piece's held growth keeps the
    /// largest limit just under a pixel per update per update.
    /// </summary>
    internal const int MaximumAccelerationPerIncrement = 16;

    /// <summary>
    /// The fastest whole-pixel horizontal speed, in pixels per frame, of each piece from the top: the top four
    /// pieces descend one unit a piece from six, the bottom four one unit a pair.
    /// </summary>
    internal static readonly int[] MaximumVelocities =
    [
        6, 5, 4, 3, 2, 2, 1, 1,
    ];
}
