using System.Diagnostics.CodeAnalysis;
using static SuperMetroid.Core.Spatial.SignedSixteenSixteen;

namespace SuperMetroid.Core.Enemies;

/// <summary>The shared quadratic enemy speed curve's authored speeds, which grow along the curve.</summary>
/// <remarks>
/// The speeds do not climb evenly (#2344): a few marks are slower than the mark before them.
/// </remarks>
public static partial class QuadraticSpeedCurveDefinitions
{
    /// <summary>
    /// The speed at each mark along the curve, its start through its last mark, toward increasing coordinates, in pixels
    /// per frame to a fraction of a pixel.
    /// </summary>
    internal static readonly QuadraticCurveSpeed[] Speeds =
    [
        new(Zero), new(FromPixelFraction(265, 65536)), new(FromPixelFraction(795, 65536)), new(FromPixelFraction(795, 32768)), new(FromPixelFraction(1325, 32768)),
        new(FromPixelFraction(3975, 65536)), new(FromPixelFraction(5565, 65536)), new(FromPixelFraction(1855, 16384)), new(FromPixelFraction(2385, 16384)), new(FromPixelFraction(11925, 65536)),
        new(FromPixelFraction(14575, 65536)), new(FromPixelFraction(8745, 32768)), new(FromPixelFraction(10335, 32768)), new(FromPixelFraction(24115, 65536)), new(FromPixelFraction(27825, 65536)),
        new(FromPixelFraction(3975, 8192)), new(FromPixelFraction(4505, 8192)), new(FromPixelFraction(40545, 65536)), new(FromPixelFraction(45315, 65536)), new(FromPixelFraction(25175, 32768)),
        new(FromPixelFraction(27825, 32768)), new(FromPixelFraction(61215, 65536)), new(FromPixelFraction(1509, 65536)), new(FromPixelFraction(17709, 16384)), new(FromPixelFraction(19299, 16384)),
        new(FromPixelFraction(83821, 65536)), new(FromPixelFraction(90711, 65536)), new(FromPixelFraction(48933, 32768)), new(FromPixelFraction(52643, 32768)), new(FromPixelFraction(112971, 65536)),
        new(FromPixelFraction(120921, 65536)), new(FromPixelFraction(8071, 4096)), new(FromPixelFraction(8457, 4096)), new(FromPixelFraction(144057, 65536)), new(FromPixelFraction(153067, 65536)),
        new(FromPixelFraction(81171, 32768)), new(FromPixelFraction(85941, 32768)), new(FromPixelFraction(181687, 65536)), new(FromPixelFraction(191757, 65536)), new(FromPixelFraction(49947, 16384)),
        new(FromPixelFraction(52597, 16384)), new(FromPixelFraction(221253, 65536)), new(FromPixelFraction(232383, 65536)), new(FromPixelFraction(121889, 32768)), new(FromPixelFraction(127719, 32768)),
        new(FromPixelFraction(265059, 65536)), new(FromPixelFraction(277249, 65536)), new(FromPixelFraction(36213, 8192)), new(FromPixelFraction(37803, 8192)), new(FromPixelFraction(315409, 65536)),
        new(FromPixelFraction(263123, 65536)), new(FromPixelFraction(169935, 32768)), new(FromPixelFraction(176825, 32768)), new(FromPixelFraction(367695, 65536)), new(FromPixelFraction(382005, 65536)),
        new(FromPixelFraction(98569, 16384)), new(FromPixelFraction(102279, 16384)), new(FromPixelFraction(424221, 65536)), new(FromPixelFraction(439591, 65536)), new(FromPixelFraction(227613, 32768)),
        new(FromPixelFraction(234411, 32768)), new(FromPixelFraction(484987, 65536)), new(FromPixelFraction(501417, 65536)), new(FromPixelFraction(16191, 2048)), new(FromPixelFraction(16649, 2048)),
        new(FromPixelFraction(549993, 65536)), new(FromPixelFraction(567483, 65536)), new(FromPixelFraction(292619, 32768)), new(FromPixelFraction(300477, 32768)), new(FromPixelFraction(619239, 65536)),
        new(FromPixelFraction(637789, 65536)), new(FromPixelFraction(147767, 16384)), new(FromPixelFraction(168345, 16384)), new(FromPixelFraction(692725, 65536)), new(FromPixelFraction(712335, 65536)),
        new(FromPixelFraction(364953, 32768)), new(FromPixelFraction(375023, 32768)), new(FromPixelFraction(770451, 65536)), new(FromPixelFraction(788817, 65536)), new(FromPixelFraction(101219, 8192)),
        new(FromPixelFraction(103869, 8192)), new(FromPixelFraction(786881, 65536)), new(FromPixelFraction(871843, 65536)), new(FromPixelFraction(446919, 32768)), new(FromPixelFraction(458049, 32768)),
        new(FromPixelFraction(936319, 65536)), new(FromPixelFraction(959109, 65536)), new(FromPixelFraction(245541, 16384)), new(FromPixelFraction(250795, 16384)), new(FromPixelFraction(1026765, 65536)),
        new(FromPixelFraction(985079, 65536)), new(FromPixelFraction(536213, 32768)), new(FromPixelFraction(548403, 32768)), new(FromPixelFraction(1119147, 65536)), new(FromPixelFraction(1144057, 65536)),
    ];

    /// <summary>The number of marks along the curve, its start through its last mark.</summary>
    public static int MarkCount => Speeds.Length;
}
