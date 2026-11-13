using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Fx.RoomFxBlockAnimationArtworkVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Fx.LookupAnimatedArtworkSourcesVerification;
using static SuperMetroid.Verification.Fx.LookupAnimatedFramesVerification;
using static SuperMetroid.Verification.Fx.LookupAnimatedHeadersVerification;
using static SuperMetroid.Verification.Fx.LookupFxAtlasSegmentsVerification;
using static SuperMetroid.Verification.Fx.LookupTreadmillArtworkVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Fx;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled room-FX animation controls and complete loops without live ROM reads.</summary>
internal static class RoomFxBlockAnimationMechanicsDefinitionsVerification
{
    /// <summary>
    /// Compares every compiled room-FX animation control word to the pinned cartridge,
    /// then runs every complete loop while rejecting reads of those mechanics bytes.
    /// </summary>
    internal static void VerifyRoomFxBlockAnimationMechanicsDefinitions()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        AssertEqual(SuperMetroid.AssetExtraction.Cartridge.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bus.Rom)), "Animation frame oracle revision");
        Suite(nameof(VerifySimpleAnimationObjectDomain), () => VerifySimpleAnimationObjectDomain());
        Suite(nameof(VerifySimpleAnimationLoopWrapsToFirstFrame), () => VerifySimpleAnimationLoopWrapsToFirstFrame());
        Suite(nameof(VerifySimpleAnimationInstructionStarts), () => VerifySimpleAnimationInstructionStarts(bus));
        Suite(nameof(VerifySimpleAnimationTransferSizes), () => VerifySimpleAnimationTransferSizes(bus));
        Suite(nameof(VerifySimpleAnimationVramDestinations), () => VerifySimpleAnimationVramDestinations(bus));
        Suite(nameof(VerifySimpleAnimationFrameCursors), () => VerifySimpleAnimationFrameCursors(bus));
        Suite(nameof(VerifySimpleAnimationFrameDurations), () => VerifySimpleAnimationFrameDurations(bus));
        Suite(nameof(VerifySimpleAnimationArtworkSources), () => VerifySimpleAnimationArtworkSources(bus));
        Suite(nameof(VerifyTreadmillArtworkSources), () => VerifyTreadmillArtworkSources(bus));
        Suite(nameof(VerifyRoomFxAtlasSegmentSources), () => VerifyRoomFxAtlasSegmentSources(bus));
        Suite(nameof(VerifyRoomFxAtlasSegmentSizes), () => VerifyRoomFxAtlasSegmentSizes());
        Suite(nameof(VerifyRoomFxAtlasSegmentRoles), () => VerifyRoomFxAtlasSegmentRoles());
        int mechanicsWordCount = 0;
        int frameCount = 0;
        foreach (RoomFxBlockAnimationLoop definition in
                 RoomFxBlockAnimationLoops.All)
        {
            mechanicsWordCount += 3;

            for (int index = 0; index < definition.Frames.Count; index++)
            {
                ushort operand = definition.FrameSourceOperandPointer(index);
                AssertTrue(!definition.TryReadMechanicsWord(
                        operand, out _),
                    $"object {definition.AnimatedObject} leaves source operand " +
                    $"$87:{operand:X4} presentation-owned");
                mechanicsWordCount++;
                frameCount++;
            }

            mechanicsWordCount += 2;

            int presentationReadCount = 0;
            var guarded = NewRoomFxBlockAnimationMechanicsForbiddenBus(bus, definition, () => presentationReadCount++);
            var state = new RoomFxBlockAnimationsState();
            state.LoadDefinition(definition.AnimatedObject);

            int steps = definition.Frames.Sum(frame => frame.Duration) + 1;
            int observedFrames = 0;
            for (int step = 0; step < steps; step++)
            {
                // Each pass selects into fresh selections, so a selection read back is the one this pass made.
                var frames = new ArtworkFrameSelections();
                state.Step(frames);
                if (frames.TryShown(BlockAnimationFrames.Of(definition.AnimatedObject), out _))
                    observedFrames++;
            }

            AssertEqual(definition.Frames.Count + 1, observedFrames,
                $"object {definition.AnimatedObject} executes one complete loop");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"object {definition.AnimatedObject} performs no mechanics ROM reads");
            AssertEqual(0, presentationReadCount,
                $"object {definition.AnimatedObject} reads no compiled artwork-source operands");
        }

        AssertEqual(7, RoomFxBlockAnimationLoops.All.Count(),
            "simple room-FX animated-tile object count");
        AssertEqual(30, frameCount, "simple room-FX animated-tile frame count");
        AssertEqual(65, mechanicsWordCount,
            "simple room-FX animated-tile compiled mechanics word count");
        Console.WriteLine(
            "  Room-FX animated tiles: 65 control words and 30 artwork-source identities across 7 objects are compiled.");
    }

    /// <summary>Rejects compiled mechanics reads and counts unexpected artwork operand reads in bank $87.</summary>
    private static CartridgeReadGuard NewRoomFxBlockAnimationMechanicsForbiddenBus(
        ISnesAddressSpace inner, RoomFxBlockAnimationLoop definition, Action presentationRead)
    {
        bool InAnimatedTileBank(int address) =>
            SnesAddress.FromBusAddress(address).Bank == SnesLittleEndianConstants.BankByte(RoomFxNativeSources.AnimatedTilesBank);
        return new CartridgeReadGuard(inner)
            .Forbid(address =>
                {
                    SnesAddress source = SnesAddress.FromBusAddress(address);
                    return InAnimatedTileBank(address) &&
                        (definition.TryReadMechanicsWord(source.Offset, out _) ||
                         definition.TryReadMechanicsWord(unchecked((ushort)(source.Offset - 1)), out _));
                },
                address => $"Production read compiled room-FX mechanics byte {SnesAddress.FromBusAddress(address)}.")
            .Observe(address =>
                {
                    ushort offset = SnesAddress.FromBusAddress(address).Offset;
                    return InAnimatedTileBank(address) && Enumerable.Range(0, definition.Frames.Count).Any(index =>
                        offset == definition.FrameSourceOperandPointer(index) ||
                        offset == unchecked((ushort)(definition.FrameSourceOperandPointer(index) + 1)));
                },
                _ => presentationRead());
    }

    /// <summary>
    /// Plays each object's loop one frame past its end through the production state: every frame appears on the
    /// update its predecessor's duration expires, and the last frame is followed by the first, as the list's goto does.
    /// </summary>
    private static void VerifySimpleAnimationLoopWrapsToFirstFrame()
    {
        foreach (RoomFxBlockAnimationLoop definition in RoomFxBlockAnimationLoops.All)
        {
            var expected = new List<RoomFxBlockAnimationMechanicsDefinitionsExpected>();
            int update = 0;
            for (int index = 0; index <= definition.Frames.Count; index++)
            {
                int frame = index % definition.Frames.Count;
                expected.Add(new RoomFxBlockAnimationMechanicsDefinitionsExpected(update,
                    AnimatedTileImageSources.OfCursor(definition, definition.FrameInstructionPointer(frame))));
                update += definition.Frames[frame].Duration;
            }

            var state = new RoomFxBlockAnimationsState();
            state.LoadDefinition(definition.AnimatedObject);
            var observed = new List<RoomFxBlockAnimationMechanicsDefinitionsExpected>();
            for (int step = 0; step <= expected[^1].Update; step++)
            {
                // Each pass selects into fresh selections, so a selection read back is the one this pass made.
                var frames = new ArtworkFrameSelections();
                state.Step(frames);
                if (AnimatedTileImageSources.OfShown(frames, definition.AnimatedObject) is int source)
                    observed.Add(new RoomFxBlockAnimationMechanicsDefinitionsExpected(step, source));
            }
            AssertTrue(expected.SequenceEqual(observed),
                $"{definition.AnimatedObject} shows each frame on its update and returns to the first after the last");
        }
    }

    /// <summary>Runs the room-FX animation control and frame lookup checks.</summary>
    [VerificationCommand("--lookup-animated-frames")]
    internal static int LookupAnimatedFramesCommand()
    {
        Suite(nameof(VerifyRoomFxBlockAnimationMechanicsDefinitions), () => VerifyRoomFxBlockAnimationMechanicsDefinitions());
        return 0;
    }

    /// <summary>Runs both mechanics and artwork checks for room-FX animated tiles.</summary>
    [VerificationCommand("--room-fx-animated-tiles")]
    internal static int RoomFxBlockAnimationsCommand()
    {
        Suite(nameof(VerifyRoomFxBlockAnimationMechanicsDefinitions), () => VerifyRoomFxBlockAnimationMechanicsDefinitions());
        Suite(nameof(VerifyRoomFxBlockAnimationArtwork), () => VerifyRoomFxBlockAnimationArtwork());
        return 0;
    }
}
