using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;

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
                var sourceBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
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
                AssertTrue(native.Room.ActiveRoomAssets!.RoomCharacters.Length >
                    RoomGraphicsLayout.CommonRoomElementCharacters.Offset * sizeof(ushort),
                    "Ceres parity exercises room characters overlapping the CRE VRAM region");
            AssertTrue(native.Vram.ImageEquals(installed.Vram),
                $"{name} installed artwork preserves the complete displayed VRAM");
            Rgba32[] nativeFrame = SuperMetroidRuntimeFrameRenderer.Render(native);
            Rgba32[] installedFrame = SuperMetroidRuntimeFrameRenderer.Render(installed);
            AssertTrue(nativeFrame.Length == installedFrame.Length &&
                nativeFrame.AsSpan().SequenceEqual(installedFrame),
                $"{name} installed artwork preserves every rendered pixel");
            if (nativeFrame.Distinct().Count() < 8)
                throw new InvalidDataException($"{name} parity compared an almost blank frame.");
            Console.WriteLine($"  {name}: {nativeFrame.Length} rendered pixels match installed art exactly.");
            if (useRealLandingDoor) VerifyVisibleOverrides();

            void VerifyVisibleOverrides()
            {
                int characterSource = native.Room.ActiveRoomAssets!.Tileset.CharacterAddress;
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
                    Rgba32[] changedFrame = SuperMetroidRuntimeFrameRenderer.Render(changed);
                    AssertTrue(!nativeFrame.AsSpan().SequenceEqual(changedFrame),
                        "Landing Site character PNG override changes rendered pixels");
                    AssertGameplayPlanesUnchanged(changed, "character PNG");
                }
                finally { File.Delete(characterOverridePath); }

                string blockName = RoomMetatileFormat.CreFileName;
                string stockBlockPath = Path.Combine(installation.RoomMetatileDirectory, blockName);
                JsonNode blocks = JsonNode.Parse(File.ReadAllText(stockBlockPath))
                    ?? throw new InvalidDataException("Installed CRE visual block JSON is empty.");
                foreach (JsonNode? block in blocks["blocks"]!.AsArray())
                {
                    JsonNode cell = block!["topLeft"]!;
                    cell["tileColumn"] = (cell["tileColumn"]!.GetValue<int>() + 1)
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
                    Rgba32[] changedFrame = SuperMetroidRuntimeFrameRenderer.Render(changed);
                    AssertTrue(!nativeFrame.AsSpan().SequenceEqual(changedFrame),
                        "Landing Site visual block override changes rendered pixels");
                    AssertGameplayPlanesUnchanged(changed, "visual block JSON");
                }
                finally { File.Delete(blockOverridePath); }
                Console.WriteLine("  Landing Site: PNG and visual block edits both alter rendered pixels " +
                    "without changing BG1 placement or BTS.");
            }

            void AssertGameplayPlanesUnchanged(SuperMetroidRuntime changed, string artwork)
            {
                AssertTrue(native.Room.LevelData!.ForegroundEntries.Span.SequenceEqual(
                        changed.Room.LevelData!.ForegroundEntries.Span) &&
                    native.Room.LevelData.BehaviorBytes.Span.SequenceEqual(
                        changed.Room.LevelData.BehaviorBytes.Span),
                    $"{artwork} override does not change placement or collision BTS");
            }

            SuperMetroidRuntime LoadRoom(bool installedArt)
            {
                var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
                var runtime = CreateRetailRuntimeFixture(bus);
                if (installedArt)
                {
                    runtime.Artwork.RoomCharacterArt = installation.LoadRoomCharacters();
                    runtime.Artwork.RoomMetatileArt = installation.LoadRoomMetatiles();
                    runtime.Artwork.RoomBackgroundTilemapArt = installation.LoadRoomBackgroundTilemaps();
                    runtime.Artwork.RoomSkyTilemapArt = installation.LoadRoomSkyTilemaps();
                }
                runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
                runtime.NmiHandler.RunNmi(0, true);
                runtime.RoomLoader.InitializeStartingCeresRoom();
                runtime.RoomLoader.InitializeCeresStartSamus();
                ushort doorPointer = useRealLandingDoor
                    ? LandingSiteRomData.LandingCutsceneDoorPointer
                    : DoorPointers.ToCeresElevatorShaft;
                CartridgeDoorHeader door = SuperMetroid.AssetExtraction.CartridgeDoorHeaderImporter.Load(bus, doorPointer);
                AssertEqual(roomPointer, door.DestinationRoomPointer,
                    $"{name} parity uses its real entry door");
                runtime.LoadCartridgeRoomThroughDoorForVerification(door);
                runtime.NmiHandler.RunNmi(0, true);
                return runtime;
            }
        }
    }
}
