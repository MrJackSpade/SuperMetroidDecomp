using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>
    /// Verifies the bank-$87 vocabulary and executes both compiled retail treadmill streams.
    /// Constructed non-retail streams prove that the loader cannot silently
    /// accept unsupported object definitions as compiled mechanics.
    /// </summary>
    static void VerifyAnimatedTileInstructionCodeCatalog()
    {
        AssertAnimatedTileCatalog(typeof(AnimatedTileInstructionCodes), 14);
        AssertAnimatedTileCatalog(typeof(AnimatedTileObjectPointers), 19);
        AssertAnimatedTileCatalog(typeof(AnimatedTileInstructionListPointers), 4);
        Suite(nameof(VerifyConstructedAnimatedTileStreams), () => VerifyConstructedAnimatedTileStreams());

        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                "  Animated tiles: catalogs and constructed dispatch paths pass; " +
                "retail treadmill streams skipped.");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        Suite(nameof(VerifyRetailTreadmillMechanics), () => VerifyRetailTreadmillMechanics(bus));
        Suite(nameof(VerifyRetailTreadmillStream), () => VerifyRetailTreadmillStream(
            bus,
            WreckedShipTreadmillDirection.Rightwards,
            [
                WreckedShipTreadmillRomData.Frame0Source,
                WreckedShipTreadmillRomData.Frame1Source,
                WreckedShipTreadmillRomData.Frame2Source,
                WreckedShipTreadmillRomData.Frame3Source,
            ]));
        Suite(nameof(VerifyRetailTreadmillStream), () => VerifyRetailTreadmillStream(
            bus,
            WreckedShipTreadmillDirection.Leftwards,
            [
                WreckedShipTreadmillRomData.Frame3Source,
                WreckedShipTreadmillRomData.Frame2Source,
                WreckedShipTreadmillRomData.Frame1Source,
                WreckedShipTreadmillRomData.Frame0Source,
            ]));

        Console.WriteLine(
            "  Animated tiles: 37 named bank-$87 pointers, constructed fail-loud " +
            "dispatch, and both retail treadmill streams agree.");
    }

    private static void VerifyConstructedAnimatedTileStreams()
    {
        // Generic ROM dispatch was removed when treadmill mechanics became compiled.
        // Even a well-formed foreign program must now fail at object admission, before
        // an unknown opcode or zero duration could be interpreted as a valid frame.
        ushort[][] programs =
        [
            [AnimatedTileInstructionCodes.WaitUntilAreaBossIsDead, 1, 0x9100,
                AnimatedTileInstructionCodes.Goto, 0x9002],
            [0xdead],
            [0],
        ];
        foreach (ushort[] program in programs)
        {
            var bus = new TestAddressSpace();
            WriteTestWords(bus, 0x878f00, 0x9000,
                WreckedShipTreadmillRomData.TransferByteCount,
                WreckedShipTreadmillRomData.EncodedVramDestination);
            WriteTestWords(bus, 0x879000, program);
            var state = new WreckedShipTreadmillAnimatedTilesState();
            var error = AssertThrows<InvalidDataException>(() => state.StartDefinition(
                bus, WreckedShipTreadmillDirection.Rightwards, 0x8f00),
                "non-retail animated-tile object rejects before interpreting its program");
            AssertTrue(error.Message.Contains("$87:8F00", StringComparison.Ordinal),
                "unsupported animated-tile error identifies the rejected object");
        }
        var mismatched = new WreckedShipTreadmillAnimatedTilesState();
        AssertThrows<InvalidDataException>(() => mismatched.StartDefinition(
            new TestAddressSpace(), WreckedShipTreadmillDirection.Leftwards,
            AnimatedTileObjectPointers.WreckedShipTreadmillRightwards),
            "compiled treadmill definition rejects the opposite direction");
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
        AssertEqual(0, guarded.PresentationReadCount,
            $"{direction} no longer reads frame-source presentation operands");
    }

    private static void VerifyRetailTreadmillMechanics(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyTreadmillHeaderSelection), () => VerifyTreadmillHeaderSelection());
        Suite(nameof(VerifyTreadmillWaitPointers), () => VerifyTreadmillWaitPointers(bus));
        Suite(nameof(VerifyTreadmillFramePointers), () => VerifyTreadmillFramePointers(bus));
        Suite(nameof(VerifyTreadmillDurations), () => VerifyTreadmillDurations(bus));
        Suite(nameof(VerifyTreadmillControlOpcodes), () => VerifyTreadmillControlOpcodes(bus));
        Suite(nameof(VerifyTreadmillLoopTargets), () => VerifyTreadmillLoopTargets(bus));
        Suite(nameof(VerifyTreadmillTransferSizes), () => VerifyTreadmillTransferSizes(bus));
        Suite(nameof(VerifyTreadmillVramDestinations), () => VerifyTreadmillVramDestinations(bus));
        Suite(nameof(VerifyTreadmillMechanicsDomain), () => VerifyTreadmillMechanicsDomain(bus));
        Suite(nameof(VerifyTreadmillArtworkSources), () => VerifyTreadmillArtworkSources(bus));
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
        WreckedShipTreadmillObjectDefinition definition) : ISnesAddressSpace, IImportCartridgeSource
    {
        public int ForbiddenReadAttempts { get; private set; }
        public int PresentationReadCount { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

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
