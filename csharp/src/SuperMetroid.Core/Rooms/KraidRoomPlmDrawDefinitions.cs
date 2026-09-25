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

    private static readonly IReadOnlyDictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Lists = Build();

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All =>
        Lists.Values;

    internal static bool TryGet(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList draw) =>
        Lists.TryGetValue(pointer, out draw);

    private static IReadOnlyDictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Build()
    {
        var result = new Dictionary<ushort,
            RoomPlmShotBlockDrawDefinitions.DrawList>();
        Add(CrumbleFirst, [0x8180]);
        Add(CrumbleSecond, [0x8181]);
        Add(CrumbleThird, [0x0182]);
        Add(CeilingBackground1, [0x013c]);
        Add(CeilingBackground2, [0x0131]);
        Add(CeilingBackground3, [0x0130]);
        Add(SpikeFirst, [0x0111]);
        Add(SpikeSecond, [0x0110]);
        Add(ClearCeiling,
            [0x013c, 0x0131, 0x0130, 0x0131, 0x0130,
             0x0131, 0x0130, 0x0131, 0x0130, 0x0131,
             0x0130, 0x0131, 0x0130, 0x0131, 0x0130]);
        Add(ClearSpikes,
            [0x0111, 0x0110, 0x0111, 0x0110, 0x0111,
             0x0110, 0x0111, 0x0110, 0x0111, 0x0110,
             0x0111, 0x0110, 0x0111, 0x0110, 0x0111,
             0x0110, 0x0111, 0x0110, 0x0111, 0x0110,
             0x0111, 0x0110]);
        return result;

        void Add(ushort pointer, ushort[] words)
        {
            ushort directionAndCount = checked((ushort)words.Length);
            var draw = new RoomPlmShotBlockDrawDefinitions.DrawList(pointer,
                new RoomPlmShotBlockDrawDefinitions.Run[]
                {
                    new(directionAndCount, words, 0, 0),
                });
            if (!result.TryAdd(pointer, draw))
                throw new InvalidDataException(
                    $"Duplicate compiled Kraid draw ${pointer:X4}.");
        }
    }
}
