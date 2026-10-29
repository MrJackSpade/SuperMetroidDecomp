using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Artwork;
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
            if (useRealLandingDoor) VerifyVisibleOverrides();

            void VerifyVisibleOverrides()
            {
                // Rooms draw their block sheet's pictures: its image where a row's live colours are the sheet's standard ones,
                // and its palette indices in the live colours elsewhere. A player's recolour overrides both files; here each
                // opaque pixel shows the inverse of its colour and its row's next colour.
                TilesetDefinition tileset = (native.Room.Scope ?? throw new InvalidOperationException("Verification requires a loaded room.")).Assets.Tileset;
                string sheetName = RoomBlockSheet.NameOf(tileset);
                RgbaPixel Inverted(RgbaPixel pixel) => pixel with { R = (byte)~pixel.R, G = (byte)~pixel.G, B = (byte)~pixel.B };
                RgbaPixel NextColour(RgbaPixel pixel) => pixel with { R = (byte)((pixel.R & ~15) | ((pixel.R & 15) % 15 + 1)) };
                string[] overridePaths = [.. new (string File, Func<RgbaPixel, RgbaPixel> Edit)[]
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
                    RgbaPixel[] changedFrame = FrontendScenePixelsAccess.Pixels(changed.PublishDisplayFrame());
                    AssertTrue(!nativeFrame.SequenceEqual(changedFrame),
                        "Landing Site block sheet override changes rendered pixels");
                    AssertShowsOverriddenPictures(changed, Inverted);
                    AssertGameplayPlanesUnchanged(changed, "block sheet");
                }
                finally
                {
                    foreach (string path in overridePaths)
                        File.Delete(path);
                }

                void WriteEdited(string file, Func<RgbaPixel, RgbaPixel> edit)
                {
                    RgbaPixel[] pixels = RgbaPng.Read(new MemoryStream(File.ReadAllBytes(Path.Combine(installation.RoomBlockSheetDirectory, file)),
                        writable: false), out Extent16<ImagePixels> size);
                    for (int index = 0; index < pixels.Length; index++)
                    {
                        if (pixels[index].A != 0)
                            pixels[index] = edit(pixels[index]);
                    }
                    using var edited = File.Create(Path.Combine(installation.RoomBlockSheetOverrideDirectory, file));
                    RgbaPng.Write(edited, size, pixels);
                }
                Console.WriteLine("  Landing Site: a block sheet edit loads, shows its repainted block pictures and alters rendered " +
                    "pixels without changing BG1 placement or BTS.");
            }

            // The room loads with the overridden sheet and every block BG1 shows draws its repainted picture (#1431): each
            // opaque pixel the stock picture of the same block paints, edited as the override edited it.
            void AssertShowsOverriddenPictures(SuperMetroidRuntime changed, Func<RgbaPixel, RgbaPixel> edit)
            {
                RoomBlockPictureTable stock = (native.Display.Latched.Rooms ?? throw new InvalidOperationException("The stock room shows no backgrounds."))
                    .Bg1.Pictures ?? throw new InvalidOperationException("The stock room's BG1 shows no block pictures.");
                RoomBlockPictureTable repainted = (changed.Display.Latched.Rooms ?? throw new InvalidOperationException("The edited room shows no backgrounds."))
                    .Bg1.Pictures ?? throw new InvalidOperationException("The edited room's BG1 shows no block pictures.");
                int compared = 0;
                for (int block = 0; block < stock.Blocks; block++)
                {
                    if (stock.Picture(block) is not { } before)
                        continue;
                    SpriteSheetFrame after = repainted.Picture(block)
                        ?? throw new InvalidOperationException($"The edited room shows block {block} with no picture.");
                    for (int y = 0; y < RoomBlockSheet.BlockPixels; y++)
                    for (int x = 0; x < RoomBlockSheet.BlockPixels; x++)
                    {
                        RgbaPixel stockPixel = before.Sheet.Pixels[(before.Corner.Y + y) * before.Sheet.Size.X + before.Corner.X + x];
                        if (stockPixel.A == 0)
                            continue;
                        RgbaPixel shown = after.Sheet.Pixels[(after.Corner.Y + y) * after.Sheet.Size.X + after.Corner.X + x];
                        AssertEqual(edit(stockPixel), shown, $"Landing Site block {block} shows its repainted picture at ({x}, {y})");
                        compared++;
                    }
                }
                AssertTrue(compared > 0, "Landing Site shows repainted block pictures");
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

                    runtime.Artwork.RoomBlockSheets = installation.LoadRoomBlockSheets();
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
                // blocks, which the override checks change.
                runtime.LoadCartridgeRoomThroughDoorForVerification(door, useRealLandingDoor ? new Position16<RoomPixels>(0, 5 * 256 - 224) : default);
                runtime.FrameBoundary.BeginAcceptedFrame(0);
                return runtime;
            }
        }
    }
}
