namespace SuperMetroid.Core.Game;

/// <summary>One physical X/Y sample in an authored Choot falling path.</summary>
internal readonly record struct ChootFallingPathPoint(ushort XOffset, ushort YOffset)
{
    /// <summary>The native <c>$8000</c> X sentinel that ends one path loop.</summary>
    internal bool IsTerminator => XOffset == 0x8000;
}

/// <summary>Compiled physical paths used by Choot after reaching its jump apex.</summary>
internal static class ChootFallingPathDefinitions
{
    private const ushort NormalPointer = 0xd84c;
    private const ushort WidePointer = 0xd976;
    private const ushort VeryWidePointer = 0xdaa0;

    /// <summary>
    /// <c>$A2:DBCA-$A2:DD41</c>: the normal path with ten extra copies of
    /// normal frame 27 at physical frames 28..37 and normal frame 62 at
    /// physical frames 73..82. Physical frame 93 remains the terminator.
    /// All 188 physical words match this expansion in the pinned NTSC J/U
    /// v1.0 ROM; <c>$DD42</c> is the separate loop Y distance. The bounded
    /// mapping is applied by <see cref="CollapseExpandedPlateaus"/>.
    /// Investigation: #625 / #658.
    /// </summary>
    private const ushort SlowPointer = 0xdbca;
    private const ushort VerySlowPointer = 0xdd44;

    private const int PositivePlateauIndex = 28;
    private const int NegativePlateauIndex = 63;
    private const int SlowPlateauFrames = 10;
    private const int VerySlowPlateauFrames = 30;

    /// <summary>
    /// <c>ChootFallingPatternData_0_Normal</c> at <c>$A2:D84C-$A2:D973</c>.
    /// Each adjacent pair is a signed X/Y word; the final X word is the terminator.
    /// </summary>
    /// <remarks>
    /// The pinned NTSC J/U v1.0 ROM matches all 73 motion pairs and the final
    /// <c>$8000,$8000</c> sentinel. Choot consumes one pair per fall frame and
    /// adds its signed offsets to the 16-bit path origin; the following
    /// <c>$D974</c> word is the loop Y distance, outside this path. Retain the
    /// bounded authored trajectory: its unequal plateaus and phase transitions
    /// would require correction data in a lossless arithmetic reconstruction.
    /// Slow and very-slow selectors reuse these pairs with separate repeated
    /// plateau frames. Investigation: #625 / #655.
    /// </remarks>
    private static ReadOnlySpan<ushort> Normal =>
    [
        0x0000, 0x0000, 0x0001, 0x0001, 0x0002, 0x0001, 0x0003, 0x0002,
        0x0004, 0x0002, 0x0005, 0x0002, 0x0006, 0x0003, 0x0007, 0x0003,
        0x0008, 0x0003, 0x0009, 0x0003, 0x000a, 0x0003, 0x000b, 0x0003,
        0x000c, 0x0003, 0x000c, 0x0003, 0x000d, 0x0003, 0x000d, 0x0003,
        0x000d, 0x0003, 0x000e, 0x0003, 0x000e, 0x0003, 0x000e, 0x0003,
        0x000f, 0x0003, 0x000f, 0x0003, 0x0010, 0x0003, 0x0010, 0x0003,
        0x0010, 0x0003, 0x0010, 0x0003, 0x0011, 0x0003, 0x0011, 0x0003,
        0x0010, 0x0005, 0x000f, 0x0006, 0x000e, 0x0008, 0x000c, 0x0009,
        0x000b, 0x000a, 0x000a, 0x000c, 0x0008, 0x000d, 0x0007, 0x000e,
        0x0006, 0x000e, 0x0004, 0x000f, 0x0003, 0x0010, 0x0002, 0x0010,
        0x0001, 0x0011, 0x0000, 0x0011, 0xffff, 0x0011, 0xfffe, 0x0012,
        0xfffc, 0x0012, 0xfffc, 0x0012, 0xfffb, 0x0012, 0xfffa, 0x0012,
        0xfff9, 0x0012, 0xfff8, 0x0012, 0xfff7, 0x0012, 0xfff7, 0x0012,
        0xfff7, 0x0012, 0xfff6, 0x0012, 0xfff6, 0x0012, 0xfff5, 0x0012,
        0xfff5, 0x0012, 0xfff4, 0x0012, 0xfff4, 0x0012, 0xfff4, 0x0012,
        0xfff3, 0x0012, 0xfff3, 0x0012, 0xfff3, 0x0012, 0xfff4, 0x0014,
        0xfff5, 0x0016, 0xfff6, 0x0017, 0xfff7, 0x0019, 0xfff9, 0x001a,
        0xfffa, 0x001b, 0xfffb, 0x001c, 0xfffd, 0x001d, 0xfffe, 0x001e,
        0xffff, 0x001f, 0x8000, 0x8000,
    ];

    /// <summary><c>ChootFallingPatternData_1_Wide</c> at <c>$A2:D976-$A2:DA9D</c>.</summary>
    /// <remarks>
    /// All 73 signed X/Y motion pairs and the <c>$8000,$8000</c> sentinel
    /// match the pinned NTSC J/U v1.0 ROM. Selector 1 consumes one pair per
    /// frame; the following <c>$DA9E</c> word is its loop Y distance.
    /// Retain this bounded authored trajectory: its X steps are not a fixed
    /// scaling of the normal path and its Y samples also differ, so an exact
    /// arithmetic fit would require less readable phase and correction data.
    /// Investigation: #625 / #656.
    /// </remarks>
    private static ReadOnlySpan<ushort> Wide =>
    [
        0x0000, 0x0000, 0x0003, 0x0000, 0x0005, 0x0001, 0x0007, 0x0001,
        0x0009, 0x0002, 0x000b, 0x0002, 0x000d, 0x0002, 0x000e, 0x0002,
        0x0010, 0x0003, 0x0012, 0x0003, 0x0013, 0x0003, 0x0015, 0x0003,
        0x0016, 0x0003, 0x0017, 0x0003, 0x0018, 0x0003, 0x0019, 0x0003,
        0x0019, 0x0003, 0x001a, 0x0003, 0x001b, 0x0003, 0x001c, 0x0003,
        0x001d, 0x0003, 0x001e, 0x0003, 0x001e, 0x0003, 0x001f, 0x0003,
        0x001f, 0x0003, 0x0020, 0x0003, 0x0020, 0x0003, 0x0020, 0x0003,
        0x001e, 0x0004, 0x001c, 0x0006, 0x001a, 0x0007, 0x0017, 0x0008,
        0x0015, 0x000a, 0x0013, 0x000b, 0x0010, 0x000c, 0x000e, 0x000c,
        0x000b, 0x000d, 0x0009, 0x000e, 0x0007, 0x000e, 0x0005, 0x000f,
        0x0002, 0x000f, 0x0000, 0x0010, 0xfffe, 0x0010, 0xfffc, 0x0010,
        0xfffa, 0x0010, 0xfff9, 0x0011, 0xfff7, 0x0011, 0xfff5, 0x0011,
        0xfff4, 0x0011, 0xfff2, 0x0011, 0xfff1, 0x0011, 0xfff0, 0x0011,
        0xfff0, 0x0011, 0xffef, 0x0011, 0xffee, 0x0011, 0xffed, 0x0011,
        0xffec, 0x0011, 0xffeb, 0x0011, 0xffea, 0x0011, 0xffea, 0x0011,
        0xffe9, 0x0011, 0xffe8, 0x0011, 0xffe8, 0x0011, 0xffea, 0x0013,
        0xffec, 0x0014, 0xffee, 0x0016, 0xfff0, 0x0017, 0xfff2, 0x0018,
        0xfff5, 0x0019, 0xfff7, 0x001a, 0xfffa, 0x001b, 0xfffc, 0x001c,
        0xfffe, 0x001d, 0x8000, 0x8000,
    ];

    /// <summary><c>ChootFallingPatternData_2_VeryWide</c> at <c>$A2:DAA0-$A2:DBC7</c>.</summary>
    /// <remarks>
    /// All 73 signed X/Y motion pairs and the <c>$8000,$8000</c> sentinel
    /// match the pinned NTSC J/U v1.0 ROM. Selector 2 consumes one pair per
    /// frame; the following <c>$DBC8</c> word is its loop Y distance.
    /// Retain this bounded authored trajectory: its X and Y transitions do
    /// not follow a single lossless scaling of the normal or wide paths, and
    /// a fitted formula with correction data would obscure these path samples.
    /// Investigation: #625 / #657.
    /// </remarks>
    private static ReadOnlySpan<ushort> VeryWide =>
    [
        0x0000, 0x0000, 0x0003, 0x0001, 0x0006, 0x0001, 0x0009, 0x0002,
        0x000c, 0x0002, 0x000f, 0x0003, 0x0011, 0x0003, 0x0014, 0x0003,
        0x0016, 0x0003, 0x0018, 0x0003, 0x001a, 0x0003, 0x001c, 0x0004,
        0x001e, 0x0004, 0x0020, 0x0004, 0x0022, 0x0004, 0x0023, 0x0004,
        0x0024, 0x0004, 0x0025, 0x0004, 0x0026, 0x0004, 0x0028, 0x0004,
        0x0029, 0x0004, 0x002a, 0x0004, 0x002a, 0x0004, 0x002b, 0x0004,
        0x002c, 0x0003, 0x002d, 0x0003, 0x002d, 0x0003, 0x002d, 0x0003,
        0x002a, 0x0005, 0x0027, 0x0007, 0x0024, 0x0009, 0x0021, 0x000a,
        0x001e, 0x000b, 0x001a, 0x000d, 0x0017, 0x000e, 0x0014, 0x000f,
        0x0010, 0x000f, 0x000d, 0x0010, 0x000a, 0x0011, 0x0006, 0x0011,
        0x0003, 0x0012, 0x0000, 0x0012, 0xfffd, 0x0013, 0xfffb, 0x0013,
        0xfff8, 0x0013, 0xfff6, 0x0013, 0xfff3, 0x0013, 0xfff1, 0x0014,
        0xffef, 0x0014, 0xffed, 0x0014, 0xffeb, 0x0014, 0xffe9, 0x0014,
        0xffe8, 0x0014, 0xffe7, 0x0014, 0xffe5, 0x0014, 0xffe4, 0x0014,
        0xffe3, 0x0014, 0xffe2, 0x0014, 0xffe1, 0x0013, 0xffe0, 0x0013,
        0xffdf, 0x0013, 0xffde, 0x0013, 0xffde, 0x0013, 0xffe1, 0x0015,
        0xffe4, 0x0017, 0xffe7, 0x0018, 0xffea, 0x001a, 0xffee, 0x001b,
        0xfff1, 0x001c, 0xfff4, 0x001d, 0xfff8, 0x001e, 0xfffb, 0x001f,
        0xfffe, 0x0020, 0x8000, 0x8000,
    ];

    /// <summary>Returns the exact physical sample selected by a native path pointer and frame.</summary>
    internal static ChootFallingPathPoint At(ushort pointer, int frameIndex)
    {
        ReadOnlySpan<ushort> path;
        int expandedFrames = 0;
        switch (pointer)
        {
            case NormalPointer:
                path = Normal;
                break;
            case WidePointer:
                path = Wide;
                break;
            case VeryWidePointer:
                path = VeryWide;
                break;
            case SlowPointer:
                path = Normal;
                expandedFrames = SlowPlateauFrames;
                break;
            case VerySlowPointer:
                path = Normal;
                expandedFrames = VerySlowPlateauFrames;
                break;
            default:
                throw new InvalidDataException(
                    $"Choot falling-pattern pointer ${pointer:X4} is not an authored stream.");
        }

        int sourceFrame = CollapseExpandedPlateaus(frameIndex, expandedFrames);
        int wordIndex = sourceFrame * 2;
        if ((uint)(wordIndex + 1) >= (uint)path.Length)
        {
            throw new InvalidDataException(
                $"Choot falling-pattern frame {frameIndex} exceeds stream ${pointer:X4}.");
        }

        return new ChootFallingPathPoint(path[wordIndex], path[wordIndex + 1]);
    }

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
