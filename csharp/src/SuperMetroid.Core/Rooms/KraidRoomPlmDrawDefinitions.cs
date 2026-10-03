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
    /// <summary><c>$84:93EF</c>: start of the following Phantoon draw region.</summary>
    internal const ushort EndExclusive = 0x93ef;

    internal const int DrawCount = 10;

    /// <summary>A horizontal, single-run Kraid draw evaluated without stored words.</summary>
    internal readonly record struct Draw(ushort Pointer)
    {
        internal int BlockCount => CountFor(Pointer);
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
