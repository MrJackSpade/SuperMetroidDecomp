using static SuperMetroid.Verification.Fx.RoomFxAnimatedTileArtworkVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Fx.LookupAnimatedArtworkSourcesVerification;
using static SuperMetroid.Verification.Fx.LookupAnimatedFramesVerification;
using static SuperMetroid.Verification.Fx.LookupAnimatedHeadersVerification;
using static SuperMetroid.Verification.Fx.LookupFxAtlasSegmentsVerification;
using static SuperMetroid.Verification.Fx.LookupFxLegacySheetsVerification;
using static SuperMetroid.Verification.Fx.LookupTreadmillArtworkVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Fx;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled room-FX animation controls and complete loops without live ROM reads.</summary>
internal static class RoomFxAnimatedTileMechanicsDefinitionsVerification
{
    /// <summary>
    /// Compares every compiled room-FX animation control word to the pinned cartridge,
    /// then runs every complete loop while rejecting reads of those mechanics bytes.
    /// </summary>
    internal static void VerifyRoomFxAnimatedTileMechanicsDefinitions()
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
        Suite(nameof(VerifyRoomFxLegacySheetSelection), () => VerifyRoomFxLegacySheetSelection());
        int mechanicsWordCount = 0;
        int frameCount = 0;
        foreach (RoomFxAnimatedTileObjectDefinition definition in
                 RoomFxAnimatedTileMechanicsDefinitions.All)
        {
            mechanicsWordCount += 3;

            foreach (RoomFxAnimatedTileFrameDefinition frame in definition.Frames)
            {
                AssertTrue(!definition.TryReadMechanicsWord(
                        frame.SourceOperandPointer, out _),
                    $"object $87:{(int)definition.ObjectPointer:X4} leaves source operand " +
                    $"$87:{frame.SourceOperandPointer:X4} presentation-owned");
                mechanicsWordCount++;
                frameCount++;
            }

            mechanicsWordCount += 2;

            int presentationReadCount = 0;
            var guarded = NewRoomFxAnimatedTileMechanicsForbiddenBus(bus, definition, () => presentationReadCount++);
            var state = new RoomFxAnimatedTilesState();
            var vram = new SnesVram();
            var writes = new VramWriteQueue();
            state.LoadDefinition(definition.ObjectPointer);

            int steps = definition.Frames.Sum(frame => frame.Duration) + 1;
            int observedFrames = 0;
            for (int step = 0; step < steps; step++)
            {
                state.Step(BgArtworkFixtures.For(vram), writes);
                if (state.LastSourceAddress.HasValue)
                    observedFrames++;
            }

            AssertEqual(definition.Frames.Count + 1, observedFrames,
                $"object $87:{(int)definition.ObjectPointer:X4} executes one complete loop");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"object $87:{(int)definition.ObjectPointer:X4} performs no mechanics ROM reads");
            AssertEqual(0, presentationReadCount,
                $"object $87:{(int)definition.ObjectPointer:X4} reads no compiled artwork-source operands");
        }

        AssertEqual(7, RoomFxAnimatedTileMechanicsDefinitions.All.Count(),
            "simple room-FX animated-tile object count");
        AssertEqual(30, frameCount, "simple room-FX animated-tile frame count");
        AssertEqual(65, mechanicsWordCount,
            "simple room-FX animated-tile compiled mechanics word count");
        Console.WriteLine(
            "  Room-FX animated tiles: 65 control words and 30 artwork-source identities across 7 objects are compiled.");
    }

    /// <summary>Rejects compiled mechanics reads and counts unexpected artwork operand reads in bank $87.</summary>
    private static CartridgeReadGuard NewRoomFxAnimatedTileMechanicsForbiddenBus(
        ISnesAddressSpace inner, RoomFxAnimatedTileObjectDefinition definition, Action presentationRead)
    {
        bool InAnimatedTileBank(int address) =>
            SnesAddress.FromBusAddress(address).Bank == SnesLittleEndian.BankByte(RoomFxRomData.Banks.AnimatedTiles);
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
                    return InAnimatedTileBank(address) && definition.Frames.Any(frame =>
                        offset == frame.SourceOperandPointer ||
                        offset == unchecked((ushort)(frame.SourceOperandPointer + 1)));
                },
                _ => presentationRead());
    }

    /// <summary>
    /// Plays each object's loop one frame past its end through the production state: every frame appears on the
    /// update its predecessor's duration expires, and the last frame is followed by the first, as the list's goto does.
    /// </summary>
    private static void VerifySimpleAnimationLoopWrapsToFirstFrame()
    {
        foreach (RoomFxAnimatedTileObjectDefinition definition in RoomFxAnimatedTileMechanicsDefinitions.All)
        {
            var expected = new List<RoomFxAnimatedTileMechanicsDefinitionsExpected>();
            int update = 0;
            for (int index = 0; index <= definition.Frames.Count; index++)
            {
                RoomFxAnimatedTileFrameDefinition frame = definition.Frames[index % definition.Frames.Count];
                expected.Add(new RoomFxAnimatedTileMechanicsDefinitionsExpected(update, RoomFxAnimatedTileArtworkDefinitions.SourceAddress(definition, frame.InstructionPointer)));
                update += frame.Duration;
            }

            var state = new RoomFxAnimatedTilesState();
            var vram = new SnesVram();
            var writes = new VramWriteQueue();
            state.LoadDefinition(definition.ObjectPointer);
            var observed = new List<RoomFxAnimatedTileMechanicsDefinitionsExpected>();
            for (int step = 0; step <= expected[^1].Update; step++)
            {
                state.Step(BgArtworkFixtures.For(vram), writes);
                if (state.LastSourceAddress is int source)
                    observed.Add(new RoomFxAnimatedTileMechanicsDefinitionsExpected(step, source));
            }
            AssertTrue(expected.SequenceEqual(observed),
                $"{definition.ObjectPointer} shows each frame on its update and returns to the first after the last");
        }
    }

    /// <summary>Runs the room-FX animation control and frame lookup checks.</summary>
    [VerificationCommand("--lookup-animated-frames")]
    internal static int LookupAnimatedFramesCommand()
    {
        Suite(nameof(VerifyRoomFxAnimatedTileMechanicsDefinitions), () => VerifyRoomFxAnimatedTileMechanicsDefinitions());
        return 0;
    }

    /// <summary>Runs both mechanics and artwork checks for room-FX animated tiles.</summary>
    [VerificationCommand("--room-fx-animated-tiles")]
    internal static int RoomFxAnimatedTilesCommand()
    {
        Suite(nameof(VerifyRoomFxAnimatedTileMechanicsDefinitions), () => VerifyRoomFxAnimatedTileMechanicsDefinitions());
        Suite(nameof(VerifyRoomFxAnimatedTileArtwork), () => VerifyRoomFxAnimatedTileArtwork());
        return 0;
    }
}
