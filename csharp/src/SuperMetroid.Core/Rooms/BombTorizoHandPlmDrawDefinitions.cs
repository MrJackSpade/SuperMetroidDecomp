using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Physical block draws selected by Bomb Torizo's resident hand PLM. These
/// level words and run offsets are cartridge mechanics, not editable art.
/// </summary>
internal static class BombTorizoHandPlmDrawDefinitions
{
    /// <summary>
    /// Intact is four horizontal strips: origin pair, left singleton, upper pair,
    /// lower triple. Cleared splits the origin row into two halves then clears the
    /// lower, top and upper rows of a four-by-four area. Original run order and
    /// origin-relative continuations are preserved; all cleared words are air FF.
    /// </summary>
    internal readonly record struct Draw(BombTorizoHandDraw Pointer)
    {
        /// <summary>Whether this list removes the hand's solid blocks instead of drawing intact stone.</summary>
        internal bool IsCleared => Pointer == BombTorizoHandDraw.Cleared;
        /// <summary>Gets the number of horizontal strips in the selected native draw list.</summary>
        internal int RunCount => IsCleared ? 5 : 4;
        /// <summary>Gets the block count in a selected strip, rejecting an invalid run index.</summary>
        internal int WordCount(int run)
        {
            CheckRun(run);
            return IsCleared ? (run < 2 ? 2 : 4) : run switch { 1 => 1, 3 => 3, _ => 2 };
        }
        /// <summary>Gets a strip's horizontal origin relative to the hand PLM block.</summary>
        internal sbyte OriginX(int run)
        {
            CheckRun(run);
            return IsCleared ? (sbyte)(run == 0 ? 0 : -2) : (sbyte)-(run & 1);
        }
        /// <summary>Gets a strip's vertical origin relative to the hand PLM block.</summary>
        internal sbyte OriginY(int run)
        {
            CheckRun(run);
            return (sbyte)(IsCleared ? run switch { 2 => 1, 3 => -2, 4 => -1, _ => 0 } :
                run switch { 2 => -1, 3 => 1, _ => 0 });
        }
        /// <summary>Gets the next strip's relative X origin or zero at list termination.</summary>
        internal sbyte NextX(int run)
        {
            CheckRun(run);
            return run + 1 == RunCount ? (sbyte)0 : OriginX(run + 1);
        }
        /// <summary>Gets the next strip's relative Y origin or zero at list termination.</summary>
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
        /// <summary>Rejects strip indices outside the selected intact or cleared list.</summary>
        private void CheckRun(int run)
        {
            if ((uint)run >= RunCount) throw new IndexOutOfRangeException();
        }
    }

    /// <summary>Recognizes one of the two native hand draw pointers and returns its calculated descriptor.</summary>
    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        bool owned = Enum.IsDefined((BombTorizoHandDraw)pointer);
        draw = owned ? new((BombTorizoHandDraw)pointer) : default;
        return owned;
    }

    /// <summary>Enumerates temporary draw DTOs for intact and cleared hand artwork in native order.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            TryGet((ushort)BombTorizoHandDraw.Intact, out var intact);
            yield return intact;
            TryGet((ushort)BombTorizoHandDraw.Cleared, out var cleared);
            yield return cleared;
        }
    }

    /// <summary>Expands a calculated hand draw into an import/export DTO; gameplay computes its strips directly.</summary>
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(pointer, out var draw)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[draw.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[draw.WordCount(run)];
            for (int word = 0; word < words.Length; word++) words[word] = draw.WordAt(run, word);
            runs[run] = new((ushort)words.Length, words, new(draw.NextX(run), draw.NextY(run)));
        }
        list = new(pointer, runs);
        return true;
    }

    /// <summary>Gets the stable artwork ID for a supported native hand draw pointer.</summary>
    internal static string VisualId(ushort pointer) =>
        VisualId(ClosedNativeWords.Decode<BombTorizoHandDraw>(pointer, "Bomb Torizo hand draw with a visual ID"));

    /// <summary>Maps an intact or cleared hand draw to its stable artwork ID.</summary>
    internal static string VisualId(BombTorizoHandDraw pointer) => pointer switch
    {
        BombTorizoHandDraw.Intact => "intact",
        BombTorizoHandDraw.Cleared => "cleared",
        _ => throw new InvalidOperationException($"Undefined BombTorizoHandDraw {pointer}."),
    };

    /// <summary>Resolves an ordinal artwork ID to the corresponding temporary hand draw DTO.</summary>
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
