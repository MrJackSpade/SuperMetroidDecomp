using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Fx;

/// <summary>Retail checks for room-FX, treadmill, and Tourian statue animated-tile PNG frames.</summary>
internal static class RoomFxAnimatedTileArtworkVerification
{
    /// <summary>Checks retail room-FX and treadmill frames against native bytes and the frame each animation shows.</summary>
    internal static void VerifyRoomFxAnimatedTileArtwork()
    {
        if (!File.Exists("Super Metroid.smc"))
        {
            Console.WriteLine("  Room-FX animation artwork: retail comparison skipped (private ROM absent).");
            return;
        }

        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        byte[] png = RoomFxAnimatedTileAtlasExtractor.Extract(rom);
        RoomFxAnimatedTileAtlas atlas = RoomFxAnimatedTileAtlasPng.Read(new MemoryStream(png));
        var guarded = NewRoomFxArtworkForbiddenBus(rom);
        int frameCount = 0;
        foreach (RoomFxAnimatedTileLoop definition in
                 RoomFxAnimatedTileLoops.All)
        {
            var state = new RoomFxAnimatedTilesState();
            var frames = new ArtworkFrameSelections();
            state.LoadDefinition(definition.AnimatedObject);
            for (int frameIndex = 0; frameIndex < definition.Frames.Count; frameIndex++)
            {
                RoomFxAnimatedTileFrameDefinition frame = definition.Frames[frameIndex];
                int source = AnimatedTileImageSources.OfCursor(
                    definition, frame.InstructionPointer);
                byte[] native = RomDataReader.ReadFixedBank(rom, source,
                    definition.FrameByteCount);
                AssertTrue(atlas.TryResolve(source, native.Length, out ReadOnlyMemory<byte> installed) &&
                    installed.Span.SequenceEqual(native),
                    $"room-FX frame $87:{frame.InstructionPointer:X4} preserves every native art byte");
                int ticksUntilFrame = frameIndex == 0 ? 1 : definition.Frames[frameIndex - 1].Duration;
                for (int tick = 0; tick < ticksUntilFrame; tick++)
                    state.Step(frames);
                AssertEqual(source, AnimatedTileImageSources.Of(state.LastImage),
                    $"room-FX frame $87:{frame.InstructionPointer:X4} selects its compiled art source");
                AssertTrue(frames.TryShown(AnimatedTileFrames.Of(definition.AnimatedObject), out ArtworkFrame shown) &&
                        shown == AnimatedTileFrames.Frame(definition.AnimatedObject, frameIndex),
                    $"room-FX frame $87:{frame.InstructionPointer:X4} is the frame its animation shows from this update");
                frameCount++;
            }
        }
        AssertEqual(30, frameCount, "all simple room-FX frames are covered");
        foreach (WreckedShipTreadmillDirection direction in
                 Enum.GetValues<WreckedShipTreadmillDirection>())
        {
            var treadmill = new WreckedShipTreadmillAnimatedTilesState();
            var treadmillFrames = new ArtworkFrameSelections();
            treadmill.Start(direction);
            treadmill.Step(areaBossDefeated: false, treadmillFrames);
            AnimatedTileObject treadmillObject = WreckedShipTreadmills.ForDirection(direction).AnimatedObject;
            AssertTrue(!treadmillFrames.TryShown(AnimatedTileFrames.Of(treadmillObject), out _),
                $"{direction} waits for Phantoon's area-boss bit before presenting art");
            for (int frame = 0; frame < RoomFxAnimatedTileAtlasFormat.TreadmillFrameCount;
                 frame++)
            {
                int nativeFrame = direction == WreckedShipTreadmillDirection.Rightwards
                    ? frame : 3 - frame;
                int source = AnimatedTileImageSources.Treadmill(nativeFrame);
                byte[] native = RomDataReader.ReadFixedBank(rom, source,
                    RoomFxAnimatedTileLoops.TreadmillFrameByteCount);
                AssertTrue(atlas.TryResolve(source, native.Length,
                        out ReadOnlyMemory<byte> installed) &&
                    installed.Span.SequenceEqual(native),
                    $"{direction} treadmill frame {frame} preserves every cartridge pixel byte");
                treadmill.Step(areaBossDefeated: true, treadmillFrames);
                AssertEqual(source, AnimatedTileImageSources.Of(treadmill.LastImage),
                    $"{direction} frame {frame} selects the compiled artwork identity");
                AssertTrue(treadmillFrames.TryShown(AnimatedTileFrames.Of(treadmillObject), out ArtworkFrame shown) &&
                        shown == AnimatedTileFrames.Frame(treadmillObject, frame),
                    $"{direction} frame {frame} is the frame the treadmill shows from this update");
            }
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts, "installed animations never read bank-$87 ROM data");
        AssertThrows<InvalidDataException>(() => atlas.TryResolve(
            AnimatedTileImageSources.Of(new AnimatedTileImage(AnimatedTileObject.Lava, 0)), 1, out _),
            "artwork cannot change the native transfer byte count");
        AssertThrows<InvalidDataException>(() => RoomFxAnimatedTileAtlasPng.Read(
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
