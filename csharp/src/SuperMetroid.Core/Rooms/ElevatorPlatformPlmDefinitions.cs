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

    /// <summary>
    /// Three frames at $84:AA97..AADE, each with two upper edge cells and a
    /// four-cell lower row. All cells are solid. Edge art advances one tile per
    /// frame; lower art advances two, mirrored about the platform centre.
    /// Continuations are absolute offsets (3,0), (0,1), then the terminator.
    /// </summary>
    /// <param name="Pointer">Bank-$84 address of one of the three authored frame lists; zero represents an empty draw.</param>
    /// <param name="Frame">Zero-based platform frame number, used to calculate the tile indices for that layout.</param>
    internal readonly record struct Draw(ushort Pointer, int Frame)
    {
        /// <summary>Returns three runs for a recognized frame and no runs for the default empty draw.</summary>
        internal int RunCount => Pointer == 0 ? 0 : 3;

        /// <summary>Rejects run indexes outside the draw's emitted horizontal segments.</summary>
        private void CheckRun(int run)
        {
            if ((uint)run >= (uint)RunCount) throw new IndexOutOfRangeException();
        }
        /// <summary>Returns one tile word for an upper edge or four words for the lower platform row.</summary>
        internal int WordCount(int run)
        {
            CheckRun(run);
            return run == 2 ? 4 : 1;
        }
        /// <summary>Returns the horizontal offset from this run's origin to the next run.</summary>
        internal sbyte NextX(int run)
        {
            CheckRun(run);
            return run == 0 ? (sbyte)3 : (sbyte)0;
        }
        /// <summary>Returns the vertical offset from this run's origin to the next run.</summary>
        internal sbyte NextY(int run)
        {
            CheckRun(run);
            return run == 1 ? (sbyte)1 : (sbyte)0;
        }
        /// <summary>Builds the solid tile word for one cell, mirroring the platform's right half and frame-specific art.</summary>
        internal ushort WordAt(int run, int cell)
        {
            if ((uint)cell >= (uint)WordCount(run)) throw new IndexOutOfRangeException();
            bool right = run == 1 || run == 2 && cell >= 2;
            int tile = run < 2 ? 0x85 + Frame : 0x88 + Frame * 2 + Math.Min(cell, 3 - cell);
            return (ushort)(0x8000 | (right ? 0x400 : 0) | tile);
        }
    }
    /// <summary>Recognizes an aligned authored frame pointer and returns its compact layout descriptor.</summary>
    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        int offset = pointer - FirstDraw;
        bool owned = offset >= 0 && offset <= ThirdDraw - FirstDraw && offset % 24 == 0;
        draw = owned ? new(pointer, offset / 24) : default;
        return owned;
    }
    /// <summary>Projects all three native frames into the common shot-block draw-list representation.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> DrawLists
    {
        get
        {
            for (int frame = 0; frame < 3; frame++)
            {
                TryGetDraw((ushort)(FirstDraw + frame * 24), out var list);
                yield return list;
            }
        }
    }

    /// <summary>Returns the stable artwork asset identifier for one of the three frame-list pointers.</summary>
    internal static string VisualId(ushort pointer) => pointer switch
    {
        FirstDraw => "first-frame",
        SecondDraw => "second-frame",
        ThirdDraw => "third-frame",
        _ => throw new InvalidDataException($"Unknown elevator-platform draw list ${pointer:X4}."),
    };

    /// <summary>Resolves a stable artwork identifier to its native pointer and common draw-list representation.</summary>
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
        return TryGetDraw(pointer, out list);
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

    // Temporary artwork DTOs; gameplay draws calculate cells directly.
    /// <summary>Builds a temporary common artwork list for an authored frame pointer, returning false for unrecognized addresses.</summary>
    internal static bool TryGetDraw(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(pointer, out var draw)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[draw.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[draw.WordCount(run)];
            for (int cell = 0; cell < words.Length; cell++) words[cell] = draw.WordAt(run, cell);
            runs[run] = new((ushort)words.Length, words, draw.NextX(run), draw.NextY(run));
        }
        list = new(pointer, runs);
        return true;
    }
}
