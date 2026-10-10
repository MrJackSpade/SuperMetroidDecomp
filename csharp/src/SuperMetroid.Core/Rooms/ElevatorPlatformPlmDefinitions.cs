using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>The three bank-$84 elevator-platform draw lists, 24 bytes apart.</summary>
internal enum ElevatorPlatformDraw : ushort
{
    /// <summary>First elevator-platform draw list, $84:AA97.</summary>
    First = 0xaa97,
    /// <summary>Second elevator-platform draw list, $84:AAAF.</summary>
    Second = 0xaaaf,
    /// <summary>Third elevator-platform draw list, $84:AAC7.</summary>
    Third = 0xaac7,
}

/// <summary>
/// The $B70B elevator-platform PLM's complete $84:AFB6 instruction loop and its
/// three tile-layout lists. Timing, list selection, collision words, and offsets
/// are cartridge-defined; the underlying tiles remain the room tileset artwork.
/// </summary>
internal static class ElevatorPlatformPlmDefinitions
{
    /// <summary>First $B70B instruction word, $84:AFB6.</summary>
    internal const ushort InstructionLoop = 0xafb6;
    /// <summary>Byte stride between consecutive draw lists.</summary>
    private const int DrawStride = 24;

    /// <summary>
    /// Three frames at $84:AA97..AADE, each with two upper edge cells and a
    /// four-cell lower row. All cells are solid. Edge art advances one tile per
    /// frame; lower art advances two, mirrored about the platform centre.
    /// Continuations are absolute offsets (3,0), (0,1), then the terminator.
    /// </summary>
    internal readonly record struct Draw(ushort Pointer, int Frame)
    {
        internal int RunCount => Pointer == 0 ? 0 : 3;
        private void CheckRun(int run)
        {
            if ((uint)run >= (uint)RunCount) throw new IndexOutOfRangeException();
        }
        internal int WordCount(int run)
        {
            CheckRun(run);
            return run == 2 ? 4 : 1;
        }
        internal sbyte NextX(int run)
        {
            CheckRun(run);
            return run == 0 ? (sbyte)3 : (sbyte)0;
        }
        internal sbyte NextY(int run)
        {
            CheckRun(run);
            return run == 1 ? (sbyte)1 : (sbyte)0;
        }
        internal ushort WordAt(int run, int cell)
        {
            if ((uint)cell >= (uint)WordCount(run)) throw new IndexOutOfRangeException();
            bool right = run == 1 || run == 2 && cell >= 2;
            int tile = run < 2 ? 0x85 + Frame : 0x88 + Frame * 2 + Math.Min(cell, 3 - cell);
            return (ushort)(0x8000 | (right ? 0x400 : 0) | tile);
        }
    }
    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        // Pointers outside the three lists belong to other draw families.
        bool owned = Enum.IsDefined((ElevatorPlatformDraw)pointer);
        draw = owned ? new(pointer, (pointer - (ushort)ElevatorPlatformDraw.First) / DrawStride) : default;
        return owned;
    }
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> DrawLists
    {
        get
        {
            for (int frame = 0; frame < 3; frame++)
            {
                TryGetDraw((ushort)((ushort)ElevatorPlatformDraw.First + frame * DrawStride), out var list);
                yield return list;
            }
        }
    }

    internal static string VisualId(ushort pointer) =>
        ClosedNativeWords.Decode<ElevatorPlatformDraw>(pointer, "elevator-platform draw list") switch
        {
            ElevatorPlatformDraw.First => "first-frame",
            ElevatorPlatformDraw.Second => "second-frame",
            ElevatorPlatformDraw.Third => "third-frame",
            _ => throw new InvalidOperationException($"Undefined elevator-platform draw list ${pointer:X4}."),
        };

    internal static bool TryGetByVisualId(string? id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        ElevatorPlatformDraw? pointer = id switch
        {
            "first-frame" => ElevatorPlatformDraw.First,
            "second-frame" => ElevatorPlatformDraw.Second,
            "third-frame" => ElevatorPlatformDraw.Third,
            _ => null,
        };
        if (pointer is not { } draw)
        {
            list = default;
            return false;
        }
        return TryGetDraw((ushort)draw, out list);
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
        if (offset == 16) value = (ushort)RoomPlmInstruction.Goto;
        else if (offset == 18) value = InstructionLoop;
        else if ((offset & 3) == 0) value = 4;
        else
        {
            int phase = offset / 4;
            int frame = 2 - Math.Abs(phase - 2);
            value = (ushort)((ushort)ElevatorPlatformDraw.First + frame * DrawStride);
        }
        return true;
    }

    // Temporary artwork DTOs; gameplay draws calculate cells directly.
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
