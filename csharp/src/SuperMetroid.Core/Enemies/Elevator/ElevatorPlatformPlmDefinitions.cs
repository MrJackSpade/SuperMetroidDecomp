using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>
/// The $B70B elevator-platform PLM's complete $84:AFB6 instruction loop and its
/// three tile-layout lists. Timing, list selection, collision words, and offsets
/// are cartridge-defined; the underlying tiles remain the room tileset artwork.
/// </summary>
internal static class ElevatorPlatformPlmDefinitions
{
    /// <summary>Byte stride between consecutive draw lists.</summary>
    internal const int DrawStride = 24;

    /// <summary>
    /// Three frames at $84:AA97..AADE, each with two upper edge cells and a
    /// four-cell lower row. All cells are solid. Edge art advances one tile per
    /// frame; lower art advances two, mirrored about the platform centre.
    /// Continuations are absolute offsets (3,0), (0,1), then the terminator.
    /// </summary>
    internal readonly record struct Draw(ushort Pointer, int Frame)
    {
        /// <summary>Gets the three native strips, or zero for an uninitialized descriptor.</summary>
        internal int RunCount => Pointer == 0 ? 0 : 3;
        /// <summary>Rejects a strip index outside this platform draw.</summary>
        private void CheckRun(int run)
        {
            if ((uint)run >= (uint)RunCount) throw new IndexOutOfRangeException();
        }
        /// <summary>Gets one edge cell or four lower-row cells for a selected strip.</summary>
        internal int WordCount(int run)
        {
            CheckRun(run);
            return run == 2 ? 4 : 1;
        }
        /// <summary>Gets the next strip's absolute horizontal offset from the PLM origin.</summary>
        internal sbyte NextX(int run)
        {
            CheckRun(run);
            return run == 0 ? (sbyte)3 : (sbyte)0;
        }
        /// <summary>Gets the next strip's absolute vertical offset from the PLM origin.</summary>
        internal sbyte NextY(int run)
        {
            CheckRun(run);
            return run == 1 ? (sbyte)1 : (sbyte)0;
        }
        /// <summary>Calculates a solid platform tile word, mirroring right-side artwork where needed.</summary>
        internal ushort WordAt(int run, int cell)
        {
            if ((uint)cell >= (uint)WordCount(run)) throw new IndexOutOfRangeException();
            bool right = run == 1 || run == 2 && cell >= 2;
            int tile = run < 2 ? 0x85 + Frame : 0x88 + Frame * 2 + Math.Min(cell, 3 - cell);
            return (ushort)(0x8000 | (right ? 0x400 : 0) | tile);
        }
    }
    /// <summary>Recognizes one of the three platform frame pointers and derives its frame index.</summary>
    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        // Pointers outside the three lists belong to other draw families.
        bool owned = Enum.IsDefined((ElevatorPlatformDraw)pointer);
        draw = owned ? new(pointer, (pointer - (ushort)ElevatorPlatformDraw.First) / DrawStride) : default;
        return owned;
    }
    /// <summary>Exports temporary draw DTOs for the three platform frames in native order.</summary>
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

    /// <summary>Maps a native platform draw pointer to its stable artwork identifier.</summary>
    internal static string VisualId(ushort pointer) =>
        ClosedNativeWords.Decode<ElevatorPlatformDraw>(pointer, "elevator-platform draw list") switch
        {
            ElevatorPlatformDraw.First => "first-frame",
            ElevatorPlatformDraw.Second => "second-frame",
            ElevatorPlatformDraw.Third => "third-frame",
            _ => throw new InvalidOperationException($"Undefined elevator-platform draw list ${pointer:X4}."),
        };

    /// <summary>Resolves a stable frame identifier to its temporary draw DTO.</summary>
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

    /// <summary>Materializes an artwork DTO; gameplay calculates platform cells directly.</summary>
    internal static bool TryGetDraw(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(pointer, out var draw)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[draw.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[draw.WordCount(run)];
            for (int cell = 0; cell < words.Length; cell++) words[cell] = draw.WordAt(run, cell);
            runs[run] = new((ushort)words.Length, words, new(draw.NextX(run), draw.NextY(run)));
        }
        list = new(pointer, runs);
        return true;
    }
}
