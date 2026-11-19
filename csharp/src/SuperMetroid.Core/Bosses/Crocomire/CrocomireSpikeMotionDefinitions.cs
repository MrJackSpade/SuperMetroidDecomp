namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The pieces of Crocomire's falling spike wall and how they fall.</summary>
internal static class CrocomireSpikeMotionDefinitions
{
    /// <summary>The row a falling piece lands on and crumbles, inclusive (#2182).</summary>
    internal const int LandingY = 168;

    /// <summary>The speed a falling piece gains downward each update: three sixteenths of a pixel per update.</summary>
    internal static readonly SignedSixteenSixteen PieceGravity = SignedSixteenSixteen.FromSixteenthsOfPixel(3);

    /// <summary>
    /// Each piece's greatest sideways acceleration is this many times its acceleration's growth. The top piece's held
    /// growth keeps the largest just under a pixel per update per update.
    /// </summary>
    internal const int MaximumAccelerationPerIncrement = 16;

    /// <summary>
    /// The step by which the pieces' acceleration growth rises from one piece to the piece above it, from the bottom piece,
    /// which grows by one step (#2181).
    /// </summary>
    private static readonly SignedSixteenSixteen GrowthStep = SignedSixteenSixteen.FromPixelFraction(17, 2048);

    /// <summary>
    /// The wall's eight pieces, the top piece first, each a spacing of a block below the piece above it (#2539). Their
    /// acceleration growth ramps by <see cref="GrowthStep"/> from the bottom piece up, the top piece's held just below a
    /// sixteenth of a pixel instead of taking the ramp's next step; their fastest speeds descend one pixel a piece from six
    /// for the top four and one a pair for the bottom four; and every other piece, the top piece first, crumbles audibly as
    /// it lands, so the wall's landings queue four crumbles.
    /// </summary>
    internal static IReadOnlyList<CrocomireSpikeWallPiece> Pieces { get; } =
    [
        new(SignedSixteenSixteen.FromPixelFraction(255, 4096), FastestSpeed: 6, Depth: 0, CrumbleSound: CrocomireSoundDefinitions.Crumble),
        new(GrowthStep * 7, FastestSpeed: 5, Depth: 16, CrumbleSound: null),
        new(GrowthStep * 6, FastestSpeed: 4, Depth: 32, CrumbleSound: CrocomireSoundDefinitions.Crumble),
        new(GrowthStep * 5, FastestSpeed: 3, Depth: 48, CrumbleSound: null),
        new(GrowthStep * 4, FastestSpeed: 2, Depth: 64, CrumbleSound: CrocomireSoundDefinitions.Crumble),
        new(GrowthStep * 3, FastestSpeed: 2, Depth: 80, CrumbleSound: null),
        new(GrowthStep * 2, FastestSpeed: 1, Depth: 96, CrumbleSound: CrocomireSoundDefinitions.Crumble),
        new(GrowthStep * 1, FastestSpeed: 1, Depth: 112, CrumbleSound: null),
    ];
}
