using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.AssetExtraction;

internal static partial class Program
{
    /// <summary>
    /// Compares every compiled room-FX animation control word to the pinned cartridge,
    /// then runs every complete loop while rejecting reads of those mechanics bytes.
    /// </summary>
    private static void VerifyRoomFxAnimatedTileMechanicsDefinitions()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        AssertEqual(SuperMetroid.AssetExtraction.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bus.Rom)), "Animation frame oracle revision");
        Suite(nameof(VerifySimpleAnimationObjectDomain), () => VerifySimpleAnimationObjectDomain());
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
            state.LoadDefinition(guarded, definition.ObjectPointer);

            int steps = definition.Frames.Sum(frame => frame.Duration) + 1;
            int observedFrames = 0;
            for (int step = 0; step < steps; step++)
            {
                state.Step(guarded, vram, writes);
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

    private static CartridgeReadGuard NewRoomFxAnimatedTileMechanicsForbiddenBus(
        ISnesAddressSpace inner, RoomFxAnimatedTileObjectDefinition definition, Action presentationRead)
    {
        bool InAnimatedTileBank(int address) =>
            SnesAddress.FromBusAddress(address).Bank == (byte)(RoomFxRomData.Banks.AnimatedTiles >> 16);
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
}
