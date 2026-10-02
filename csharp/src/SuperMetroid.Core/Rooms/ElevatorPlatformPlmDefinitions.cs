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

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> DrawLists => Lists.Values;

    internal static string VisualId(ushort pointer) => pointer switch
    {
        FirstDraw => "first-frame",
        SecondDraw => "second-frame",
        ThirdDraw => "third-frame",
        _ => throw new InvalidDataException($"Unknown elevator-platform draw list ${pointer:X4}."),
    };

    internal static bool TryGetByVisualId(string? id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        ushort pointer = id switch
        {
            "first-frame" => FirstDraw,
            "second-frame" => SecondDraw,
            "third-frame" => ThirdDraw,
            _ => 0,
        };
        return Lists.TryGetValue(pointer, out list);
    }

    /// <summary>
    /// $84:AFB6..AFC8, ten aligned words: four holds of four ticks with a
    /// first/second/third/second ping-pong selection, then Goto the loop start.
    /// Only original word starts are owned; odd and adjacent reads are rejected.
    /// </summary>
    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        int offset = address - InstructionLoop;
        if (offset < 0 || offset > 18 || (offset & 1) != 0)
        {
            value = 0;
            return false;
        }
        if (offset == 16) value = RoomPlmInstructionCodes.Goto;
        else if (offset == 18) value = InstructionLoop;
        else if ((offset & 3) == 0) value = 4;
        else
        {
            int phase = offset / 4;
            int frame = 2 - Math.Abs(phase - 2);
            value = (ushort)(FirstDraw + frame * 24);
        }
        return true;
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
