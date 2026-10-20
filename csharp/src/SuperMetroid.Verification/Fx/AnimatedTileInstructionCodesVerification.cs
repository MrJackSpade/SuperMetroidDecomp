using SuperMetroid.Tooling;
using System.Runtime.CompilerServices;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.EnemyInstructionCodePointersVerification;
using static SuperMetroid.Verification.Fx.LookupTreadmillArtworkVerification;
using static SuperMetroid.Verification.Fx.LookupTreadmillMechanicsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Fx;
using SuperMetroid.Verification.Harness;

/// <summary>Checks bank-87 animated-tile pointer catalogs and the compiled Wrecked Ship treadmill streams.</summary>
internal static class AnimatedTileInstructionCodesVerification
{
    /// <summary>
    /// Verifies the bank-$87 vocabulary and executes both compiled retail treadmill streams.
    /// Constructed non-retail streams prove that the loader cannot silently
    /// accept unsupported object definitions as compiled mechanics.
    /// </summary>
    internal static void VerifyAnimatedTileInstructionCodeCatalog()
    {
        AssertAnimatedTileCatalog(nameof(AnimatedTileInstruction),
            Enum.GetValues<AnimatedTileInstruction>().Select(code => (ushort)code).ToArray(), 14);
        AssertAnimatedTileCatalog(nameof(AnimatedTileObject),
            Enum.GetValues<AnimatedTileObject>().Where(header => header != AnimatedTileObject.None)
                .Select(header => (ushort)header).ToArray(), 19);
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

        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        Suite(nameof(VerifyRetailTreadmillMechanics), () => VerifyRetailTreadmillMechanics(bus));
        Suite(nameof(VerifyRetailTreadmillStream), () => VerifyRetailTreadmillStream(
            bus,
            WreckedShipTreadmillDirection.Rightwards,
            [
                WreckedShipTreadmillRomData.Frame0Source,
                WreckedShipTreadmillRomData.FrameSource(1),
                WreckedShipTreadmillRomData.FrameSource(2),
                WreckedShipTreadmillRomData.FrameSource(3),
            ]));
        Suite(nameof(VerifyRetailTreadmillStream), () => VerifyRetailTreadmillStream(
            bus,
            WreckedShipTreadmillDirection.Leftwards,
            [
                WreckedShipTreadmillRomData.FrameSource(3),
                WreckedShipTreadmillRomData.FrameSource(2),
                WreckedShipTreadmillRomData.FrameSource(1),
                WreckedShipTreadmillRomData.Frame0Source,
            ]));

        Console.WriteLine(
            "  Animated tiles: 37 named bank-$87 pointers, constructed fail-loud " +
            "dispatch, and both retail treadmill streams agree.");
    }

    /// <summary>Words outside <see cref="AnimatedTileObject"/>, including a constructed bank-$87 header.</summary>
    internal static readonly ushort[] UndefinedAnimatedTileObjects = [0x0001, 0x8f00, 0xffff];

    /// <summary>Checks that foreign and direction-mismatched objects are rejected before any program interpretation.</summary>
    private static void VerifyConstructedAnimatedTileStreams()
    {
        // Generic ROM dispatch was removed when treadmill mechanics became compiled.
        // Even a well-formed foreign program must now fail at object admission, before
        // an unknown opcode or zero duration could be interpreted as a valid frame.
        ushort[][] programs =
        [
            [(ushort)AnimatedTileInstruction.WaitUntilAreaBossIsDead, 1, 0x9100,
                (ushort)AnimatedTileInstruction.Goto, 0x9002],
            [0xdead],
            [0],
        ];
        foreach (ushort[] program in programs)
        {
            TestAddressSpace bus = CreateConstructedLavaAnimatedTileCartridge(program);
            var state = new WreckedShipTreadmillAnimatedTilesState();
            var error = AssertThrows<InvalidDataException>(() => state.StartDefinition(
                WreckedShipTreadmillDirection.Rightwards, AnimatedTileObject.Lava),
                "non-treadmill animated-tile object rejects before interpreting its program");
            AssertTrue(error.Message.Contains("$87:82AB", StringComparison.Ordinal),
                "unsupported animated-tile error identifies the rejected object");
        }
        var mismatched = new WreckedShipTreadmillAnimatedTilesState();
        AssertThrows<InvalidDataException>(() => mismatched.StartDefinition(
            WreckedShipTreadmillDirection.Leftwards,
            AnimatedTileObject.WreckedShipTreadmillRightwards),
            "compiled treadmill definition rejects the opposite direction");
    }

    /// <summary>Creates a non-treadmill bank-87 object with supplied program words to check admission behavior.</summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for a constructed bank-$87 lava animated-tile header and program")]
    private static TestAddressSpace CreateConstructedLavaAnimatedTileCartridge(ushort[] program)
    {
        var bus = new TestAddressSpace();
        WriteTestWords(bus, 0x870000 | (ushort)AnimatedTileObject.Lava, 0x9000,
            WreckedShipTreadmillRomData.TransferByteCount,
            WreckedShipTreadmillRomData.EncodedVramDestination);
        WriteTestWords(bus, 0x879000, program);
        return bus;
    }
    /// <summary>Checks boss wait, four directional DMA sources, loop restart, and forbidden source reads.</summary>
    internal static void VerifyRetailTreadmillStream(
        ISnesAddressSpace bus,
        WreckedShipTreadmillDirection direction,
        int[] expectedSources)
    {
        WreckedShipTreadmillObjectDefinition definition =
            WreckedShipTreadmillMechanicsDefinitions.ForDirection(direction);
        var guarded = NewWreckedShipTreadmillMechanicsForbiddenBus(bus, definition, out StrongBox<int> guardedPresentationReads);
        var state = new WreckedShipTreadmillAnimatedTilesState();
        var writes = new VramWriteQueue();
        state.Start(direction);
        state.Step(areaBossDefeated: false, writes);
        AssertEqual(0, writes.Entries.Count, $"{direction} retail boss wait");

        for (int frame = 0; frame < expectedSources.Length; frame++)
        {
            state.Step(areaBossDefeated: true, writes);
            VramWriteEntry entry = writes.Entries[frame];
            AssertEqual(expectedSources[frame], entry.SourceAddress,
                $"{direction} retail source frame {frame}");
            AssertEqual(WreckedShipTreadmillRomData.TransferByteCount, entry.SizeInBytes,
                $"{direction} retail transfer size {frame}");
            AssertEqual(WreckedShipTreadmillRomData.EncodedVramDestination,
                entry.Destination.Encoded,
                $"{direction} retail VRAM destination {frame}");
        }

        state.Step(areaBossDefeated: true, writes);
        AssertEqual(expectedSources[0], writes.Entries[^1].SourceAddress,
            $"{direction} compiled goto returns to frame zero");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            $"{direction} performs no treadmill mechanics ROM reads");
        AssertEqual(0, guardedPresentationReads.Value,
            $"{direction} no longer reads frame-source presentation operands");
    }

    /// <summary>Runs bounded checks for header, control, timing, transfer, and artwork-source definitions.</summary>
    internal static void VerifyRetailTreadmillMechanics(ISnesAddressSpace bus)
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
    /// <summary>Collects a catalog's ushort constants and checks its count and mapped pointer uniqueness.</summary>
    private static void AssertAnimatedTileCatalog(Type catalog, int expectedCount) =>
        AssertAnimatedTileCatalog(catalog.Name, GetUshortConstants(catalog)
            .Select(field => field.RequiredConstant<ushort>()).ToArray(), expectedCount);

    /// <summary>Checks that an animated-tile pointer set is complete, unique, and in the mapped bank half.</summary>
    private static void AssertAnimatedTileCatalog(string name, ushort[] pointers, int expectedCount)
    {
        AssertEqual(expectedCount, pointers.Length, $"{name} exhaustive entry count");
        AssertEqual(pointers.Length, pointers.Distinct().Count(),
            $"{name} contains no duplicate pointers");
        foreach (ushort pointer in pointers)
            AssertTrue(pointer >= 0x8000, $"{name} pointer ${pointer:X4} is mapped");
    }

    /// <summary>Rejects compiled treadmill control reads and counts obsolete frame-source operand reads.</summary>
    private static CartridgeReadGuard NewWreckedShipTreadmillMechanicsForbiddenBus(
        ISnesAddressSpace inner,
        WreckedShipTreadmillObjectDefinition definition,
        out StrongBox<int> presentationReads)
    {
        var reads = new StrongBox<int>();
        presentationReads = reads;
        bool InTreadmillBank(int address) =>
            SnesAddress.FromBusAddress(address).Bank == SnesLittleEndian.BankByte(RoomFxRomData.Banks.AnimatedTiles);
        return new CartridgeReadGuard(inner)
            .Forbid(address =>
                {
                    if (!InTreadmillBank(address))
                        return false;
                    ushort offset = SnesAddress.FromBusAddress(address).Offset;
                    return definition.TryReadMechanicsWord(offset, out _) ||
                        definition.TryReadMechanicsWord(unchecked((ushort)(offset - 1)), out _);
                },
                address => $"Production read compiled treadmill mechanics byte {SnesAddress.FromBusAddress(address)}.")
            .Observe(address =>
                {
                    if (!InTreadmillBank(address))
                        return false;
                    ushort offset = SnesAddress.FromBusAddress(address).Offset;
                    return definition.FrameInstructionPointers.Any(pointer =>
                        offset == unchecked((ushort)(pointer + 2)) ||
                        offset == unchecked((ushort)(pointer + 3)));
                },
                _ => reads.Value++);
    }

    /// <summary>Runs the animated-tile and treadmill suite through the verification command dispatcher.</summary>
    [VerificationCommand("--treadmill-animated-tiles")]
    internal static int TreadmillAnimatedTilesCommand()
    {
        Suite(nameof(VerifyAnimatedTileInstructionCodeCatalog), () => VerifyAnimatedTileInstructionCodeCatalog());
        return 0;
    }
}
