using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
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
    [PpuLayoutBoundary("exact hardware test: checks native PPU geometry, native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for scrolling-sky transfer bytes compared with installed artwork")]
    internal static void VerifyRoomArtworkRenderParity(
        GameInstallation installation, string sourceRom)
    {
        foreach ((string name, RoomHeaderDefinition expectedRoom, bool useRealLandingDoor) in new[]
        {
            new VerifyRoomArtworkRenderParityCase("Ceres elevator overlap", RoomHeaderDefinitions.CeresElev, false),
            new VerifyRoomArtworkRenderParityCase("Landing Site sky", RoomHeaderDefinitions.LandingSite, true),
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
                AssertTrue((native.Room.Scope ?? throw new InvalidOperationException("Verification requires a loaded room.")).Assets.RoomCharacters.Length >
                    RoomGraphicsToolingLayout.CommonRoomElementCharacters.Offset * sizeof(ushort),
                    "Ceres parity exercises room characters overlapping the CRE VRAM region");
            AssertTrue(native.ComposedVram().AsSpan().SequenceEqual(installed.ComposedVram()),
                $"{name} installed artwork binds the same background characters");
            Rgba32[] nativeFrame = FrontendScenePixelsAccess.Pixels(native.PublishDisplayFrame());
            Rgba32[] installedFrame = FrontendScenePixelsAccess.Pixels(installed.PublishDisplayFrame());
            AssertTrue(nativeFrame.Length == installedFrame.Length &&
                nativeFrame.SequenceEqual(installedFrame),
                $"{name} installed artwork preserves every rendered pixel");
            if (nativeFrame.Distinct().Count() < 8)
                throw new InvalidDataException($"{name} parity compared an almost blank frame.");
            Console.WriteLine($"  {name}: {nativeFrame.Length} rendered pixels match installed art exactly.");
            if (useRealLandingDoor) VerifyVisibleOverrides();

            void VerifyVisibleOverrides()
            {
                // Rooms draw their block sheet's pictures: its image where a row's live colours are the sheet's standard ones,
                // and its palette indices in the live colours elsewhere. A player's recolour overrides both files; here each
                // opaque pixel shows the inverse of its colour and its row's next colour.
                TilesetDefinition tileset = (native.Room.Scope ?? throw new InvalidOperationException("Verification requires a loaded room.")).Assets.Tileset;
                string sheetName = RoomBlockSheet.NameOf(tileset);
                Rgba32 Inverted(Rgba32 pixel) => pixel with { R = (byte)~pixel.R, G = (byte)~pixel.G, B = (byte)~pixel.B };
                Rgba32 NextColour(Rgba32 pixel) => pixel with { R = (byte)((pixel.R & ~15) | ((pixel.R & 15) % 15 + 1)) };
                string[] overridePaths = [.. new (string File, Func<Rgba32, Rgba32> Edit)[]
                {
                    (SpriteSheetStore.ImageFile(sheetName), Inverted),
                    (SpriteSheetStore.PaletteFile(sheetName), NextColour),
                }.Select(edit => Path.Combine(installation.RoomBlockSheetOverrideDirectory, edit.File))];
                Directory.CreateDirectory(installation.RoomBlockSheetOverrideDirectory);
                if (overridePaths.Any(File.Exists))
                    throw new InvalidOperationException("Render parity fixture found an existing block sheet override.");
                try
                {
                    WriteEdited(SpriteSheetStore.ImageFile(sheetName), Inverted);
                    WriteEdited(SpriteSheetStore.PaletteFile(sheetName), NextColour);
                    SuperMetroidRuntime changed = LoadRoom(installedArt: true);
                    Rgba32[] changedFrame = FrontendScenePixelsAccess.Pixels(changed.PublishDisplayFrame());
                    AssertTrue(!nativeFrame.SequenceEqual(changedFrame),
                        "Landing Site block sheet override changes rendered pixels");
                    AssertGameplayPlanesUnchanged(changed, "block sheet");
                }
                finally
                {
                    foreach (string path in overridePaths)
                        File.Delete(path);
                }

                void WriteEdited(string file, Func<Rgba32, Rgba32> edit)
                {
                    Rgba32[] pixels = RgbaPng.Read(new MemoryStream(File.ReadAllBytes(Path.Combine(installation.RoomBlockSheetDirectory, file)),
                        writable: false), out Extent16<ImagePixels> size);
                    for (int index = 0; index < pixels.Length; index++)
                    {
                        if (pixels[index].A != 0)
                            pixels[index] = edit(pixels[index]);
                    }
                    using var edited = File.Create(Path.Combine(installation.RoomBlockSheetOverrideDirectory, file));
                    RgbaPng.Write(edited, size, pixels);
                }

                string blockName = RoomMetatileFormat.CreFileName;
                string stockBlockPath = Path.Combine(installation.RoomMetatileDirectory, blockName);
                JsonNode blocks = JsonNode.Parse(File.ReadAllText(stockBlockPath))
                    ?? throw new InvalidDataException("Installed CRE visual block JSON is empty.");
                foreach (JsonNode? block in blocks.Required("blocks").AsArray().RequiredElements())
                {
                    JsonNode cell = block.Required("topLeft");
                    cell["tileColumn"] = (cell.Required("tileColumn").GetValue<int>() + 1)
                        % RoomMetatileFormat.TileColumns;
                }
                Directory.CreateDirectory(installation.RoomMetatileOverrideDirectory);
                string blockOverridePath = Path.Combine(
                    installation.RoomMetatileOverrideDirectory, blockName);
                if (File.Exists(blockOverridePath))
                    throw new InvalidOperationException("Render parity fixture found an existing block override.");
                try
                {
                    File.WriteAllText(blockOverridePath, blocks.ToJsonString());
                    SuperMetroidRuntime changed = LoadRoom(installedArt: true);
                    Rgba32[] changedFrame = FrontendScenePixelsAccess.Pixels(changed.PublishDisplayFrame());
                    AssertTrue(!nativeFrame.SequenceEqual(changedFrame),
                        "Landing Site visual block override changes rendered pixels");
                    AssertGameplayPlanesUnchanged(changed, "visual block JSON");
                }
                finally { File.Delete(blockOverridePath); }
                Console.WriteLine("  Landing Site: block sheet and visual block edits both alter rendered pixels " +
                    "without changing BG1 placement or BTS.");
            }

            void AssertGameplayPlanesUnchanged(SuperMetroidRuntime changed, string artwork)
            {
                AssertTrue(native.Room.LoadedLevelData.ForegroundEntries.Span.SequenceEqual(
                        changed.Room.LoadedLevelData.ForegroundEntries.Span) &&
                    native.Room.LoadedLevelData.BehaviorBytes.AsSpan().SequenceEqual(
                        changed.Room.LoadedLevelData.BehaviorBytes.AsSpan()),
                    $"{artwork} override does not change placement or collision BTS");
            }

            SuperMetroidRuntime LoadRoom(bool installedArt)
            {
                var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
                var runtime = CreateRetailRuntimeFixture();
                if (installedArt)
                {
                    runtime.Artwork.RoomCharacterArt = installation.LoadRoomCharacters();
                    runtime.Artwork.RoomMetatileArt = installation.LoadRoomMetatiles();
                    runtime.Artwork.RoomBackgroundTilemapArt = installation.LoadRoomBackgroundTilemaps();
                    runtime.Artwork.RoomSkyTilemapArt = installation.LoadRoomSkyTilemaps();
                    runtime.Artwork.RoomBlockSheets = installation.LoadRoomBlockSheets();
                }
                runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
                runtime.FrameBoundary.BeginAcceptedFrame(0);
                runtime.RoomLoader.InitializeStartingCeresRoom();
                runtime.RoomLoader.InitializeCeresStartSamus();
                ushort doorPointer = useRealLandingDoor
                    ? LandingSiteRomData.LandingCutsceneDoorPointer
                    : DoorPointers.ToCeresElevatorShaft;
                CartridgeDoorHeader door = SuperMetroid.AssetExtraction.Rooms.CartridgeDoorHeaderImporter.Load(bus, doorPointer);
                AssertEqual(expectedRoom, door.DestinationRoom,
                    $"{name} parity uses its real entry door");
                // The Landing Site opens on its sky, which its sky image draws; its bottom-left screen shows the ground's
                // blocks, which the override checks change.
                runtime.LoadCartridgeRoomThroughDoorForVerification(door, useRealLandingDoor ? new Position16<RoomPixels>(0, 5 * 256 - 224) : default);
                runtime.FrameBoundary.BeginAcceptedFrame(0);
                return runtime;
            }
        }
    }
}
