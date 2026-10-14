using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuiteTimingVerification;

/// <summary>Checks Kraid room PLM ownership, draw records, native words, and editable visual identities.</summary>
internal static class KraidDrawMappingsVerification
{
    /// <summary>Compares the eight owner headers with bank-$84 programs and checks classification across all header values.</summary>
    internal static void VerifyKraidDrawOwnerClassification(SuperMetroidAddressSpace rom)
    {
        // Independent native header/list identities from bank_84.asm B7A3..B7C1.
        // B79F is the unused background-one setter, outside the managed owner set.
        (ushort Header, ushort Program)[] owners =
        [
            (0xb7a3, 0xab6d), (0xb7a7, 0xab8b),
            (0xb7ab, 0xab7f), (0xb7af, 0xab9d),
            (0xb7b3, 0xab91), (0xb7b7, 0xaba3),
            (0xb7bb, 0xabdd), (0xb7bf, 0xaba9),
        ];
        foreach (var owner in owners)
            AssertEqual(owner.Program, CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x840000 | (owner.Header + 2)),
                "Kraid draw owner native program identity");
        var headers = owners.Select(owner => owner.Header).ToHashSet();
        for (int header = 0; header <= ushort.MaxValue; header++)
            AssertEqual(headers.Contains((ushort)header),
                KraidRoomPlmDrawDefinitions.IsKraidOwner((PlmHeaderId)header),
                "Kraid visual owner classification across full header domain");
    }

    /// <summary>Independent native record positions, run lengths, and visual names for Kraid's ten draw lists.</summary>
    private static (ushort Pointer, int Count, string Id)[] KraidDrawOracle() =>
    [
        (0x9367, 1, "crumble-first"), (0x936d, 1, "crumble-second"),
        (0x9373, 1, "crumble-third"), (0x9379, 1, "ceiling-background-one"),
        (0x937f, 1, "ceiling-background-two"), (0x9385, 1, "ceiling-background-three"),
        (0x9391, 1, "spike-first"), (0x9397, 1, "spike-second"),
        (0x939d, 15, "clear-ceiling"), (0x93bf, 22, "clear-spikes"),
    ];

    /// <summary>Checks ordered draw addresses, exact pointer membership, and ordinal bounds.</summary>
    internal static void VerifyKraidDrawAddresses()
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

    /// <summary>Checks each native horizontal run count, exported run extent, and zero-displacement terminator.</summary>
    internal static void VerifyKraidDrawShapes(SuperMetroidAddressSpace rom)
    {
        var exported = KraidRoomPlmDrawDefinitions.All.ToDictionary(draw => draw.Pointer);
        foreach (var expected in KraidDrawOracle())
        {
            AssertTrue(KraidRoomPlmDrawDefinitions.TryGet(expected.Pointer, out var draw), "Kraid draw exists");
            ushort directionCount = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x840000 | expected.Pointer);
            AssertEqual((ushort)expected.Count, directionCount, "Kraid native horizontal run count");
            AssertEqual(expected.Count, draw.BlockCount, "Kraid calculated block count");
            AssertEqual(1, exported[expected.Pointer].Runs.Length, "Kraid export has one run");
            var run = exported[expected.Pointer].Runs.Span[0];
            AssertEqual(directionCount, run.DirectionAndCount, "Kraid export direction/count");
            AssertEqual(expected.Count, run.LevelWords.Length, "Kraid export word extent");
            ushort tail = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x840000 | (expected.Pointer + 2 + expected.Count * 2));
            AssertEqual((ushort)0, tail, "Kraid native terminator has zero displacement");
            AssertEqual((sbyte)0, run.Next.X, "Kraid export terminal X");
            AssertEqual((sbyte)0, run.Next.Y, "Kraid export terminal Y");
        }
    }

    /// <summary>Checks single-block records and both long clearing runs against their native level words.</summary>
    internal static void VerifyKraidDrawWords(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyKraidSingleDrawWords), () => VerifyKraidSingleDrawWords(rom));
        Suite(nameof(VerifyKraidCeilingClearWords), () => VerifyKraidCeilingClearWords(rom));
        Suite(nameof(VerifyKraidSpikeClearWords), () => VerifyKraidSpikeClearWords(rom));
    }

    /// <summary>Checks the first eight one-block Kraid draw records.</summary>
    private static void VerifyKraidSingleDrawWords(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidDrawWordSet), () => VerifyKraidDrawWordSet(rom, KraidDrawOracle().Take(8)));

    /// <summary>Checks the fifteen-block ceiling clearing record.</summary>
    private static void VerifyKraidCeilingClearWords(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidDrawWordSet), () => VerifyKraidDrawWordSet(rom, KraidDrawOracle().Where(draw => draw.Pointer == 0x939d)));

    /// <summary>Checks the twenty-two-block spike clearing record.</summary>
    private static void VerifyKraidSpikeClearWords(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidDrawWordSet), () => VerifyKraidDrawWordSet(rom, KraidDrawOracle().Where(draw => draw.Pointer == 0x93bf)));

    /// <summary>Compares physical and visual level words in the compiled and exported forms of selected records.</summary>
    private static void VerifyKraidDrawWordSet(SuperMetroidAddressSpace rom,
        IEnumerable<(ushort Pointer, int Count, string Id)> definitions)
    {
        var exported = KraidRoomPlmDrawDefinitions.All.ToDictionary(draw => draw.Pointer);
        foreach (var expected in definitions)
        {
            AssertTrue(KraidRoomPlmDrawDefinitions.TryGet(expected.Pointer, out var draw), "Kraid word owner exists");
            for (int block = 0; block < expected.Count; block++)
            {
                ushort word = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x840000 | (expected.Pointer + 2 + block * 2));
                AssertEqual(word, draw.WordAt(block), "Kraid calculated original physical/visual word");
                AssertEqual(word, exported[expected.Pointer].Runs.Span[0].LevelWords.Span[block],
                    "Kraid materialized export word");
            }
            foreach (int block in new[] { int.MinValue, -1, expected.Count, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => draw.WordAt(block), "Kraid computed word bounds");
        }
    }

    /// <summary>Checks forward and reverse visual-ID lookup, including case-sensitive rejection and unknown pointers.</summary>
    internal static void VerifyKraidDrawVisualIds()
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
        foreach (string invalid in new[] { "", "unknown" })
            AssertTrue(!KraidRoomPlmDrawDefinitions.TryGetByVisualId(invalid, out _), "Kraid invalid visual ID");
        var pointers = expected.Select(draw => draw.Pointer).ToHashSet();
        for (int pointer = 0x9365; pointer <= 0x93ef; pointer++)
            if (!pointers.Contains((ushort)pointer))
                AssertThrows<InvalidDataException>(() => KraidRoomPlmDrawDefinitions.VisualId((ushort)pointer),
                    "Kraid unknown pointer has no visual ID");
    }
}
