using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

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
        AssertAnimatedTileCatalog(typeof(AnimatedTileObjectPointers), 7);
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
        VerifyRetailTreadmillMechanics(bus);
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
            "  Animated tiles: 25 named bank-$87 pointers, constructed fail-loud " +
            "dispatch, and both retail treadmill streams agree.");
    }

    private static void VerifyConstructedAnimatedTileStreams()
    {
        var bus = new TestAddressSpace();
        WriteTestWords(
            bus,
            0x878f00,
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
        state.StartDefinition(bus, WreckedShipTreadmillDirection.Rightwards, 0x8f00);
        state.Step(bus, areaBossDefeated: false, writes);
        AssertEqual(0, writes.Entries.Count, "animated-tile boss wait suppresses transfer");
        state.Step(bus, areaBossDefeated: true, writes);
        AssertEqual(1, writes.Entries.Count, "animated-tile timed frame queues transfer");
        AssertEqual(0x879100, writes.Entries[0].SourceAddress,
            "animated-tile frame source retains bank 87");

        var unknownBus = new TestAddressSpace();
        WriteTestWords(
            unknownBus,
            0x878f00,
            0x9200,
            WreckedShipTreadmillRomData.TransferByteCount,
            WreckedShipTreadmillRomData.EncodedVramDestination);
        WriteTestWord(unknownBus, 0x879200, 0xdead);
        var unknown = new WreckedShipTreadmillAnimatedTilesState();
        unknown.StartDefinition(
            unknownBus,
            WreckedShipTreadmillDirection.Rightwards,
            0x8f00);
        AssertThrows<NotSupportedException>(
            () => unknown.Step(unknownBus, areaBossDefeated: true, new VramWriteQueue()),
            "unknown animated-tile callback fails loudly");

        var zeroBus = new TestAddressSpace();
        WriteTestWords(
            zeroBus,
            0x878f00,
            0x9300,
            WreckedShipTreadmillRomData.TransferByteCount,
            WreckedShipTreadmillRomData.EncodedVramDestination);
        WriteTestWord(zeroBus, 0x879300, 0);
        var zero = new WreckedShipTreadmillAnimatedTilesState();
        zero.StartDefinition(zeroBus, WreckedShipTreadmillDirection.Rightwards, 0x8f00);
        AssertThrows<InvalidDataException>(
            () => zero.Step(zeroBus, areaBossDefeated: true, new VramWriteQueue()),
            "zero-duration animated-tile frame fails loudly");
    }

    private static void VerifyRetailTreadmillStream(
        ISnesAddressSpace bus,
        WreckedShipTreadmillDirection direction,
        int[] expectedSources)
    {
        WreckedShipTreadmillObjectDefinition definition =
            WreckedShipTreadmillMechanicsDefinitions.ForDirection(direction);
        var guarded = new WreckedShipTreadmillMechanicsForbiddenBus(bus, definition);
        var state = new WreckedShipTreadmillAnimatedTilesState();
        var writes = new VramWriteQueue();
        state.Start(guarded, direction);
        state.Step(guarded, areaBossDefeated: false, writes);
        AssertEqual(0, writes.Entries.Count, $"{direction} retail boss wait");

        for (int frame = 0; frame < expectedSources.Length; frame++)
        {
            state.Step(guarded, areaBossDefeated: true, writes);
            VramWriteEntry entry = writes.Entries[frame];
            AssertEqual(expectedSources[frame], entry.SourceAddress,
                $"{direction} retail source frame {frame}");
            AssertEqual(WreckedShipTreadmillRomData.TransferByteCount, entry.SizeInBytes,
                $"{direction} retail transfer size {frame}");
            AssertEqual(WreckedShipTreadmillRomData.EncodedVramDestination,
                entry.EncodedVramDestination,
                $"{direction} retail VRAM destination {frame}");
        }

        state.Step(guarded, areaBossDefeated: true, writes);
        AssertEqual(expectedSources[0], writes.Entries[^1].SourceAddress,
            $"{direction} compiled goto returns to frame zero");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            $"{direction} performs no treadmill mechanics ROM reads");
        AssertEqual(10, guarded.PresentationReadCount,
            $"{direction} retains five live source-pointer reads");
    }

    private static void VerifyRetailTreadmillMechanics(ISnesAddressSpace bus)
    {
        int wordCount = 0;
        foreach (WreckedShipTreadmillObjectDefinition definition in
                 WreckedShipTreadmillMechanicsDefinitions.All)
        {
            var expectedWords = new Dictionary<ushort, ushort>
            {
                [definition.ObjectPointer] = definition.WaitInstructionPointer,
                [unchecked((ushort)(definition.ObjectPointer + 2))] =
                    WreckedShipTreadmillRomData.TransferByteCount,
                [unchecked((ushort)(definition.ObjectPointer + 4))] =
                    WreckedShipTreadmillRomData.EncodedVramDestination,
                [definition.WaitInstructionPointer] =
                    AnimatedTileInstructionCodes.WaitUntilAreaBossIsDead,
                [definition.GotoInstructionPointer] = AnimatedTileInstructionCodes.Goto,
                [unchecked((ushort)(definition.GotoInstructionPointer + 2))] =
                    definition.LoopInstructionPointer,
            };
            foreach (ushort framePointer in definition.FrameInstructionPointers)
                expectedWords.Add(framePointer, 1);

            AssertEqual(10, expectedWords.Count,
                $"{definition.Direction} treadmill mechanics word count");
            foreach ((ushort pointer, ushort expected) in expectedWords)
            {
                AssertTrue(definition.TryReadMechanicsWord(pointer, out ushort compiled),
                    $"{definition.Direction} catalogs $87:{pointer:X4}");
                AssertEqual(expected, compiled,
                    $"{definition.Direction} compiled $87:{pointer:X4}");
                AssertEqual(expected, RomDataReader.ReadWordFixedBank(
                        bus, RoomFxRomData.Banks.AnimatedTiles | pointer),
                    $"{definition.Direction} cartridge $87:{pointer:X4}");
                wordCount++;
            }

            foreach (ushort framePointer in definition.FrameInstructionPointers)
            {
                AssertTrue(!definition.TryReadMechanicsWord(
                        unchecked((ushort)(framePointer + 2)), out _),
                    $"{definition.Direction} leaves frame source presentation-owned");
            }
        }

        AssertEqual(20, wordCount, "compiled treadmill mechanics word count");
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

    private sealed class WreckedShipTreadmillMechanicsForbiddenBus(
        ISnesAddressSpace inner,
        WreckedShipTreadmillObjectDefinition definition) : ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }
        public int PresentationReadCount { get; private set; }

        public byte ReadByte(int address)
        {
            SnesAddress source = SnesAddress.FromBusAddress(address);
            if (source.Bank == (byte)(RoomFxRomData.Banks.AnimatedTiles >> 16))
            {
                if (definition.TryReadMechanicsWord(source.Offset, out _) ||
                    definition.TryReadMechanicsWord(
                        unchecked((ushort)(source.Offset - 1)), out _))
                {
                    ForbiddenReadAttempts++;
                    throw new InvalidOperationException(
                        $"Production read compiled treadmill mechanics byte {source}.");
                }

                if (definition.FrameInstructionPointers.Any(pointer =>
                        source.Offset == unchecked((ushort)(pointer + 2)) ||
                        source.Offset == unchecked((ushort)(pointer + 3))))
                {
                    PresentationReadCount++;
                }
            }

            return inner.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
    }
}
