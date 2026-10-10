using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>The ten reachable bank-$84 Kraid ceiling and spike draw lists.</summary>
internal enum KraidRoomDraw : ushort
{
    /// <summary><c>$84:9367</c>: first crumble frame, also used by the elevatube.</summary>
    CrumbleFirst = MaridiaElevatubePlmDefinitions.DrawPointer,
    /// <summary><c>$84:936D</c>: second crumble frame.</summary>
    CrumbleSecond = 0x936d,
    /// <summary><c>$84:9373</c>: third crumble frame.</summary>
    CrumbleThird = 0x9373,
    /// <summary><c>$84:9379</c>: ceiling background-one final block.</summary>
    CeilingBackground1 = 0x9379,
    /// <summary><c>$84:937F</c>: ceiling background-two final block.</summary>
    CeilingBackground2 = 0x937f,
    /// <summary><c>$84:9385</c>: ceiling background-three final block.</summary>
    CeilingBackground3 = 0x9385,
    /// <summary><c>$84:9391</c>: spike first-column final block.</summary>
    SpikeFirst = 0x9391,
    /// <summary><c>$84:9397</c>: spike second-column final block.</summary>
    SpikeSecond = 0x9397,
    /// <summary><c>$84:939D</c>: already-defeated ceiling clear, fifteen blocks.</summary>
    ClearCeiling = 0x939d,
    /// <summary><c>$84:93BF</c>: already-defeated spikes clear, twenty-two blocks.</summary>
    ClearSpikes = 0x93bf,
}

/// <summary>
/// Reachable Kraid ceiling and spike draw lists at $84:9367..93EE. The
/// first list is also used by the Maridia elevatube, but its physical word
/// is the same for either PLM owner.
/// </summary>
internal static class KraidRoomPlmDrawDefinitions
{
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
            < 6 => (ushort)((ushort)KraidRoomDraw.CrumbleFirst + 6 * index),
            < 8 => (ushort)((ushort)KraidRoomDraw.SpikeFirst + 6 * (index - 6)),
            8 => (ushort)KraidRoomDraw.ClearCeiling,
            _ => (ushort)KraidRoomDraw.ClearSpikes,
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
    private static int CountFor(ushort pointer) =>
        Enum.IsDefined((KraidRoomDraw)pointer) ? CountFor((KraidRoomDraw)pointer) : 0;

    private static int CountFor(KraidRoomDraw draw) => draw switch
    {
        KraidRoomDraw.CrumbleFirst or KraidRoomDraw.CrumbleSecond or KraidRoomDraw.CrumbleThird or
        KraidRoomDraw.CeilingBackground1 or KraidRoomDraw.CeilingBackground2 or KraidRoomDraw.CeilingBackground3 or
        KraidRoomDraw.SpikeFirst or KraidRoomDraw.SpikeSecond => 1,
        KraidRoomDraw.ClearCeiling => 15,
        KraidRoomDraw.ClearSpikes => 22,
        _ => throw new InvalidOperationException($"Undefined {nameof(KraidRoomDraw)} {(int)draw:X4}."),
    };

    /// <summary>Evaluates $84:9369..93EB level words: solid crumble stages,
    /// their air final stage, and alternating ceiling/spike background columns.</summary>
    /// <remarks>The ceiling clear starts with its unique left edge, then repeats
    /// background two/three. The spike clear repeats first/second columns eleven times.
    /// Full words retain collision nibbles; editable visual overrides are applied later.</remarks>
    private static ushort LevelWord(ushort pointer, int index)
    {
        if ((uint)index >= (uint)CountFor(pointer)) throw new IndexOutOfRangeException();
        return LevelWord(ClosedNativeWords.Decode<KraidRoomDraw>(pointer, "Kraid room draw"), index);
    }

    private static ushort LevelWord(KraidRoomDraw draw, int index) => draw switch
    {
        KraidRoomDraw.CrumbleFirst => 0x8180,
        KraidRoomDraw.CrumbleSecond => 0x8181,
        KraidRoomDraw.CrumbleThird => 0x0182,
        KraidRoomDraw.CeilingBackground1 => 0x013c,
        KraidRoomDraw.CeilingBackground2 => 0x0131,
        KraidRoomDraw.CeilingBackground3 => 0x0130,
        KraidRoomDraw.SpikeFirst => 0x0111,
        KraidRoomDraw.SpikeSecond => 0x0110,
        KraidRoomDraw.ClearCeiling => LevelWord(index == 0 ? KraidRoomDraw.CeilingBackground1
            : (index & 1) != 0 ? KraidRoomDraw.CeilingBackground2 : KraidRoomDraw.CeilingBackground3, 0),
        KraidRoomDraw.ClearSpikes => LevelWord((index & 1) == 0 ? KraidRoomDraw.SpikeFirst : KraidRoomDraw.SpikeSecond, 0),
        _ => throw new InvalidOperationException($"Undefined {nameof(KraidRoomDraw)} {(int)draw:X4}."),
    };

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

    internal static bool TryGetByVisualId(string id, out Draw draw)
    {
        foreach (KraidRoomDraw candidate in Enum.GetValues<KraidRoomDraw>())
            if (string.Equals(id, VisualId(candidate), StringComparison.Ordinal))
                return TryGet((ushort)candidate, out draw);
        draw = default;
        return false;
    }
    internal static string VisualId(ushort pointer) =>
        VisualId(ClosedNativeWords.Decode<KraidRoomDraw>(pointer, "Kraid room draw with a visual ID"));
    private static string VisualId(KraidRoomDraw draw) => draw switch
    {
        KraidRoomDraw.CrumbleFirst => "crumble-first",
        KraidRoomDraw.CrumbleSecond => "crumble-second",
        KraidRoomDraw.CrumbleThird => "crumble-third",
        KraidRoomDraw.CeilingBackground1 => "ceiling-background-one",
        KraidRoomDraw.CeilingBackground2 => "ceiling-background-two",
        KraidRoomDraw.CeilingBackground3 => "ceiling-background-three",
        KraidRoomDraw.SpikeFirst => "spike-first",
        KraidRoomDraw.SpikeSecond => "spike-second",
        KraidRoomDraw.ClearCeiling => "clear-ceiling",
        KraidRoomDraw.ClearSpikes => "clear-spikes",
        _ => throw new InvalidOperationException($"Undefined {nameof(KraidRoomDraw)} {(int)draw:X4}."),
    };

    internal static bool IsKraidOwner(PlmHeaderId header) => header is
        PlmHeaderId.CrumbleKraidCeilingIntoBackground1 or
        PlmHeaderId.CrumbleKraidPlatformVariant1 or
        PlmHeaderId.CrumbleKraidCeilingIntoBackground2 or
        PlmHeaderId.CrumbleKraidPlatformVariant2 or
        PlmHeaderId.CrumbleKraidCeilingIntoBackground3 or
        PlmHeaderId.ClearKraidCeiling or
        PlmHeaderId.CrumbleKraidSpikes or
        PlmHeaderId.ClearKraidSpikes;

}
