namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The $B70B elevator-platform PLM's complete $84:AFB6 instruction loop and its
/// three tile-layout lists. Timing, list selection, collision words, and offsets
/// are cartridge-defined; the underlying tiles remain the room tileset artwork.
/// </summary>
internal static class ElevatorPlatformPlmDefinitions
{
    /// <summary>First $B70B instruction word, $84:AFB6.</summary>
    internal const ushort InstructionLoop = 0xafb6;
    /// <summary>First elevator-platform draw list, $84:AA97.</summary>
    internal const ushort FirstDraw = 0xaa97;
    /// <summary>Second elevator-platform draw list, $84:AAAF.</summary>
    internal const ushort SecondDraw = 0xaaaf;
    /// <summary>Third elevator-platform draw list, $84:AAC7.</summary>
    internal const ushort ThirdDraw = 0xaac7;

    private static readonly (ushort Address, ushort Value)[] Program =
    [
        (0xafb6, 4), (0xafb8, FirstDraw),
        (0xafba, 4), (0xafbc, SecondDraw),
        (0xafbe, 4), (0xafc0, ThirdDraw),
        (0xafc2, 4), (0xafc4, SecondDraw),
        (0xafc6, RoomPlmInstructionCodes.Goto), (0xafc8, InstructionLoop),
    ];

    private static readonly IReadOnlyDictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Lists =
        new Dictionary<ushort, RoomPlmShotBlockDrawDefinitions.DrawList>
        {
            [FirstDraw] = CreateDraw(FirstDraw, 0x8085, 0x8485,
                0x8088, 0x8089, 0x8489, 0x8488),
            [SecondDraw] = CreateDraw(SecondDraw, 0x8086, 0x8486,
                0x808a, 0x808b, 0x848b, 0x848a),
            [ThirdDraw] = CreateDraw(ThirdDraw, 0x8087, 0x8487,
                0x808c, 0x808d, 0x848d, 0x848c),
        };

    internal static ReadOnlySpan<(ushort Address, ushort Value)> ProgramWords => Program;
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> DrawLists => Lists.Values;

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        foreach ((ushort candidate, ushort word) in Program)
        {
            if (candidate == address)
            {
                value = word;
                return true;
            }
        }
        value = 0;
        return false;
    }

    internal static bool TryGetDraw(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        Lists.TryGetValue(pointer, out list);

    private static RoomPlmShotBlockDrawDefinitions.DrawList CreateDraw(
        ushort pointer, ushort upperLeft, ushort lowerLeft,
        ushort centerTopLeft, ushort centerTopRight,
        ushort centerBottomRight, ushort centerBottomLeft) =>
        new(pointer,
        new RoomPlmShotBlockDrawDefinitions.Run[]
        {
            new(1, new ushort[] { upperLeft }, 3, 0),
            new(1, new ushort[] { lowerLeft }, 0, 1),
            new(4, new ushort[] { centerTopLeft, centerTopRight,
                centerBottomRight, centerBottomLeft }, 0, 0),
        });
}
