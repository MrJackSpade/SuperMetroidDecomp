using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Fx;

/// <summary>Retail checks for room-FX, treadmill, and Tourian statue animated-tile PNG frames.</summary>
internal static class RoomFxBlockAnimationArtworkVerification
{
    /// <summary>Checks retail room-FX and treadmill frames against native bytes and the frame each animation shows.</summary>
    internal static void VerifyRoomFxBlockAnimationArtwork()
    {
        if (!File.Exists("Super Metroid.smc"))
        {
            Console.WriteLine("  Room-FX animation artwork: retail comparison skipped (private ROM absent).");
            return;
        }

        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        byte[] png = RoomFxBlockAnimationAtlasExtractor.Extract(rom);
        RoomFxBlockAnimationAtlas atlas = RoomFxBlockAnimationAtlasPng.Read(new MemoryStream(png));
        var guarded = NewRoomFxArtworkForbiddenBus(rom);
        int frameCount = 0;
        foreach (RoomFxBlockAnimationLoop definition in
                 RoomFxBlockAnimationLoops.All)
        {
            var state = new RoomFxBlockAnimationsState();
            state.LoadDefinition(definition.AnimatedObject);
            for (int frameIndex = 0; frameIndex < definition.Frames.Count; frameIndex++)
            {
                ushort cursor = definition.FrameInstructionPointer(frameIndex);
                int source = AnimatedTileImageSources.OfCursor(
                    definition, cursor);
                byte[] native = RomDataReader.ReadFixedBank(rom, source,
                    definition.PictureByteCount);
                AssertTrue(atlas.TryResolve(source, native.Length, out ReadOnlyMemory<byte> installed) &&
                    installed.Span.SequenceEqual(native),
                    $"room-FX frame $87:{cursor:X4} preserves every native art byte");
                int ticksUntilFrame = frameIndex == 0 ? 1 : definition.Frames[frameIndex - 1].Duration;
                // The frame's pass selects into fresh selections, so the selection read back is the one that pass made.
                for (int tick = 1; tick < ticksUntilFrame; tick++)
                    state.Step(new ArtworkFrameSelections());
                var frames = new ArtworkFrameSelections();
                state.Step(frames);
                AssertTrue(frames.TryShown(BlockAnimationFrames.Of(definition.AnimatedObject), out ArtworkFrame shown) &&
                        shown == definition.Frames[frameIndex].Frame,
                    $"room-FX frame $87:{cursor:X4} is the frame its animation shows from this update");
                AssertEqual(source, AnimatedTileImageSources.OfShown(frames, definition.AnimatedObject),
                    $"room-FX frame $87:{cursor:X4} selects its compiled art source");
                frameCount++;
            }
        }
        AssertEqual(30, frameCount, "all simple room-FX frames are covered");
        foreach (WreckedShipTreadmillDirection direction in
                 Enum.GetValues<WreckedShipTreadmillDirection>())
        {
            var treadmill = new WreckedShipTreadmillBlockAnimationsState();
            var waitFrames = new ArtworkFrameSelections();
            treadmill.Start(direction);
            treadmill.Step(areaBossDefeated: false, waitFrames);
            BlockAnimation treadmillObject = WreckedShipTreadmills.ForDirection(direction).AnimatedObject;
            AssertTrue(!waitFrames.TryShown(BlockAnimationFrames.Of(treadmillObject), out _),
                $"{direction} waits for Phantoon's area-boss bit before presenting art");
            for (int frame = 0; frame < RoomFxBlockAnimationAtlasFormat.TreadmillFrameCount;
                 frame++)
            {
                int nativeFrame = direction == WreckedShipTreadmillDirection.Rightwards
                    ? frame : 3 - frame;
                int source = AnimatedTileImageSources.Treadmill(nativeFrame);
                byte[] native = RomDataReader.ReadFixedBank(rom, source,
                    RoomFxBlockAnimationLoops.TreadmillFrameByteCount);
                AssertTrue(atlas.TryResolve(source, native.Length,
                        out ReadOnlyMemory<byte> installed) &&
                    installed.Span.SequenceEqual(native),
                    $"{direction} treadmill frame {frame} preserves every cartridge pixel byte");
                // Each pass selects into fresh selections, so the selection read back is the one this pass made.
                var treadmillFrames = new ArtworkFrameSelections();
                treadmill.Step(areaBossDefeated: true, treadmillFrames);
                AssertTrue(treadmillFrames.TryShown(BlockAnimationFrames.Of(treadmillObject), out ArtworkFrame shown) &&
                        shown == BlockAnimationFrames.Frame(treadmillObject, frame),
                    $"{direction} frame {frame} is the frame the treadmill shows from this update");
                AssertEqual(source, AnimatedTileImageSources.OfShown(treadmillFrames, treadmillObject),
                    $"{direction} frame {frame} selects the compiled artwork identity");
            }
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts, "installed animations never read bank-$87 ROM data");
        AssertThrows<InvalidDataException>(() => atlas.TryResolve(
            AnimatedTileImageSources.Of(new AnimatedTileImage(BlockAnimation.Lava, 0)), 1, out _),
            "artwork cannot change the native transfer byte count");
        AssertThrows<InvalidDataException>(() => RoomFxBlockAnimationAtlasPng.Read(
            new MemoryStream([1, 2, 3])), "corrupt room-FX PNG fails loudly");
        Console.WriteLine("  Room-FX animation artwork: 30 simple and four treadmill PNG frames match native bytes, each shown on its update.");
    }

    /// <summary>Wraps the runtime bus so installed animated-tile transfers cannot reread any bank-$87 artwork.</summary>
    internal static MutableCartridgeReadGuard NewRoomFxArtworkForbiddenBus(ISnesAddressSpace inner)
    {
        var guard = new MutableCartridgeReadGuard(inner);
        guard.Forbid(address => (SnesAddressMath.BankNumber(address)) == 0x87,
            address => $"Installed room-FX artwork read ROM ${address:X6}.");
        return guard;
    }
}
