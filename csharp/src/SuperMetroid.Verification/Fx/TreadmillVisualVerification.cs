using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Fx;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using System.Text.Json;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.TestSupport.VerificationOracles;
namespace SuperMetroid.Verification.Fx;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Doors;

/// <summary>Checks Wrecked Ship treadmill DMA frames through the floor characters each frame shows.</summary>
internal static class TreadmillVisualVerification
{
    /// <summary>Compares four cycling graphics frames with native transfer sources in powered treadmill rooms.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyTreadmillVisual()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        using var rooms = JsonDocument.Parse(File.ReadAllText("standalone-assets/maps/room-placements.json"));
        foreach (var placement in rooms.RootElement.EnumerateArray())
        {
            if (placement.GetProperty("Area").GetString() != "WreckedShip") continue;
            ushort pointer = Convert.ToUInt16((placement.GetProperty("RoomHeader").GetString() ?? throw new InvalidOperationException("Verification requires placement.GetProperty(\"RoomHeader\").GetString()."))[2..], 16);
            var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.System.SetBossBits(AreaId.WreckedShip, BossBits.AreaBoss);
            runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Get(pointer));
            var room = runtime.Room.LoadedRoom;
            ushort record = RoomFxDefinitions.SelectedRecordPointer(room.State.Fx, null);
            byte bits = record == 0 ? (byte)0 : RoomFxDefinitions.RecordAt(record).AnimatedTileBitset;
            Console.WriteLine($"Room {room.Identity} header={pointer:X4} FX={room.State.FxPointer:X4} animation bits={bits:X2} door treadmill={runtime.WreckedShipTreadmill.IsActive}");
            if ((bits & 12) == 0) continue;
            var images = new HashSet<string>();
            int[] sources = (bits & 4) != 0
                ? [WreckedShipTreadmillRomData.Frame0Source, WreckedShipTreadmillRomData.FrameSource(1),
                    WreckedShipTreadmillRomData.FrameSource(2), WreckedShipTreadmillRomData.FrameSource(3)]
                : [WreckedShipTreadmillRomData.FrameSource(3), WreckedShipTreadmillRomData.FrameSource(2),
                    WreckedShipTreadmillRomData.FrameSource(1), WreckedShipTreadmillRomData.Frame0Source];
            // The floor's frames are selected (#1406): the frame boundary shows the frame the previous update selected, which the
            // render places over the room's characters where the cartridge copies it.
            ArtworkAnimation treadmill = AnimatedTileFrames.Of((bits & 4) != 0
                ? AnimatedTileObject.WreckedShipTreadmillRightwards : AnimatedTileObject.WreckedShipTreadmillLeftwards);
            RoomFxAnimatedTileAtlas animatedTiles = runtime.Artwork.MapPresentation?.RoomFxAnimatedTiles
                ?? throw new InvalidOperationException("The runtime has no installed animated tiles.");
            for (int frame = 0; frame < 12; frame++)
            {
                ArtworkFrame? latched = runtime.Room.LoadedScope.ArtworkFrames.TryShown(treadmill, out ArtworkFrame selected) ? selected : null;
                runtime.StepFrame(0);
                byte[] shown = BgArtworkFixtures.Composed(runtime.Vram, runtime.BgArtwork);
                if (latched is { } latchedFrame)
                    AnimatedTileFrameArtwork.Bytes(latchedFrame.Artwork, animatedTiles).Span.CopyTo(
                        shown.AsSpan(AnimatedTileVramDestinations.WordOf(AnimatedTileObject.WreckedShipTreadmillRightwards) * 2));
                // The floor's characters as the frame shows them: the actual artwork, not merely a changing DMA
                // pointer or unrelated enemy animation.
                byte[] floor = shown.AsSpan(
                    AnimatedTileVramDestinations.WordOf(AnimatedTileObject.WreckedShipTreadmillRightwards) * 2,
                    WreckedShipTreadmillRomData.FrameByteCount).ToArray();
                images.Add(Convert.ToHexString(floor));
                if (frame > 0)
                {
                    int matched = Array.FindIndex(sources, source => floor.AsSpan().SequenceEqual(NativeFloorCharacters(bus, source)));
                    AssertTrue(floor.AsSpan().SequenceEqual(NativeFloorCharacters(bus, sources[(frame - 1) & 3])),
                        $"{room.Identity} native direction/cadence characters frame {frame}: expected native frame {(frame - 1) & 3}, " +
                        (matched < 0 ? "shown characters match no native frame" : $"shown characters are native frame {matched}"));
                }
            }
            AssertEqual(4, images.Count, $"{room.Identity} powered floor shows all four graphics frames");
        }
    }

    /// <summary>The floor's characters as the native transfer of <paramref name="source"/> leaves them.</summary>
    [PpuLayoutBoundary("exact hardware test: reads a native VRAM transfer")]
    private static byte[] NativeFloorCharacters(ISnesAddressSpace bus, int source)
    {
        var expected = new SnesVram();
        int destination = AnimatedTileVramDestinations.WordOf(AnimatedTileObject.WreckedShipTreadmillRightwards);
        ImportedVramOracle.ExecuteHardware(expected, bus, source, WreckedShipTreadmillRomData.FrameByteCount, destination);
        return expected.Bytes.Slice(destination * 2, WreckedShipTreadmillRomData.FrameByteCount).ToArray();
    }

    /// <summary>Runs the focused Wrecked Ship treadmill visual comparison.</summary>
    [VerificationCommand("--treadmill-visual")]
    internal static int TreadmillVisualCommand()
    {
        Suite(nameof(VerifyTreadmillVisual), () => VerifyTreadmillVisual());
        return 0;
    }
}
