using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Installation;

namespace SuperMetroid.Verification.Rooms;

/// <summary>Compares installed room artwork with cartridge pixels in Ceres and Landing Site.</summary>
internal static class RoomArtworkRenderParityVerification
{
    /// <summary>
    /// Compare final displayed pixels, not only decompressed bytes. Ceres exercises the
    /// overlapping full-size room-character/CRE transfers; Landing Site exercises the
    /// real door-selected sky page and ordinary room-art upload in one frame.
    /// </summary>
    internal static void VerifyRoomArtworkRenderParity(
        GameInstallation installation, CartridgeImportAddressSpace bus)
    {
        // The installed artwork is loaded once and bound by each installed-art runtime, which only reads it.
        RoomCharacterAtlases installedCharacters = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.RoomCharacters;
        InstalledRoomArtwork installedBlockSheets = installation.LoadRoomBlockSheets();
        foreach ((string name, RoomDefinition expectedRoom, bool useRealLandingDoor) in new[]
        {
            new VerifyRoomArtworkRenderParityCase("Ceres elevator overlap", RoomDefinitions.CeresElev, false),
            new VerifyRoomArtworkRenderParityCase("Landing Site sky", RoomDefinitions.LandingSite, true),
        })
        {
            SuperMetroidRuntime native = LoadRoom(installedArt: false);
            SuperMetroidRuntime installed = LoadRoom(installedArt: true);
            if (useRealLandingDoor)
            {
                // The door-selected sky page reaches the frame as the sky's picture (#1406); the frames compared below
                // show it, so here the room only has to have entered through the landing door with its sky running.
                ushort landingDoor = LandingSiteRomData.LandingCutsceneDoorPointer;
                AssertTrue(native.Room.ActiveDoor?.Pointer == landingDoor && native.Room.ScrollingSky is not null,
                    "Landing Site enters through its real landing door with its scrolling sky");
            }
            else
                AssertTrue(native.Artwork.RoomCharacterArt.Get((native.Room.Scope ?? throw new InvalidOperationException("Verification requires a loaded room.")).Assets.Tileset.Characters).Characters.Length >
                    RoomGraphicsToolingLayout.CommonRoomElementCharacters.Offset * sizeof(ushort),
                    "Ceres parity exercises room characters overlapping the CRE VRAM region");
            byte[] nativeComposed = native.ComposedVram();
            byte[] installedComposed = installed.ComposedVram();
            AssertTrue(nativeComposed.AsSpan().SequenceEqual(installedComposed),
                $"{name} installed artwork binds the same background characters");
            RgbaPixel[] nativeFrame = FrontendScenePixelsAccess.Pixels(native.PublishDisplayFrame());
            RgbaPixel[] installedFrame = FrontendScenePixelsAccess.Pixels(installed.PublishDisplayFrame());
            AssertTrue(nativeFrame.Length == installedFrame.Length &&
                nativeFrame.SequenceEqual(installedFrame),
                $"{name} installed artwork preserves every rendered pixel");
            if (nativeFrame.Distinct().Count() < 8)
                throw new InvalidDataException($"{name} parity compared an almost blank frame.");
            Console.WriteLine($"  {name}: {nativeFrame.Length} rendered pixels match installed art exactly.");

            SuperMetroidRuntime LoadRoom(bool installedArt)
            {
                var runtime = CreateRetailRuntimeFixture();
                if (installedArt)
                {
                    runtime.Artwork.RoomCharacterArt = installedCharacters;
                    runtime.Artwork.RoomBlockSheets = installedBlockSheets;
                }
                runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
                runtime.FrameBoundary.BeginAcceptedFrame(0);
                runtime.RoomLoader.InitializeStartingCeresRoom();
                runtime.RoomLoader.InitializeCeresStartSamus();
                ushort doorPointer = useRealLandingDoor
                    ? LandingSiteRomData.LandingCutsceneDoorPointer
                    : DoorPointers.ToCeresElevatorShaft;
                DoorDefinition door = SuperMetroid.AssetExtraction.Rooms.CartridgeDoorHeaderImporter.Load(bus, doorPointer);
                AssertEqual(expectedRoom, door.DestinationRoom,
                    $"{name} parity uses its real entry door");
                // The Landing Site opens on its sky, which its sky image draws; its bottom-left screen shows the ground's
                // blocks.
                runtime.LoadCartridgeRoomThroughDoorForVerification(door, useRealLandingDoor ? new Position16<RoomPixels>(0, 5 * 256 - 224) : default);
                runtime.FrameBoundary.BeginAcceptedFrame(0);
                return runtime;
            }
        }
    }
}
