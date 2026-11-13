using System.Collections.Immutable;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The scrolling sky's bands (#1415, #1687): from the top of the room down, the rows each band covers and the drift it
/// slides sideways with. Most bands alternate between the slow and the fast drift; the bands at rows 568 and 736 share
/// the fastest drift, the band at row 1088 has the slowest, and the sky's bottom rows from 1168 stand still.
/// </summary>
public static class ScrollingSkyBandDefinitions
{
    /// <summary>The drift of the sky's bottom rows, which never move.</summary>
    public static SkyDrift Still { get; } = new(SignedSixteenSixteen.Zero);

    /// <summary>The slowest drift, seven sixteenths of a pixel a frame.</summary>
    public static SkyDrift Slowest { get; } = new(SignedSixteenSixteen.FromSixteenthsOfPixel(7));

    /// <summary>The slow drift, half a pixel a frame.</summary>
    public static SkyDrift Slow { get; } = new(SignedSixteenSixteen.FromSixteenthsOfPixel(8));

    /// <summary>The fast drift, three quarters of a pixel a frame.</summary>
    public static SkyDrift Fast { get; } = new(SignedSixteenSixteen.FromSixteenthsOfPixel(12));

    /// <summary>The fastest drift, a pixel and a quarter a frame, which two bands share.</summary>
    public static SkyDrift Fastest { get; } = new(SignedSixteenSixteen.FromSixteenthsOfPixel(20));

    /// <summary>Every drift the sky's bands move with.</summary>
    public static ImmutableArray<SkyDrift> Drifts { get; } = [Still, Slowest, Slow, Fast, Fastest];

    /// <summary>The sky's bands from the top of the room down, each ending where the next begins and the last at <see cref="EndRow"/>.</summary>
    public static ImmutableArray<SkyBand> Bands { get; } =
    [
        new(0, Slow), new(16, Fast), new(56, Slow), new(208, Fast),
        new(224, Slow), new(288, Fast), new(416, Slow), new(472, Fast),
        new(568, Fastest), new(616, Fast), new(672, Slow), new(736, Fastest),
        new(768, Slow), new(800, Fast), new(848, Slow), new(888, Fast),
        new(968, Slow), new(1088, Slowest), new(1120, Fast), new(1152, Slow),
        new(1168, Still),
    ];

    /// <summary>The room row below the sky's last band; the rows from it down show unshifted.</summary>
    public const int EndRow = 1280;
}
