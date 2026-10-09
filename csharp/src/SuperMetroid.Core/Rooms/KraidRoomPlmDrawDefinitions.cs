namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Reachable Kraid ceiling and spike draw lists at $84:9367..93EE. The
/// first list is also used by the Maridia elevatube, but its physical word
/// is the same for either PLM owner.
/// </summary>
internal static class KraidRoomPlmDrawDefinitions
{
    /// <summary><c>$84:9367</c>: first crumble frame, also used by the elevatube.</summary>
    internal const ushort CrumbleFirst = MaridiaElevatubePlmDefinitions.DrawPointer;
    /// <summary><c>$84:936D</c>: second crumble frame.</summary>
    internal const ushort CrumbleSecond = 0x936d;
    /// <summary><c>$84:9373</c>: third crumble frame.</summary>
    internal const ushort CrumbleThird = 0x9373;
    /// <summary><c>$84:9379</c>: ceiling background-one final block.</summary>
    internal const ushort CeilingBackground1 = 0x9379;
    /// <summary><c>$84:937F</c>: ceiling background-two final block.</summary>
    internal const ushort CeilingBackground2 = 0x937f;
    /// <summary><c>$84:9385</c>: ceiling background-three final block.</summary>
    internal const ushort CeilingBackground3 = 0x9385;
    /// <summary><c>$84:9391</c>: spike first-column final block.</summary>
    internal const ushort SpikeFirst = 0x9391;
    /// <summary><c>$84:9397</c>: spike second-column final block.</summary>
    internal const ushort SpikeSecond = 0x9397;
    /// <summary><c>$84:939D</c>: already-defeated ceiling clear, fifteen blocks.</summary>
    internal const ushort ClearCeiling = 0x939d;
    /// <summary><c>$84:93BF</c>: already-defeated spikes clear, twenty-two blocks.</summary>
    internal const ushort ClearSpikes = 0x93bf;

    /// <summary>Number of distinct Kraid room draw lists represented by the compiled catalog.</summary>
    internal const int DrawCount = 10;

    /// <summary>A horizontal, single-run Kraid draw evaluated without stored words.</summary>
    /// <param name="Pointer">Bank-$84 address identifying the native draw list.</param>
    internal readonly record struct Draw(ushort Pointer)
    {
        /// <summary>Gets the number of level blocks emitted by this draw list.</summary>
        internal int BlockCount => CountFor(Pointer);

        /// <summary>Calculates one full room level word at its position in the draw list.</summary>
        /// <param name="index">Zero-based block index within the draw list.</param>
        /// <returns>The level word, including its collision bits.</returns>
        /// <exception cref="IndexOutOfRangeException">The index is outside this draw list's block count.</exception>
        internal ushort WordAt(int index) => LevelWord(Pointer, index);
    }

    /// <summary>Native record order: six six-byte draws, skip unused938B,
    /// two six-byte spike draws, then the fifteen- and twenty-two-block clears.</summary>
    internal static ushort PointerAt(int index)
    {
        if ((uint)index >= DrawCount) throw new IndexOutOfRangeException();
        return index switch
        {
            < 6 => (ushort)(CrumbleFirst + 6 * index),
            < 8 => (ushort)(SpikeFirst + 6 * (index - 6)),
            8 => ClearCeiling,
            _ => ClearSpikes,
        };
    }

    /// <summary>Resolves a native draw-list pointer when it belongs to the compiled Kraid catalog.</summary>
    /// <param name="pointer">Bank-$84 address to look up.</param>
    /// <param name="draw">Receives the calculated draw when the pointer is recognized; otherwise receives the default value.</param>
    /// <returns><see langword="true"/> when the pointer identifies a supported draw list.</returns>
    internal static bool TryGet(ushort pointer, out Draw draw)
    {
        if (CountFor(pointer) != 0)
        {
            draw = new(pointer);
            return true;
        }
        draw = default;
        return false;
    }

    /// <summary>Native horizontal run counts at $84:9367..93BF; zero means no draw.</summary>
    private static int CountFor(ushort pointer) => pointer switch
    {
        CrumbleFirst or CrumbleSecond or CrumbleThird or
        CeilingBackground1 or CeilingBackground2 or CeilingBackground3 or
        SpikeFirst or SpikeSecond => 1,
        ClearCeiling => 15,
        ClearSpikes => 22,
        _ => 0,
    };

    /// <summary>Evaluates $84:9369..93EB level words: solid crumble stages,
    /// their air final stage, and alternating ceiling/spike background columns.</summary>
    /// <remarks>The ceiling clear starts with its unique left edge, then repeats
    /// background two/three. The spike clear repeats first/second columns eleven times.
    /// Full words retain collision nibbles; editable visual overrides are applied later.</remarks>
    private static ushort LevelWord(ushort pointer, int index)
    {
        if ((uint)index >= (uint)CountFor(pointer)) throw new IndexOutOfRangeException();
        return pointer switch
        {
            CrumbleFirst => 0x8180,
            CrumbleSecond => 0x8181,
            CrumbleThird => 0x0182,
            CeilingBackground1 => 0x013c,
            CeilingBackground2 => 0x0131,
            CeilingBackground3 => 0x0130,
            SpikeFirst => 0x0111,
            SpikeSecond => 0x0110,
            ClearCeiling => LevelWord(index == 0 ? CeilingBackground1
                : (index & 1) != 0 ? CeilingBackground2 : CeilingBackground3, 0),
            ClearSpikes => LevelWord((index & 1) == 0 ? SpikeFirst : SpikeSecond, 0),
            _ => throw new InvalidDataException($"Unknown Kraid draw ${pointer:X4}."),
        };
    }

    /// <summary>Materializes temporary native-shaped records for asset import/export and
    /// inspection. Gameplay reads Draw.WordAt directly; no generated draw cache exists.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            for (int index = 0; index < DrawCount; index++)
            {
                var draw = new Draw(PointerAt(index));
                ushort[] words = new ushort[draw.BlockCount];
                for (int block = 0; block < words.Length; block++) words[block] = draw.WordAt(block);
                yield return new(draw.Pointer,
                    new RoomPlmShotBlockDrawDefinitions.Run[] { new((ushort)words.Length, words, 0, 0) });
            }
        }
    }

    /// <summary>Resolves a stable presentation key to its corresponding compiled room draw.</summary>
    /// <param name="id">One of the catalog's crumble, background, or clear visual identifiers.</param>
    /// <param name="draw">Receives the matching draw, or the default value when the identifier is unknown.</param>
    /// <returns><see langword="true"/> when the identifier maps to a Kraid draw list.</returns>
    internal static bool TryGetByVisualId(string id, out Draw draw) =>
        TryGet(id switch
        {
            "crumble-first" => CrumbleFirst,
            "crumble-second" => CrumbleSecond,
            "crumble-third" => CrumbleThird,
            "ceiling-background-one" => CeilingBackground1,
            "ceiling-background-two" => CeilingBackground2,
            "ceiling-background-three" => CeilingBackground3,
            "spike-first" => SpikeFirst,
            "spike-second" => SpikeSecond,
            "clear-ceiling" => ClearCeiling,
            "clear-spikes" => ClearSpikes,
            _ => (ushort)0,
        }, out draw);
    /// <summary>Returns the stable presentation identifier for a compiled Kraid draw pointer.</summary>
    /// <param name="pointer">Bank-$84 address of a supported draw list.</param>
    /// <returns>The semantic key used to identify the list in editable presentation data.</returns>
    /// <exception cref="InvalidDataException">No Kraid visual identifier is defined for the pointer.</exception>
    internal static string VisualId(ushort pointer) => pointer switch
    {
        CrumbleFirst => "crumble-first",
        CrumbleSecond => "crumble-second",
        CrumbleThird => "crumble-third",
        CeilingBackground1 => "ceiling-background-one",
        CeilingBackground2 => "ceiling-background-two",
        CeilingBackground3 => "ceiling-background-three",
        SpikeFirst => "spike-first",
        SpikeSecond => "spike-second",
        ClearCeiling => "clear-ceiling",
        ClearSpikes => "clear-spikes",
        _ => throw new InvalidDataException(
            $"Kraid room draw ${pointer:X4} has no visual ID."),
    };

    /// <summary>Determines whether a room PLM header owns one of the compiled Kraid ceiling or spike lists.</summary>
    /// <param name="header">Room PLM header value to classify.</param>
    /// <returns><see langword="true"/> for a Kraid crumble or clear PLM.</returns>
    internal static bool IsKraidOwner(ushort header) => header is
        RoomPlmHeaders.CrumbleKraidCeilingIntoBackground1 or
        RoomPlmHeaders.CrumbleKraidPlatformVariant1 or
        RoomPlmHeaders.CrumbleKraidCeilingIntoBackground2 or
        RoomPlmHeaders.CrumbleKraidPlatformVariant2 or
        RoomPlmHeaders.CrumbleKraidCeilingIntoBackground3 or
        RoomPlmHeaders.ClearKraidCeiling or
        RoomPlmHeaders.CrumbleKraidSpikes or
        RoomPlmHeaders.ClearKraidSpikes;

}
