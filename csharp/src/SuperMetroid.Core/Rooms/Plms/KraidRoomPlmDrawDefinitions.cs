namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Reachable Kraid ceiling and spike draw lists at $84:9367..93EE. The
/// first list is also used by the Maridia elevatube, but its physical word
/// is the same for either PLM owner.
/// </summary>
internal static class KraidRoomPlmDrawDefinitions
{
    /// <summary>Number of reachable Kraid ceiling and spike draw lists.</summary>
    internal const int DrawCount = 10;

    /// <summary>A horizontal, single-run Kraid draw evaluated without stored words.</summary>
    internal readonly record struct Draw(KraidRoomDraw Frame)
    {
        /// <summary>Gets the number of blocks in this single horizontal native run.</summary>
        internal int BlockCount => CountFor(Frame);
        /// <summary>Calculates the full collision and tile word for a block in this draw.</summary>
        internal ushort WordAt(int index) =>
            (uint)index < (uint)BlockCount ? LevelWord(Frame, index) : throw new ArgumentOutOfRangeException(nameof(index));
    }

    /// <summary>Recognizes a Kraid room draw and returns its calculated descriptor.</summary>
    internal static bool TryGet(PlmDraw request, out Draw draw)
    {
        bool found = PlmDraw.TryFrame(request, out KraidRoomDraw frame);
        draw = found ? new(frame) : default;
        return found;
    }

    /// <summary>Gets the one, fifteen, or twenty-two block span for a named Kraid draw.</summary>
    private static int CountFor(KraidRoomDraw draw) => draw switch
    {
        KraidRoomDraw.CrumbleFirst or KraidRoomDraw.CrumbleSecond or KraidRoomDraw.CrumbleThird or
        KraidRoomDraw.CeilingBackground1 or KraidRoomDraw.CeilingBackground2 or KraidRoomDraw.CeilingBackground3 or
        KraidRoomDraw.SpikeFirst or KraidRoomDraw.SpikeSecond => 1,
        KraidRoomDraw.ClearCeiling => 15,
        KraidRoomDraw.ClearSpikes => 22,
        _ => throw new InvalidOperationException($"Undefined {nameof(KraidRoomDraw)} {draw}."),
    };

    /// <summary>Calculates a named crumble, background, or alternating clear block word.</summary>
    /// <remarks>The ceiling clear starts with its unique left edge, then repeats
    /// background two/three. The spike clear repeats first/second columns eleven times.
    /// Full words retain collision nibbles; editable visual overrides are applied later.</remarks>
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
        _ => throw new InvalidOperationException($"Undefined {nameof(KraidRoomDraw)} {draw}."),
    };

    /// <summary>Materializes temporary native-shaped records for asset import/export and
    /// inspection. Gameplay reads Draw.WordAt directly; no generated draw cache exists.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (KraidRoomDraw frame in Enum.GetValues<KraidRoomDraw>())
            {
                var draw = new Draw(frame);
                ushort[] words = new ushort[draw.BlockCount];
                for (int block = 0; block < words.Length; block++) words[block] = draw.WordAt(block);
                yield return new(PlmDraw.Of(frame),
                    new RoomPlmShotBlockDrawDefinitions.Run[] { new(PlmRunDirection.Horizontal, words, new(0, 0)) });
            }
        }
    }

    /// <summary>Resolves an ordinal artwork identifier to its calculated Kraid draw.</summary>
    internal static bool TryGetByVisualId(string id, out Draw draw)
    {
        foreach (KraidRoomDraw candidate in Enum.GetValues<KraidRoomDraw>())
            if (string.Equals(id, VisualId(candidate), StringComparison.Ordinal))
                return TryGet(PlmDraw.Of(candidate), out draw);
        draw = default;
        return false;
    }
    /// <summary>Gets the stable artwork identifier of a Kraid room draw.</summary>
    internal static string VisualId(PlmDraw request) =>
        VisualId(PlmDraw.FrameOf<KraidRoomDraw>(request, "Kraid room draw with a visual ID"));
    /// <summary>Maps a named ceiling or spike draw to its stable artwork identifier.</summary>
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
        _ => throw new InvalidOperationException($"Undefined {nameof(KraidRoomDraw)} {draw}."),
    };

}
