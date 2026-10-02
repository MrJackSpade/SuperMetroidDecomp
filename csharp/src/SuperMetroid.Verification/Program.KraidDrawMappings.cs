using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static (ushort Pointer, int Count, string Id)[] KraidDrawOracle() =>
    [
        (0x9367, 1, "crumble-first"), (0x936d, 1, "crumble-second"),
        (0x9373, 1, "crumble-third"), (0x9379, 1, "ceiling-background-one"),
        (0x937f, 1, "ceiling-background-two"), (0x9385, 1, "ceiling-background-three"),
        (0x9391, 1, "spike-first"), (0x9397, 1, "spike-second"),
        (0x939d, 15, "clear-ceiling"), (0x93bf, 22, "clear-spikes"),
    ];

    private static void VerifyKraidDrawAddresses()
    {
        var expected = KraidDrawOracle();
        AssertEqual(expected.Length, KraidRoomPlmDrawDefinitions.DrawCount, "Kraid draw count");
        AssertTrue(expected.Select(draw => draw.Pointer).SequenceEqual(
            KraidRoomPlmDrawDefinitions.All.Select(draw => draw.Pointer)), "Kraid export draw order");
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index].Pointer, KraidRoomPlmDrawDefinitions.PointerAt(index),
                "Kraid original draw record position");
        var pointers = expected.Select(draw => draw.Pointer).ToHashSet();
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
        {
            bool exists = pointers.Contains((ushort)pointer);
            AssertEqual(exists, KraidRoomPlmDrawDefinitions.TryGet((ushort)pointer, out var draw),
                "Kraid full draw-pointer domain");
            AssertEqual(exists ? (ushort)pointer : (ushort)0, draw.Pointer,
                "Kraid draw pointer or default on miss");
        }
        foreach (int index in new[] { int.MinValue, -1, 10, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => KraidRoomPlmDrawDefinitions.PointerAt(index),
                "Kraid draw ordinal bounds");
    }

    private static void VerifyKraidDrawShapes(SuperMetroidAddressSpace rom)
    {
        var exported = KraidRoomPlmDrawDefinitions.All.ToDictionary(draw => draw.Pointer);
        foreach (var expected in KraidDrawOracle())
        {
            AssertTrue(KraidRoomPlmDrawDefinitions.TryGet(expected.Pointer, out var draw), "Kraid draw exists");
            ushort directionCount = KraidDrawRomWord(rom, expected.Pointer);
            AssertEqual((ushort)expected.Count, directionCount, "Kraid native horizontal run count");
            AssertEqual(expected.Count, draw.BlockCount, "Kraid calculated block count");
            AssertEqual(1, exported[expected.Pointer].Runs.Length, "Kraid export has one run");
            var run = exported[expected.Pointer].Runs.Span[0];
            AssertEqual(directionCount, run.DirectionAndCount, "Kraid export direction/count");
            AssertEqual(expected.Count, run.LevelWords.Length, "Kraid export word extent");
            ushort tail = KraidDrawRomWord(rom, expected.Pointer + 2 + expected.Count * 2);
            AssertEqual((ushort)0, tail, "Kraid native terminator has zero displacement");
            AssertEqual((sbyte)0, run.NextX, "Kraid export terminal X");
            AssertEqual((sbyte)0, run.NextY, "Kraid export terminal Y");
        }
    }

    private static void VerifyKraidDrawWords(SuperMetroidAddressSpace rom)
    {
        VerifyKraidSingleDrawWords(rom);
        VerifyKraidCeilingClearWords(rom);
        VerifyKraidSpikeClearWords(rom);
    }

    private static void VerifyKraidSingleDrawWords(SuperMetroidAddressSpace rom) =>
        VerifyKraidDrawWordSet(rom, KraidDrawOracle().Take(8));

    private static void VerifyKraidCeilingClearWords(SuperMetroidAddressSpace rom) =>
        VerifyKraidDrawWordSet(rom, KraidDrawOracle().Where(draw => draw.Pointer == 0x939d));

    private static void VerifyKraidSpikeClearWords(SuperMetroidAddressSpace rom) =>
        VerifyKraidDrawWordSet(rom, KraidDrawOracle().Where(draw => draw.Pointer == 0x93bf));

    private static void VerifyKraidDrawWordSet(SuperMetroidAddressSpace rom,
        IEnumerable<(ushort Pointer, int Count, string Id)> definitions)
    {
        var exported = KraidRoomPlmDrawDefinitions.All.ToDictionary(draw => draw.Pointer);
        foreach (var expected in definitions)
        {
            AssertTrue(KraidRoomPlmDrawDefinitions.TryGet(expected.Pointer, out var draw), "Kraid word owner exists");
            for (int block = 0; block < expected.Count; block++)
            {
                ushort word = KraidDrawRomWord(rom, expected.Pointer + 2 + block * 2);
                AssertEqual(word, draw.WordAt(block), "Kraid calculated original physical/visual word");
                AssertEqual(word, exported[expected.Pointer].Runs.Span[0].LevelWords.Span[block],
                    "Kraid materialized export word");
            }
            foreach (int block in new[] { int.MinValue, -1, expected.Count, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => draw.WordAt(block), "Kraid computed word bounds");
        }
    }

    private static void VerifyKraidDrawVisualIds()
    {
        var expected = KraidDrawOracle();
        foreach (var entry in expected)
        {
            AssertEqual(entry.Id, KraidRoomPlmDrawDefinitions.VisualId(entry.Pointer), "Kraid published visual ID");
            AssertTrue(KraidRoomPlmDrawDefinitions.TryGetByVisualId(entry.Id, out var draw), "Kraid reverse visual ID");
            AssertEqual(entry.Pointer, draw.Pointer, "Kraid reverse ID pointer");
            foreach (string invalid in new[] { entry.Id.ToUpperInvariant(), entry.Id + " ", " " + entry.Id })
            {
                AssertTrue(!KraidRoomPlmDrawDefinitions.TryGetByVisualId(invalid, out var missing),
                    "Kraid IDs preserve ordinal exact matching");
                AssertEqual((ushort)0, missing.Pointer, "Kraid missing ID clears result");
            }
        }
        foreach (string invalid in new[] { "", "unknown", null! })
            AssertTrue(!KraidRoomPlmDrawDefinitions.TryGetByVisualId(invalid, out _), "Kraid invalid visual ID");
        var pointers = expected.Select(draw => draw.Pointer).ToHashSet();
        for (int pointer = 0x9365; pointer <= 0x93ef; pointer++)
            if (!pointers.Contains((ushort)pointer))
                AssertThrows<InvalidDataException>(() => KraidRoomPlmDrawDefinitions.VisualId((ushort)pointer),
                    "Kraid unknown pointer has no visual ID");
    }

    private static ushort KraidDrawRomWord(SuperMetroidAddressSpace rom, int address) =>
        (ushort)(rom.ReadByte(0x840000 | address) | rom.ReadByte(0x840000 | (address + 1)) << 8);
}
