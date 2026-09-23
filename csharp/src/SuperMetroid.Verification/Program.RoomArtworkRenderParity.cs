using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// Compare final displayed pixels, not only decompressed bytes. Ceres exercises the
    /// overlapping full-size room-character/CRE transfers; Landing Site exercises the
    /// real door-selected sky page and ordinary room-art upload in one frame.
    /// </summary>
    private static void VerifyRoomArtworkRenderParity(
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
                var sourceBus = SuperMetroidAddressSpace.LoadRetailRom(sourceRom);
                LandingSiteEntryState entry = LandingSiteEntryState.LoadLandingCutscene(sourceBus);
                byte[] sky = RomDataReader.ReadFixedBank(sourceBus,
                    entry.SkySourceAddress, entry.SkyByteCount);
                AssertTrue(native.ActiveDoor?.Pointer == entry.DoorPointer &&
                    native.ScrollingSky is not null &&
                    native.Vram.Bytes.Slice(entry.SkyVramDestination * 2, sky.Length)
                        .SequenceEqual(sky),
                    "Landing Site frame contains its real door-selected sky page");
            }
            else
                AssertTrue(native.ActiveRoomAssets!.RoomCharacters.Length >
                    RoomAssetRomData.GraphicsLayout.CreCharactersVramByteOffset,
                    "Ceres parity exercises room characters overlapping the CRE VRAM region");
            AssertTrue(native.Vram.Bytes.SequenceEqual(installed.Vram.Bytes),
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
                int characterSource = native.ActiveRoomAssets!.Tileset.CharacterAddress;
                string characterName = RoomCharacterAtlasFormat.SourceFileName(characterSource);
                string stockCharacterPath = Path.Combine(installation.RoomCharacterDirectory,
                    characterName);
                int tileCount = native.ActiveRoomAssets.RoomCharacters.Length /
                    RoomCharacterAtlasFormat.BytesPerTile;
                int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
                int rows = (tileCount + columns - 1) / columns;
                IndexedPngImage characterImage;
                using (var stockPng = File.OpenRead(stockCharacterPath))
                    characterImage = IndexedPng.Read(stockPng, columns * 8, rows * 8);
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
                        IndexedPng.Write(editedPng, characterImage.Width, characterImage.Height,
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
                AssertTrue(native.LevelData!.ForegroundEntries.Span.SequenceEqual(
                        changed.LevelData!.ForegroundEntries.Span) &&
                    native.LevelData.BehaviorBytes.Span.SequenceEqual(
                        changed.LevelData.BehaviorBytes.Span),
                    $"{artwork} override does not change placement or collision BTS");
            }

            SuperMetroidRuntime LoadRoom(bool installedArt)
            {
                var bus = SuperMetroidAddressSpace.LoadRetailRom(sourceRom);
                var runtime = new SuperMetroidRuntime(bus);
                if (installedArt)
                {
                    runtime.RoomCharacterArt = installation.LoadRoomCharacters();
                    runtime.RoomMetatileArt = installation.LoadRoomMetatiles();
                    runtime.RoomBackgroundTilemapArt = installation.LoadRoomBackgroundTilemaps();
                    runtime.RoomSkyTilemapArt = installation.LoadRoomSkyTilemaps();
                }
                runtime.InitializeHud(HudSnapshot.CeresDebug);
                runtime.RunNmi(0, true);
                runtime.InitializeStartingCeresRoom();
                runtime.InitializeCeresStartSamus();
                ushort doorPointer = useRealLandingDoor
                    ? LandingSiteRomData.LandingCutsceneDoorPointer
                    : DoorPointers.ToCeresElevatorShaft;
                CartridgeDoorHeader door = CartridgeDoorHeader.Load(bus, doorPointer);
                AssertEqual(roomPointer, door.DestinationRoomPointer,
                    $"{name} parity uses its real entry door");
                runtime.LoadCartridgeRoomThroughDoorForVerification(door);
                runtime.RunNmi(0, true);
                return runtime;
            }
        }
    }
}
