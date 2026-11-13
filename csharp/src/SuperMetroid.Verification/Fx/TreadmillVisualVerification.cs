using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Fx;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using System.Text.Json;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Fx;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Doors;

/// <summary>Checks Wrecked Ship treadmill DMA frames through the floor characters each frame shows.</summary>
internal static class TreadmillVisualVerification
{
    /// <summary>Compares four cycling graphics frames with native transfer sources in powered treadmill rooms.</summary>
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
            AnimatedTileObject[] tiles = record == 0 ? [] : [.. RoomFxDefinitions.RecordAt(record).AnimatedTiles];
            Console.WriteLine($"Room {room.Identity} header={pointer:X4} FX={room.State.FxPointer:X4} animated tiles={string.Join(", ", tiles)} door treadmill={runtime.WreckedShipTreadmill.IsActive}");
            bool rightwards = tiles.Contains(AnimatedTileObject.WreckedShipTreadmillRightwards);
            if (!rightwards && !tiles.Contains(AnimatedTileObject.WreckedShipTreadmillLeftwards)) continue;
            var images = new HashSet<string>();
            int[] sources = rightwards
                ? [AnimatedTileImageSources.Treadmill(0), AnimatedTileImageSources.Treadmill(1),
                    AnimatedTileImageSources.Treadmill(2), AnimatedTileImageSources.Treadmill(3)]
                : [AnimatedTileImageSources.Treadmill(3), AnimatedTileImageSources.Treadmill(2),
                    AnimatedTileImageSources.Treadmill(1), AnimatedTileImageSources.Treadmill(0)];
            // The floor's frames are selected (#1406): the frame boundary shows the frame the previous update selected, which the
            // render places over the room's characters where the cartridge copies it.
            ArtworkAnimation treadmill = AnimatedTileFrames.Of(rightwards
                ? AnimatedTileObject.WreckedShipTreadmillRightwards : AnimatedTileObject.WreckedShipTreadmillLeftwards);
            // The animated tiles' characters are import data no installation holds: the cartridge-derived extraction's (#1636).
            RoomFxAnimatedTileAtlas animatedTiles = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.ImportMaps.RoomFxAnimatedTiles;
            for (int frame = 0; frame < 12; frame++)
            {
                ArtworkFrame? latched = runtime.Room.LoadedScope.ArtworkFrames.TryShown(treadmill, out ArtworkFrame selected) ? selected : null;
                runtime.StepFrame(0);
                byte[] shown = BgArtworkFixtures.Composed(runtime.Vram, runtime.BgArtwork, runtime.BackgroundArtwork());
                if (latched is { } latchedFrame)
                    AnimatedTileFrameArtwork.Bytes(BgArtworkIdentity.Frame(latchedFrame), animatedTiles).Span.CopyTo(
                        shown.AsSpan(AnimatedTileVramDestinations.WordOf(AnimatedTileObject.WreckedShipTreadmillRightwards) * 2));
                // The floor's characters as the frame shows them: the actual artwork, not merely a changing DMA
                // pointer or unrelated enemy animation.
                byte[] floor = shown.AsSpan(
                    AnimatedTileVramDestinations.WordOf(AnimatedTileObject.WreckedShipTreadmillRightwards) * 2,
                    RoomFxAnimatedTileLoops.TreadmillFrameByteCount).ToArray();
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
    private static byte[] NativeFloorCharacters(ISnesAddressSpace bus, int source)
    {
        var expected = new SnesVram();
        ushort destination = AnimatedTileVramDestinations.WordOf(AnimatedTileObject.WreckedShipTreadmillRightwards);
        ImportedVramOracle.ExecuteHardware(expected, bus, source, RoomFxAnimatedTileLoops.TreadmillFrameByteCount, destination);
        return expected.Bytes.Slice(destination * 2, RoomFxAnimatedTileLoops.TreadmillFrameByteCount).ToArray();
    }

    /// <summary>Runs the focused Wrecked Ship treadmill visual comparison.</summary>
    [VerificationCommand("--treadmill-visual")]
    internal static int TreadmillVisualCommand()
    {
        Suite(nameof(VerifyTreadmillVisual), () => VerifyTreadmillVisual());
        return 0;
    }
}
