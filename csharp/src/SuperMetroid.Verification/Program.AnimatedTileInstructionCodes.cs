using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using System.Runtime.CompilerServices;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using static AssertionsVerification;
using static EnemyInstructionCodePointersVerification;
using static LookupTreadmillArtworkVerification;
using static LookupTreadmillMechanicsVerification;
using static SuiteTimingVerification;
using static VerificationHelpersVerification;

/// <summary>Represents animated tile instruction codes verification.</summary>
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

    /// <summary>Words outside <see cref="AnimatedTileObject"/>, including a constructed bank-$87 header.</summary>
    internal static readonly ushort[] UndefinedAnimatedTileObjects = [0x0001, 0x8f00, 0xffff];

    /// <summary>Verifies constructed animated tile streams.</summary>
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
                bus, WreckedShipTreadmillDirection.Rightwards, AnimatedTileObject.Lava),
                "non-treadmill animated-tile object rejects before interpreting its program");
            AssertTrue(error.Message.Contains("$87:82AB", StringComparison.Ordinal),
                "unsupported animated-tile error identifies the rejected object");
        }
        var mismatched = new WreckedShipTreadmillAnimatedTilesState();
        AssertThrows<InvalidDataException>(() => mismatched.StartDefinition(
            new TestAddressSpace(), WreckedShipTreadmillDirection.Leftwards,
            AnimatedTileObject.WreckedShipTreadmillRightwards),
            "compiled treadmill definition rejects the opposite direction");
    }

    /// <summary>Creates constructed lava animated tile cartridge.</summary>
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
    /// <summary>Verifies retail treadmill stream.</summary>
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
        AssertEqual(0, guardedPresentationReads.Value,
            $"{direction} no longer reads frame-source presentation operands");
    }

    /// <summary>Verifies retail treadmill mechanics.</summary>
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
    /// <summary>Asserts animated tile catalog.</summary>
    private static void AssertAnimatedTileCatalog(Type catalog, int expectedCount) =>
        AssertAnimatedTileCatalog(catalog.Name, GetUshortConstants(catalog)
            .Select(field => (ushort)field.GetRawConstantValue()!).ToArray(), expectedCount);

    /// <summary>Asserts animated tile catalog.</summary>
    private static void AssertAnimatedTileCatalog(string name, ushort[] pointers, int expectedCount)
    {
        AssertEqual(expectedCount, pointers.Length, $"{name} exhaustive entry count");
        AssertEqual(pointers.Length, pointers.Distinct().Count(),
            $"{name} contains no duplicate pointers");
        foreach (ushort pointer in pointers)
            AssertTrue(pointer >= 0x8000, $"{name} pointer ${pointer:X4} is mapped");
    }

    /// <summary>New wrecked ship treadmill mechanics forbidden bus.</summary>
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

    /// <summary>Treadmill animated tiles command.</summary>
    [VerificationCommand("--treadmill-animated-tiles")]
    internal static int TreadmillAnimatedTilesCommand()
    {
        Suite(nameof(VerifyAnimatedTileInstructionCodeCatalog), () => VerifyAnimatedTileInstructionCodeCatalog());
        return 0;
    }
}
