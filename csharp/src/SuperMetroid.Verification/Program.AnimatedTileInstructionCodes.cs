using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Verifies the proven bank-$87 vocabulary and executes both real treadmill streams.
    /// A constructed unknown-command stream proves that the interpreter cannot silently
    /// reinterpret a new animated-tile callback as frame data.
    /// </summary>
    static void VerifyAnimatedTileInstructionCodeCatalog()
    {
        AssertAnimatedTileCatalog(typeof(AnimatedTileInstructionCodes), 14);
        AssertAnimatedTileCatalog(typeof(AnimatedTileObjectPointers), 5);
        AssertAnimatedTileCatalog(typeof(AnimatedTileInstructionListPointers), 4);
        VerifyConstructedAnimatedTileStreams();

        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                "  Animated tiles: catalogs and constructed dispatch paths pass; " +
                "retail treadmill streams skipped.");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyRetailTreadmillStream(
            bus,
            WreckedShipTreadmillDirection.Rightwards,
            [
                WreckedShipTreadmillRomData.Frame0Source,
                WreckedShipTreadmillRomData.Frame1Source,
                WreckedShipTreadmillRomData.Frame2Source,
                WreckedShipTreadmillRomData.Frame3Source,
            ]);
        VerifyRetailTreadmillStream(
            bus,
            WreckedShipTreadmillDirection.Leftwards,
            [
                WreckedShipTreadmillRomData.Frame3Source,
                WreckedShipTreadmillRomData.Frame2Source,
                WreckedShipTreadmillRomData.Frame1Source,
                WreckedShipTreadmillRomData.Frame0Source,
            ]);

        Console.WriteLine(
            "  Animated tiles: 20 named bank-$87 pointers, constructed fail-loud " +
            "dispatch, and both retail treadmill streams agree.");
    }

    private static void VerifyConstructedAnimatedTileStreams()
    {
        var bus = new TestAddressSpace();
        WriteTestWords(
            bus,
            0x870000 | AnimatedTileObjectPointers.WreckedShipTreadmillRightwards,
            0x9000,
            WreckedShipTreadmillRomData.TransferByteCount,
            WreckedShipTreadmillRomData.EncodedVramDestination);
        WriteTestWords(
            bus,
            0x879000,
            AnimatedTileInstructionCodes.WaitUntilAreaBossIsDead,
            1,
            0x9100,
            AnimatedTileInstructionCodes.Goto,
            0x9002);

        var state = new WreckedShipTreadmillAnimatedTilesState();
        var writes = new VramWriteQueue();
        state.Start(bus, WreckedShipTreadmillDirection.Rightwards);
        state.Step(bus, areaBossDefeated: false, writes);
        AssertEqual(0, writes.Entries.Count, "animated-tile boss wait suppresses transfer");
        state.Step(bus, areaBossDefeated: true, writes);
        AssertEqual(1, writes.Entries.Count, "animated-tile timed frame queues transfer");
        AssertEqual(0x879100, writes.Entries[0].SourceAddress,
            "animated-tile frame source retains bank 87");

        var unknownBus = new TestAddressSpace();
        WriteTestWords(
            unknownBus,
            0x870000 | AnimatedTileObjectPointers.WreckedShipTreadmillRightwards,
            0x9200,
            WreckedShipTreadmillRomData.TransferByteCount,
            WreckedShipTreadmillRomData.EncodedVramDestination);
        WriteTestWord(unknownBus, 0x879200, 0xdead);
        var unknown = new WreckedShipTreadmillAnimatedTilesState();
        unknown.Start(unknownBus, WreckedShipTreadmillDirection.Rightwards);
        AssertThrows<NotSupportedException>(
            () => unknown.Step(unknownBus, areaBossDefeated: true, new VramWriteQueue()),
            "unknown animated-tile callback fails loudly");
    }

    private static void VerifyRetailTreadmillStream(
        ISnesAddressSpace bus,
        WreckedShipTreadmillDirection direction,
        int[] expectedSources)
    {
        var state = new WreckedShipTreadmillAnimatedTilesState();
        var writes = new VramWriteQueue();
        state.Start(bus, direction);
        state.Step(bus, areaBossDefeated: false, writes);
        AssertEqual(0, writes.Entries.Count, $"{direction} retail boss wait");

        for (int frame = 0; frame < expectedSources.Length; frame++)
        {
            state.Step(bus, areaBossDefeated: true, writes);
            VramWriteEntry entry = writes.Entries[frame];
            AssertEqual(expectedSources[frame], entry.SourceAddress,
                $"{direction} retail source frame {frame}");
            AssertEqual(WreckedShipTreadmillRomData.TransferByteCount, entry.SizeInBytes,
                $"{direction} retail transfer size {frame}");
            AssertEqual(WreckedShipTreadmillRomData.EncodedVramDestination,
                entry.EncodedVramDestination,
                $"{direction} retail VRAM destination {frame}");
        }
    }

    private static void AssertAnimatedTileCatalog(Type catalog, int expectedCount)
    {
        FieldInfo[] fields = GetUshortConstants(catalog);
        AssertEqual(expectedCount, fields.Length, $"{catalog.Name} exhaustive entry count");
        ushort[] pointers = fields.Select(field => (ushort)field.GetRawConstantValue()!).ToArray();
        AssertEqual(pointers.Length, pointers.Distinct().Count(),
            $"{catalog.Name} contains no duplicate pointers");
        foreach (ushort pointer in pointers)
            AssertTrue(pointer >= 0x8000, $"{catalog.Name} pointer ${pointer:X4} is mapped");
    }
}
