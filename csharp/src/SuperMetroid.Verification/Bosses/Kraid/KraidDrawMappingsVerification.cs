using static SuperMetroid.Verification.Rooms.Plms.PlmRunHeaderAssertions;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Rooms;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks Kraid room PLM ownership, draw records, native words, and editable visual identities.</summary>
internal static class KraidDrawMappingsVerification
{
    /// <summary>Compares the eight owner headers with bank-$84 programs and checks classification across all header values.</summary>
    internal static void VerifyKraidDrawOwnerClassification(SuperMetroidAddressSpace rom)
    {
        // Independent native header/list identities from bank_84.asm B7A3..B7C1.
        // B79F is the unused background-one setter, outside the managed owner set.
        KraidDrawMappingsOwner[] owners =
        [
            new KraidDrawMappingsOwner(0xb7a3, 0xab6d), new KraidDrawMappingsOwner(0xb7a7, 0xab8b),
            new KraidDrawMappingsOwner(0xb7ab, 0xab7f), new KraidDrawMappingsOwner(0xb7af, 0xab9d),
            new KraidDrawMappingsOwner(0xb7b3, 0xab91), new KraidDrawMappingsOwner(0xb7b7, 0xaba3),
            new KraidDrawMappingsOwner(0xb7bb, 0xabdd), new KraidDrawMappingsOwner(0xb7bf, 0xaba9),
        ];
        foreach (var owner in owners)
            AssertEqual(owner.Program, CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x840000 | (owner.Header + 2)),
                "Kraid draw owner native program identity");
    }

    /// <summary>Independent native record positions, run lengths, and visual names for Kraid's ten draw lists.</summary>
    private static Definition[] KraidDrawOracle() =>
    [
        new Definition(0x9367, 1, "crumble-first"), new Definition(0x936d, 1, "crumble-second"),
        new Definition(0x9373, 1, "crumble-third"), new Definition(0x9379, 1, "ceiling-background-one"),
        new Definition(0x937f, 1, "ceiling-background-two"), new Definition(0x9385, 1, "ceiling-background-three"),
        new Definition(0x9391, 1, "spike-first"), new Definition(0x9397, 1, "spike-second"),
        new Definition(0x939d, 15, "clear-ceiling"), new Definition(0x93bf, 22, "clear-spikes"),
    ];

    /// <summary>Checks ordered draw addresses, exact pointer membership, and ordinal bounds.</summary>
    internal static void VerifyKraidDrawAddresses()
    {
        var expected = KraidDrawOracle();
        AssertEqual(expected.Length, KraidRoomPlmDrawDefinitions.DrawCount, "Kraid draw count");
        AssertTrue(expected.Select(draw => draw.Pointer).SequenceEqual(
            KraidRoomPlmDraws.All.Select(draw => PlmDrawAddressCodec.Of(draw.Draw))), "Kraid export draw order");
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index].Pointer, PlmDrawAddressCodec.Of(KraidRoomPlmDraws.All.ElementAt(index).Draw),
                "Kraid original draw record position");
        var pointers = expected.Select(draw => draw.Pointer).ToHashSet();
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
        {
            bool exists = pointers.Contains((ushort)pointer);
            AssertEqual(exists, KraidRoomPlmDraws.TryGet(NativePlmDraws.At<KraidRoomDraw>((ushort)pointer), out var draw),
                "Kraid full draw-pointer domain");
            if (exists)
                AssertEqual((ushort)pointer, PlmDrawAddressCodec.Of(PlmDraw.Of(draw.Frame)), "Kraid draw address");
            else
                AssertEqual(default(KraidRoomPlmDraws.Draw), draw, "Kraid draw default on miss");
        }
        foreach (int index in new[] { int.MinValue, -1, 10, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => KraidRoomPlmDraws.All.ElementAt(index),
                "Kraid draw ordinal bounds");
    }

    /// <summary>Checks each native horizontal run count, exported run extent, and zero-displacement terminator.</summary>
    internal static void VerifyKraidDrawShapes(SuperMetroidAddressSpace rom)
    {
        var exported = KraidRoomPlmDraws.All.ToDictionary(draw => PlmDrawAddressCodec.Of(draw.Draw));
        foreach (var expected in KraidDrawOracle())
        {
            AssertTrue(KraidRoomPlmDraws.TryGet(NativePlmDraws.At<KraidRoomDraw>(expected.Pointer), out var draw), "Kraid draw exists");
            ushort directionCount = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x840000 | expected.Pointer);
            AssertEqual((ushort)expected.Count, directionCount, "Kraid native horizontal run count");
            AssertEqual(expected.Count, draw.BlockCount, "Kraid calculated block count");
            AssertEqual(1, exported[expected.Pointer].Runs.Length, "Kraid export has one run");
            var run = exported[expected.Pointer].Runs.Span[0];
            AssertRunHeader(directionCount, run, "Kraid export direction/count");
            AssertEqual(expected.Count, run.Count, "Kraid export word extent");
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
        IEnumerable<Definition> definitions)
    {
        var exported = KraidRoomPlmDraws.All.ToDictionary(draw => PlmDrawAddressCodec.Of(draw.Draw));
        foreach (var expected in definitions)
        {
            AssertTrue(KraidRoomPlmDraws.TryGet(NativePlmDraws.At<KraidRoomDraw>(expected.Pointer), out var draw), "Kraid word owner exists");
            for (int block = 0; block < expected.Count; block++)
            {
                RoomTerrainBlock native = RoomLevelWordCodec.Decode(
                    CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x840000 | (expected.Pointer + 2 + block * 2)));
                AssertEqual(native, draw.BlockAt(block), "Kraid calculated block matches the native level word");
                AssertEqual(native, exported[expected.Pointer].Runs.Span[0].Blocks.Span[block],
                    "Kraid materialized export block");
            }
            foreach (int block in new[] { int.MinValue, -1, expected.Count, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => draw.BlockAt(block), "Kraid computed block bounds");
        }
    }

    /// <summary>Checks forward and reverse visual-ID lookup, including case-sensitive rejection and unknown pointers.</summary>
    internal static void VerifyKraidDrawVisualIds()
    {
        var expected = KraidDrawOracle();
        foreach (var entry in expected)
        {
            AssertEqual(entry.Id, KraidRoomPlmDraws.VisualId(NativePlmDraws.At<KraidRoomDraw>(entry.Pointer)), "Kraid published visual ID");
            AssertTrue(KraidRoomPlmDraws.TryGetByVisualId(entry.Id, out var draw), "Kraid reverse visual ID");
            AssertEqual(entry.Pointer, PlmDrawAddressCodec.Of(PlmDraw.Of(draw.Frame)), "Kraid reverse ID pointer");
            foreach (string invalid in new[] { entry.Id.ToUpperInvariant(), entry.Id + " ", " " + entry.Id })
            {
                AssertTrue(!KraidRoomPlmDraws.TryGetByVisualId(invalid, out var missing),
                    "Kraid IDs preserve ordinal exact matching");
                AssertEqual(default(KraidRoomPlmDraws.Draw), missing, "Kraid missing ID clears result");
            }
        }
        foreach (string invalid in new[] { "", "unknown" })
            AssertTrue(!KraidRoomPlmDraws.TryGetByVisualId(invalid, out _), "Kraid invalid visual ID");
        var pointers = expected.Select(draw => draw.Pointer).ToHashSet();
        for (int pointer = 0x9365; pointer <= 0x93ef; pointer++)
            if (!pointers.Contains((ushort)pointer))
                AssertThrows<InvalidDataException>(() => KraidRoomPlmDraws.VisualId(NativePlmDraws.At<KraidRoomDraw>((ushort)pointer)),
                    "Kraid unknown pointer has no visual ID");
    }
}
