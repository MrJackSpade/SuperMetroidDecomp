namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>Compiled physical paths used by Choot after reaching its jump apex.</summary>
internal static class ChootFallingPathDefinitions
{
    /// <summary>First physical frame after the normal path's positive-X plateau sample.</summary>
    private const int PositivePlateauIndex = 28;
    /// <summary>First physical frame after the normal path's negative-X plateau sample.</summary>
    private const int NegativePlateauIndex = 63;
    /// <summary>Additional copies of each plateau sample in the slow native path.</summary>
    private const int SlowPlateauFrames = 10;
    /// <summary>Additional copies of each plateau sample in the very-slow native path.</summary>
    private const int VerySlowPlateauFrames = 30;

    /// <summary>The <c>$8000,$8000</c> sample that ends a path loop.</summary>
    private static readonly Offset16<RoomPixels> End = new(short.MinValue, short.MinValue);

    /// <summary>
    /// <c>ChootFallingPatternData_0_Normal</c> at <c>$A2:D84C-$A2:D973</c>.
    /// Each sample is a signed X/Y offset; the final <see cref="End"/> sample is the terminator.
    /// </summary>
    /// <remarks>
    /// The pinned NTSC J/U v1.0 ROM matches all 73 motion pairs and the final
    /// <c>$8000,$8000</c> sentinel. Choot consumes one pair per fall frame and
    /// adds its signed offsets to the 16-bit path origin; the following
    /// <c>$D974</c> word is the loop Y distance, outside this path.
    /// </remarks>
    private static readonly Offset16<RoomPixels>[] Normal =
    [
        new(0, 0), new(1, 1), new(2, 1), new(3, 2), new(4, 2), new(5, 2), new(6, 3), new(7, 3),
        new(8, 3), new(9, 3), new(10, 3), new(11, 3), new(12, 3), new(12, 3), new(13, 3), new(13, 3),
        new(13, 3), new(14, 3), new(14, 3), new(14, 3), new(15, 3), new(15, 3), new(16, 3), new(16, 3),
        new(16, 3), new(16, 3), new(17, 3), new(17, 3), new(16, 5), new(15, 6), new(14, 8), new(12, 9),
        new(11, 10), new(10, 12), new(8, 13), new(7, 14), new(6, 14), new(4, 15), new(3, 16), new(2, 16),
        new(1, 17), new(0, 17), new(-1, 17), new(-2, 18), new(-4, 18), new(-4, 18), new(-5, 18), new(-6, 18),
        new(-7, 18), new(-8, 18), new(-9, 18), new(-9, 18), new(-9, 18), new(-10, 18), new(-10, 18), new(-11, 18),
        new(-11, 18), new(-12, 18), new(-12, 18), new(-12, 18), new(-13, 18), new(-13, 18), new(-13, 18), new(-12, 20),
        new(-11, 22), new(-10, 23), new(-9, 25), new(-7, 26), new(-6, 27), new(-5, 28), new(-3, 29), new(-2, 30),
        new(-1, 31),
        End,
    ];

    /// <summary><c>ChootFallingPatternData_1_Wide</c> at <c>$A2:D976-$A2:DA9D</c>.</summary>
    /// <remarks>
    /// All 73 signed X/Y motion pairs and the <c>$8000,$8000</c> sentinel
    /// match the pinned NTSC J/U v1.0 ROM. Selector 1 consumes one pair per
    /// frame; the following <c>$DA9E</c> word is its loop Y distance.
    /// </remarks>
    private static readonly Offset16<RoomPixels>[] Wide =
    [
        new(0, 0), new(3, 0), new(5, 1), new(7, 1), new(9, 2), new(11, 2), new(13, 2), new(14, 2),
        new(16, 3), new(18, 3), new(19, 3), new(21, 3), new(22, 3), new(23, 3), new(24, 3), new(25, 3),
        new(25, 3), new(26, 3), new(27, 3), new(28, 3), new(29, 3), new(30, 3), new(30, 3), new(31, 3),
        new(31, 3), new(32, 3), new(32, 3), new(32, 3), new(30, 4), new(28, 6), new(26, 7), new(23, 8),
        new(21, 10), new(19, 11), new(16, 12), new(14, 12), new(11, 13), new(9, 14), new(7, 14), new(5, 15),
        new(2, 15), new(0, 16), new(-2, 16), new(-4, 16), new(-6, 16), new(-7, 17), new(-9, 17), new(-11, 17),
        new(-12, 17), new(-14, 17), new(-15, 17), new(-16, 17), new(-16, 17), new(-17, 17), new(-18, 17), new(-19, 17),
        new(-20, 17), new(-21, 17), new(-22, 17), new(-22, 17), new(-23, 17), new(-24, 17), new(-24, 17), new(-22, 19),
        new(-20, 20), new(-18, 22), new(-16, 23), new(-14, 24), new(-11, 25), new(-9, 26), new(-6, 27), new(-4, 28),
        new(-2, 29),
        End,
    ];

    /// <summary><c>ChootFallingPatternData_2_VeryWide</c> at <c>$A2:DAA0-$A2:DBC7</c>.</summary>
    /// <remarks>
    /// All 73 signed X/Y motion pairs and the <c>$8000,$8000</c> sentinel
    /// match the pinned NTSC J/U v1.0 ROM. Selector 2 consumes one pair per
    /// frame; the following <c>$DBC8</c> word is its loop Y distance.
    /// </remarks>
    private static readonly Offset16<RoomPixels>[] VeryWide =
    [
        new(0, 0), new(3, 1), new(6, 1), new(9, 2), new(12, 2), new(15, 3), new(17, 3), new(20, 3),
        new(22, 3), new(24, 3), new(26, 3), new(28, 4), new(30, 4), new(32, 4), new(34, 4), new(35, 4),
        new(36, 4), new(37, 4), new(38, 4), new(40, 4), new(41, 4), new(42, 4), new(42, 4), new(43, 4),
        new(44, 3), new(45, 3), new(45, 3), new(45, 3), new(42, 5), new(39, 7), new(36, 9), new(33, 10),
        new(30, 11), new(26, 13), new(23, 14), new(20, 15), new(16, 15), new(13, 16), new(10, 17), new(6, 17),
        new(3, 18), new(0, 18), new(-3, 19), new(-5, 19), new(-8, 19), new(-10, 19), new(-13, 19), new(-15, 20),
        new(-17, 20), new(-19, 20), new(-21, 20), new(-23, 20), new(-24, 20), new(-25, 20), new(-27, 20), new(-28, 20),
        new(-29, 20), new(-30, 20), new(-31, 19), new(-32, 19), new(-33, 19), new(-34, 19), new(-34, 19), new(-31, 21),
        new(-28, 23), new(-25, 24), new(-22, 26), new(-18, 27), new(-15, 28), new(-12, 29), new(-8, 30), new(-5, 31),
        new(-2, 32),
        End,
    ];

    /// <summary>Returns the exact physical sample of one falling path at one frame.</summary>
    internal static ChootFallingPathPoint At(ChootFallingPath fallingPath, int frameIndex)
    {
        Offset16<RoomPixels>[] path;
        int expandedFrames = 0;
        switch (fallingPath)
        {
            case ChootFallingPath.Normal:
                path = Normal;
                break;
            case ChootFallingPath.Wide:
                path = Wide;
                break;
            case ChootFallingPath.VeryWide:
                path = VeryWide;
                break;
            case ChootFallingPath.Slow:
                path = Normal;
                expandedFrames = SlowPlateauFrames;
                break;
            case ChootFallingPath.VerySlow:
                path = Normal;
                expandedFrames = VerySlowPlateauFrames;
                break;
            default:
                throw new InvalidOperationException($"Undefined ChootFallingPath {fallingPath}.");
        }

        int sourceFrame = CollapseExpandedPlateaus(frameIndex, expandedFrames);
        if ((uint)sourceFrame >= (uint)path.Length)
        {
            throw new InvalidDataException(
                $"Choot falling-pattern frame {frameIndex} exceeds stream ${(int)fallingPath:X4}.");
        }

        return new ChootFallingPathPoint(path[sourceFrame]);
    }

    /// <summary>Maps a slow-path physical frame back to the normal stream, repeating the two authored plateau samples.</summary>
    private static int CollapseExpandedPlateaus(int frameIndex, int expandedFrames)
    {
        if (frameIndex < 0)
            return frameIndex;
        if (frameIndex < PositivePlateauIndex)
            return frameIndex;
        if (frameIndex < PositivePlateauIndex + expandedFrames)
            return PositivePlateauIndex - 1;

        frameIndex -= expandedFrames;
        if (frameIndex < NegativePlateauIndex)
            return frameIndex;
        if (frameIndex < NegativePlateauIndex + expandedFrames)
            return NegativePlateauIndex - 1;
        return frameIndex - expandedFrames;
    }
}
