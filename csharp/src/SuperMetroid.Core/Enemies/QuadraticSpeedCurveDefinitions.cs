using System.Diagnostics.CodeAnalysis;
using static SuperMetroid.Core.Spatial.SignedSixteenSixteen;

namespace SuperMetroid.Core.Enemies;

/// <summary>The shared quadratic enemy speed curve's authored speeds, which grow along the curve.</summary>
/// <remarks>
/// The speeds do not climb evenly (#2344): a few marks are slower than the mark before them.
/// </remarks>
public static class QuadraticSpeedCurveDefinitions
{
    /// <summary>
    /// The speed at each mark along the curve, its start through its last mark, toward increasing coordinates, in pixels
    /// per frame to a fraction of a pixel.
    /// </summary>
    [SuppressMessage("Design", "SME6447", Justification = "Each speed's fraction is the game's own fraction of a pixel, written exactly; only the size of the fractions above half a pixel makes this rule flag them.")]
    internal static readonly QuadraticCurveSpeed[] Speeds =
    [
        new(FromParts(0, 0)), new(FromParts(0, 265)), new(FromParts(0, 795)), new(FromParts(0, 1590)), new(FromParts(0, 2650)),
        new(FromParts(0, 3975)), new(FromParts(0, 5565)), new(FromParts(0, 7420)), new(FromParts(0, 9540)), new(FromParts(0, 11925)),
        new(FromParts(0, 14575)), new(FromParts(0, 17490)), new(FromParts(0, 20670)), new(FromParts(0, 24115)), new(FromParts(0, 27825)),
        new(FromParts(0, 31800)), new(FromParts(0, 36040)), new(FromParts(0, 40545)), new(FromParts(0, 45315)), new(FromParts(0, 50350)),
        new(FromParts(0, 55650)), new(FromParts(0, 61215)), new(FromParts(0, 1509)), new(FromParts(1, 5300)), new(FromParts(1, 11660)),
        new(FromParts(1, 18285)), new(FromParts(1, 25175)), new(FromParts(1, 32330)), new(FromParts(1, 39750)), new(FromParts(1, 47435)),
        new(FromParts(1, 55385)), new(FromParts(1, 63600)), new(FromParts(2, 4240)), new(FromParts(2, 12985)), new(FromParts(2, 21995)),
        new(FromParts(2, 31270)), new(FromParts(2, 40810)), new(FromParts(2, 50615)), new(FromParts(2, 60685)), new(FromParts(3, 3180)),
        new(FromParts(3, 13780)), new(FromParts(3, 24645)), new(FromParts(3, 35775)), new(FromParts(3, 47170)), new(FromParts(3, 58830)),
        new(FromParts(4, 2915)), new(FromParts(4, 15105)), new(FromParts(4, 27560)), new(FromParts(4, 40280)), new(FromParts(4, 53265)),
        new(FromParts(4, 979)), new(FromParts(5, 12190)), new(FromParts(5, 25970)), new(FromParts(5, 40015)), new(FromParts(5, 54325)),
        new(FromParts(6, 1060)), new(FromParts(6, 15900)), new(FromParts(6, 31005)), new(FromParts(6, 46375)), new(FromParts(6, 62010)),
        new(FromParts(7, 10070)), new(FromParts(7, 26235)), new(FromParts(7, 42665)), new(FromParts(7, 59360)), new(FromParts(8, 8480)),
        new(FromParts(8, 25705)), new(FromParts(8, 43195)), new(FromParts(8, 60950)), new(FromParts(9, 11130)), new(FromParts(9, 29415)),
        new(FromParts(9, 47965)), new(FromParts(9, 1244)), new(FromParts(10, 18020)), new(FromParts(10, 37365)), new(FromParts(10, 56975)),
        new(FromParts(11, 9010)), new(FromParts(11, 29150)), new(FromParts(11, 49555)), new(FromParts(12, 2385)), new(FromParts(12, 23320)),
        new(FromParts(12, 44520)), new(FromParts(12, 449)), new(FromParts(13, 19875)), new(FromParts(13, 41870)), new(FromParts(13, 64130)),
        new(FromParts(14, 18815)), new(FromParts(14, 41605)), new(FromParts(14, 64660)), new(FromParts(15, 20140)), new(FromParts(15, 43725)),
        new(FromParts(15, 2039)), new(FromParts(16, 23850)), new(FromParts(16, 48230)), new(FromParts(17, 5035)), new(FromParts(17, 29945)),
    ];

    /// <summary>The number of marks along the curve, its start through its last mark.</summary>
    public static int MarkCount => Speeds.Length;
}
