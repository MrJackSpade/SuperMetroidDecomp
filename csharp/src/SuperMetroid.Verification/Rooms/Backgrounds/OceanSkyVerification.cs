using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Rooms.Backgrounds;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Installation;

/// <summary>Checks West Ocean's published scrolling-sky rows against ROM.</summary>
internal static class OceanSkyVerification
{
    /// <summary>Checks the published sky rows across camera positions against the ocean chunks.</summary>
    internal static void VerifyOceanSky()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.WestOcean, camera: new Position16<RoomPixels>(128, 768));
        // Capture consumes the accepted NMI's register snapshot, not the room's mutable scroll state.
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var scene = GameplayDisplayCapture.CaptureOrdinaryBase(runtime.PublishDisplayFrame());
        var ordinary = (GameplaySceneLayer)scene.Layers[0];
        Console.WriteLine($"Ocean room: main={runtime.Room.LoadedRoom.State.MainCallback}, BG2={ordinary.View.BackgroundMapSize.X}x{ordinary.View.BackgroundMapSize.Y}, sky={runtime.Room.ScrollingSky is not null}");

        ScrollingSkyState sky = runtime.Room.ScrollingSky ?? throw new InvalidOperationException("Ocean main installs the scrolling sky.");
        AssertEqual(ScrollingSkyChunkTable.Ocean, sky.Variant, "the ocean main reads the ocean chunk table, not the land table");
        AssertTrue(runtime.PublishDisplayFrame().FrameEnd.ScrollingSky is not null, "The ocean frame publishes its sky layer.");
        // The installed sky pages the import pictured; the runtime holds only their picture (#1466).
        SkyTilemap shown = SuperMetroid.TestSupport.RepositoryInstallation.RepositoryInstallation.Installation.LoadRoomSkyTilemaps()
            .Tilemap(sky.Variant);
        AssertTrue(ordinary.Rooms.Sky is { } drawn && drawn.RowsShown.SequenceEqual(Enumerable.Range(0, SkyTilemap.Rows).Select(shown.RowShown)),
            "the ocean's BG2 shows the published sky rows");
        // Every row the camera can show, at each tile-row step down the room, is the ocean ROM chunk's row.
        for (ushort cameraY = 0; cameraY <= 768; cameraY += 8)
        {
            for (int line = 32; line < 224; line += 8)
            {
                int worldY = cameraY + line;
                int pointerAddress = 0x88ADA6 + RoomScreenOf.Pixel(worldY) * 2;
                int pointer = bus.ReadCartridgeWord(pointerAddress);
                for (int column = 0; column < 32; column++)
                {
                    int source = 0x8A0000 | (SnesAddressMath.OffsetInBank(pointer + (worldY & 0xF8) * 8 + column * 2));
                    ushort expected = bus.ReadCartridgeWord(source);
                    int row = worldY % (SkyTilemap.Rows * 8) / 8;
                    AssertTrue(shown.RowShown(row) && shown.Tiles[row * SkyTilemap.Columns + column].PackedWord == expected,
                        $"ocean visible row {worldY:X4}, column {column} follows ROM ocean chunk");
                }
            }
        }
        Console.WriteLine("  Ocean sky: the published rows across 97 camera positions match the ocean ROM chunks.");
    }

    /// <summary>Runs the focused West Ocean sky rows check.</summary>
    [VerificationCommand("--ocean-sky")]
    internal static int OceanSkyCommand()
    {
        Suite(nameof(VerifyOceanSky), () => VerifyOceanSky());
        return 0;
    }
}
