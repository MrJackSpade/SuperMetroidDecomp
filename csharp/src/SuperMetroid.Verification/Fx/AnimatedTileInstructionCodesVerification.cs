using SuperMetroid.Core.Assets;
using SuperMetroid.Tooling;
using System.Runtime.CompilerServices;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Enemies.EnemyInstructionCodePointersVerification;
using static SuperMetroid.Verification.Fx.LookupTreadmillArtworkVerification;
using static SuperMetroid.Verification.Fx.LookupTreadmillMechanicsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Verification.Fx;
using SuperMetroid.Verification.Harness;

/// <summary>Checks bank-87 animated-tile pointer catalogs and the compiled Wrecked Ship treadmill streams.</summary>
internal static class AnimatedTileInstructionCodesVerification
{
    /// <summary>
    /// Verifies the bank-$87 vocabulary and executes both compiled retail treadmill streams.
    /// </summary>
    internal static void VerifyAnimatedTileInstructionCodeCatalog()
    {
        AssertAnimatedTileCatalog(nameof(AnimatedTileInstruction),
            Enum.GetValues<AnimatedTileInstruction>().Select(code => (ushort)code).ToArray(), 14);
        AssertAnimatedTileCatalog(nameof(BlockAnimation),
            Enum.GetValues<BlockAnimation>()
                .Select(FxNativeWords.AnimatedTileObjects.Word).ToArray(), 13);
        AssertAnimatedTileCatalog(nameof(AnimatedTileAnimationScriptPointers),
            [AnimatedTileAnimationScriptPointers.WreckedShipTreadmillRightwardsWait, AnimatedTileAnimationScriptPointers.WreckedShipTreadmillLeftwardsWait,
             AnimatedTileAnimationScriptPointers.WreckedShipTreadmillLeftwardsLoop, AnimatedTileAnimationScriptPointers.WreckedShipTreadmillRightwardsLoop], 4);

        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                "  Animated tiles: catalogs pass; " +
                "retail treadmill streams skipped.");
            return;
        }

        CartridgeImportAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        Suite(nameof(VerifyRetailTreadmillMechanics), () => VerifyRetailTreadmillMechanics(bus));
        Suite(nameof(VerifyRetailTreadmillStream), () => VerifyRetailTreadmillStream(
            bus,
            WreckedShipTreadmillDirection.Rightwards,
            [
                AnimatedTileImageSources.Treadmill(0),
                AnimatedTileImageSources.Treadmill(1),
                AnimatedTileImageSources.Treadmill(2),
                AnimatedTileImageSources.Treadmill(3),
            ]));
        Suite(nameof(VerifyRetailTreadmillStream), () => VerifyRetailTreadmillStream(
            bus,
            WreckedShipTreadmillDirection.Leftwards,
            [
                AnimatedTileImageSources.Treadmill(3),
                AnimatedTileImageSources.Treadmill(2),
                AnimatedTileImageSources.Treadmill(1),
                AnimatedTileImageSources.Treadmill(0),
            ]));

        Console.WriteLine(
            "  Animated tiles: 37 named bank-$87 pointers and both retail treadmill streams agree.");
    }

    /// <summary>Values naming no <see cref="BlockAnimation"/> member (its values are arbitrary, #1415), including a constructed bank-$87 header word.</summary>
    internal static readonly ushort[] UndefinedAnimatedTileObjects = [0x0100, 0x8f00, 0xffff];

    /// <summary>Checks boss wait, four directional DMA sources, loop restart, and forbidden source reads.</summary>
    internal static void VerifyRetailTreadmillStream(
        ISnesAddressSpace bus,
        WreckedShipTreadmillDirection direction,
        int[] expectedSources)
    {
        BlockAnimation definition = BlockAnimationLoops.Treadmill(direction);
        var guarded = NewWreckedShipTreadmillMechanicsForbiddenBus(bus, definition, out StrongBox<int> guardedPresentationReads);
        var state = new RoomBlockAnimations(null, null);
        var frames = new ArtworkFrameSelections();
        state.StartDoorTreadmill(direction);
        state.Step(areaBossDefeated: false, frames);
        AssertTrue(!frames.TryShown(BlockAnimationFrames.Of(definition), out _), $"{direction} retail boss wait");

        // Each pass selects into fresh selections, so a selection read back is the one this pass made.
        for (int frame = 0; frame < expectedSources.Length; frame++)
        {
            frames = new ArtworkFrameSelections();
            state.Step(areaBossDefeated: true, frames);
            AssertTrue(frames.TryShown(BlockAnimationFrames.Of(definition), out ArtworkFrame shown) &&
                    shown == BlockAnimationLoops.Frames(definition)[frame].Frame,
                $"{direction} retail frame {frame} is the frame the treadmill shows");
            AssertEqual(expectedSources[frame], AnimatedTileImageSources.OfShown(frames, definition),
                $"{direction} retail source frame {frame}");
        }

        frames = new ArtworkFrameSelections();
        state.Step(areaBossDefeated: true, frames);
        AssertTrue(frames.TryShown(BlockAnimationFrames.Of(definition), out ArtworkFrame wrapped) &&
                wrapped == BlockAnimationLoops.Frames(definition)[0].Frame,
            $"{direction} compiled goto returns to frame zero");
        AssertEqual(expectedSources[0], AnimatedTileImageSources.OfShown(frames, definition),
            $"{direction} compiled goto returns to frame zero's source");
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
        BlockAnimation definition,
        out StrongBox<int> presentationReads)
    {
        var reads = new StrongBox<int>();
        presentationReads = reads;
        bool InTreadmillBank(int address) =>
            SnesAddress.FromBusAddress(address).Bank == SnesLittleEndianConstants.BankByte(RoomFxNativeSources.AnimatedTilesBank);
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
