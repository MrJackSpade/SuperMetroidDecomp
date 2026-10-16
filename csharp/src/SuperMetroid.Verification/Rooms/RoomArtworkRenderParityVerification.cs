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
        foreach ((string name, ushort roomPointer, bool useRealLandingDoor) in new[]
        {
            ("Ceres elevator overlap", RoomHeaderPointers.CeresElevatorShaft, false),
            ("Landing Site sky", RoomHeaderPointers.LandingSite, true),
        })
        {
            SuperMetroidRuntime native = LoadRoom(installedArt: false);
            SuperMetroidRuntime installed = LoadRoom(installedArt: true);
            if (useRealLandingDoor)
            {
                var sourceBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
                ushort landingDoor = LandingSiteRomData.LandingCutsceneDoorPointer;
                LibraryBackgroundInstruction skyTransfer = LibraryBackgroundProgramDefinitions.GetDoorTransfer(
                    unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress), landingDoor);
                byte[] sky = RomDataReader.ReadFixedBank(sourceBus,
                    skyTransfer.SourceAddress, skyTransfer.ByteCount);
                AssertTrue(native.Room.ActiveDoor?.Pointer == landingDoor &&
                    native.Room.ScrollingSky is not null &&
                    native.Vram.Bytes.Slice(skyTransfer.Destination * 2, sky.Length)
                        .SequenceEqual(sky),
                    "Landing Site frame contains its real door-selected sky page");
            }
            else
                AssertTrue((native.Room.ActiveRoomAssets ?? throw new InvalidOperationException("Verification requires native.Room.ActiveRoomAssets.")).RoomCharacters.Length >
                    RoomGraphicsLayout.CommonRoomElementCharacters.Offset * sizeof(ushort),
                    "Ceres parity exercises room characters overlapping the CRE VRAM region");
            AssertTrue(native.Vram.ImageEquals(installed.Vram),
                $"{name} installed artwork preserves the complete displayed VRAM");
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
                int characterSource = (native.Room.ActiveRoomAssets ?? throw new InvalidOperationException("Verification requires native.Room.ActiveRoomAssets.")).Tileset.CharacterAddress;
                string characterName = RoomCharacterAtlasFormat.SourceFileName(characterSource);
                string stockCharacterPath = Path.Combine(installation.RoomCharacterDirectory,
                    characterName);
                int tileCount = native.Room.ActiveRoomAssets.RoomCharacters.Length /
                    RoomCharacterAtlasFormat.BytesPerTile;
                int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
                int rows = (tileCount + columns - 1) / columns;
                IndexedPngImage characterImage;
                using (var stockPng = File.OpenRead(stockCharacterPath))
                    characterImage = IndexedPng.Read(stockPng, new(checked((ushort)(columns * 8)), checked((ushort)(rows * 8))));
                for (int index = 0; index < characterImage.Pixels.Length; index++)
                    characterImage.Pixels[index] = (byte)((characterImage.Pixels[index] + 1) & 15);
                Directory.CreateDirectory(installation.RoomCharacterOverrideDirectory);
                string characterOverridePath = Path.Combine(
                    installation.RoomCharacterOverrideDirectory, characterName);
                if (File.Exists(characterOverridePath))
                    throw new InvalidOperationException("Render parity fixture found an existing character override.");
                try
                {
                    using (var editedPng = File.Create(characterOverridePath))
                        IndexedPng.Write(editedPng, new(characterImage.Size.X, characterImage.Size.Y),
                            characterImage.Pixels, characterImage.Palette);
                    SuperMetroidRuntime changed = LoadRoom(installedArt: true);
                    Rgba32[] changedFrame = FrontendScenePixelsAccess.Pixels(changed.PublishDisplayFrame());
                    AssertTrue(!nativeFrame.SequenceEqual(changedFrame),
                        "Landing Site character PNG override changes rendered pixels");
                    AssertGameplayPlanesUnchanged(changed, "character PNG");
                }
                finally { File.Delete(characterOverridePath); }

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
                Console.WriteLine("  Landing Site: PNG and visual block edits both alter rendered pixels " +
                    "without changing BG1 placement or BTS.");
            }

            void AssertGameplayPlanesUnchanged(SuperMetroidRuntime changed, string artwork)
            {
                AssertTrue(native.Room.LoadedLevelData.ForegroundEntries.Span.SequenceEqual(
                        changed.Room.LoadedLevelData.ForegroundEntries.Span) &&
                    native.Room.LoadedLevelData.BehaviorBytes.Span.SequenceEqual(
                        changed.Room.LoadedLevelData.BehaviorBytes.Span),
                    $"{artwork} override does not change placement or collision BTS");
            }

            SuperMetroidRuntime LoadRoom(bool installedArt)
            {
                var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
                var runtime = CreateRetailRuntimeFixture(bus);
                if (installedArt)
                {
                    runtime.Artwork.RoomCharacterArt = installation.LoadRoomCharacters();
                    runtime.Artwork.RoomMetatileArt = installation.LoadRoomMetatiles();
                    runtime.Artwork.RoomBackgroundTilemapArt = installation.LoadRoomBackgroundTilemaps();
                    runtime.Artwork.RoomSkyTilemapArt = installation.LoadRoomSkyTilemaps();
                }
                runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
                runtime.FrameBoundary.BeginAcceptedFrame(0);
                runtime.RoomLoader.InitializeStartingCeresRoom();
                runtime.RoomLoader.InitializeCeresStartSamus();
                ushort doorPointer = useRealLandingDoor
                    ? LandingSiteRomData.LandingCutsceneDoorPointer
                    : DoorPointers.ToCeresElevatorShaft;
                CartridgeDoorHeader door = SuperMetroid.AssetExtraction.Rooms.CartridgeDoorHeaderImporter.Load(bus, doorPointer);
                AssertEqual(roomPointer, door.DestinationRoomPointer,
                    $"{name} parity uses its real entry door");
                runtime.LoadCartridgeRoomThroughDoorForVerification(door);
                runtime.FrameBoundary.BeginAcceptedFrame(0);
                return runtime;
            }
        }
    }
}
