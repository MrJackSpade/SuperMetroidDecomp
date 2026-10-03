namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Physical block draws selected by Bomb Torizo's resident hand PLM. These
/// level words and run offsets are cartridge mechanics, not editable art.
/// </summary>
internal static class BombTorizoHandPlmDrawDefinitions
{
    /// <summary>Intact Chozo hand and surrounding stone at $84:9877.</summary>
    internal const ushort Intact = 0x9877;
    /// <summary>Final five-run cleared-hand area at $84:989D.</summary>
    internal const ushort Cleared = 0x989d;

    /// <summary>
    /// Intact is four horizontal strips: origin pair, left singleton, upper pair,
    /// lower triple. Cleared splits the origin row into two halves then clears the
    /// lower, top and upper rows of a four-by-four area. Original run order and
    /// origin-relative continuations are preserved; all cleared words are air FF.
    /// </summary>
    internal readonly record struct Draw(ushort Pointer)
    {
        internal bool IsCleared => Pointer == Cleared;
        internal int RunCount => IsCleared ? 5 : 4;
        internal int WordCount(int run)
        {
            CheckRun(run);
            return IsCleared ? (run < 2 ? 2 : 4) : run switch { 1 => 1, 3 => 3, _ => 2 };
        }
        internal sbyte OriginX(int run)
        {
            CheckRun(run);
            return IsCleared ? (sbyte)(run == 0 ? 0 : -2) : (sbyte)-(run & 1);
        }
        internal sbyte OriginY(int run)
        {
            CheckRun(run);
            return (sbyte)(IsCleared ? run switch { 2 => 1, 3 => -2, 4 => -1, _ => 0 } :
                run switch { 2 => -1, 3 => 1, _ => 0 });
        }
        internal sbyte NextX(int run)
        {
            CheckRun(run);
            return run + 1 == RunCount ? (sbyte)0 : OriginX(run + 1);
        }
        internal sbyte NextY(int run)
        {
            CheckRun(run);
            return run + 1 == RunCount ? (sbyte)0 : OriginY(run + 1);
        }
        /// <summary>
        /// $84:9877 intact strip tiles are consecutive within each spatial role:
        /// origin 65..66, left 64, upper 45..46, lower 47..49, all solid type 8.
        /// $84:989D clears sixteen cells to air with blank tile FF.
        /// </summary>
        internal ushort WordAt(int run, int word)
        {
            if ((uint)word >= WordCount(run)) throw new IndexOutOfRangeException();
            if (IsCleared) return 0xff;
            int firstTile = run switch { 0 => 0x65, 1 => 0x64, 2 => 0x45, _ => 0x47 };
            return (ushort)(0x8000 | (firstTile + word));
        }
        private void CheckRun(int run)
        {
            if ((uint)run >= RunCount) throw new IndexOutOfRangeException();
        }
    }

    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        bool owned = pointer is Intact or Cleared;
        draw = owned ? new(pointer) : default;
        return owned;
    }

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            TryGet(Intact, out var intact);
            yield return intact;
            TryGet(Cleared, out var cleared);
            yield return cleared;
        }
    }

    // Temporary DTOs serve artwork import/export; gameplay computes each strip directly.
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(pointer, out var draw)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[draw.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[draw.WordCount(run)];
            for (int word = 0; word < words.Length; word++) words[word] = draw.WordAt(run, word);
            runs[run] = new((ushort)words.Length, words, draw.NextX(run), draw.NextY(run));
        }
        list = new(pointer, runs);
        return true;
    }

    internal static string VisualId(ushort pointer) => pointer switch
    {
        Intact => "intact",
        Cleared => "cleared",
        _ => throw new InvalidDataException(
            $"Bomb Torizo hand draw ${pointer:X4} has no visual ID."),
    };

    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList candidate in All)
        {
            if (string.Equals(id, VisualId(candidate.Pointer), StringComparison.Ordinal))
            {
                list = candidate;
                return true;
            }
        }
        list = default;
        return false;
    }

}
