using SuperMetroid.AssetExtraction;
using SuperMetroid.Tooling;
using System.Buffers.Binary;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Verification.Rooms;

/// <summary>Checks the installed room terrain and appearances against the cartridge's level words, decoded by the import's codec.</summary>
internal static class RoomVisualLayoutsVerification
{
    /// <summary>
    /// Checks every installed level's terrain, behaviours, background and terrain beyond the room, and every stock
    /// appearance, against the cartridge's level words decoded by the import's codec; then proves an installed override
    /// changes both rendered planes without changing terrain or behaviours.
    /// </summary>
    internal static void VerifyRoomVisualLayouts(GameInstallation installed,
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus, SelectedRoom landing)
    {
        RoomLevelArtworks stock = installed.LoadRoomVisualLayouts();
        AssertEqual(246, RoomVisualLayoutFiles.RetailLevels.Count,
            "distinct pinned room levels");
        InstalledRoomLevelStreams levelStreams = installed.LoadRoomLevelStreams();
        AssertEqual(RoomVisualLayoutFiles.RetailLevels.Count, levelStreams.Count,
            "installed room-level count");
        AssertTrue(!Directory.Exists(Path.Combine(installed.Root, "overrides",
                GameInstallationLayout.RoomLevelStreamDirectoryName)),
            "installed room-level streams have no override directory");
        VerifyRoomLevelStreamRejections(installed);
        try
        {
            _ = levelStreams.Get(new RoomLevelId("UnknownLevel"));
            throw new InvalidOperationException("Unknown installed room level was accepted.");
        }
        catch (InvalidDataException error)
        {
            AssertTrue(error.Message.Contains("lacks level", StringComparison.Ordinal),
                "unknown installed room level fails without ROM fallback");
        }
        foreach ((RoomLevelId level, int width) in RoomVisualLayoutFiles.RetailLevels)
        {
            RoomLevelArtwork layout = stock.Get(level);
            InstalledRoomLevel terrain = levelStreams.Get(level);
            byte[] native = RomDataReader.Decompress(bus, RoomLevelSourceDefinitions.Addresses[level]);
            int layerBytes = BinaryPrimitives.ReadUInt16LittleEndian(native);
            int count = layerBytes / 2;
            int backgroundOffset = 2 + layerBytes + count;
            int backgroundBytes = Math.Min(layerBytes, native.Length - backgroundOffset);
            AssertEqual(width, layout.Size.X, $"room level {level} visual stride");
            AssertEqual(count / width, layout.Size.Y,
                $"room level {level} visual allocation height");
            AssertEqual(layout.Size, terrain.Size, $"room level {level} terrain grid size");
            for (int index = 0; index < count; index++)
            {
                ushort foreground = BinaryPrimitives.ReadUInt16LittleEndian(native.AsSpan(2 + index * 2));
                RoomBlockAppearance background = RoomLevelWordCodec.DecodeAppearance(index * 2 < backgroundBytes
                    ? BinaryPrimitives.ReadUInt16LittleEndian(native.AsSpan(backgroundOffset + index * 2))
                    : (ushort)0);
                AssertEqual(RoomLevelWordCodec.Decode(foreground), terrain.Foreground[index],
                    $"room level {level} installed terrain {index}");
                AssertEqual(new RoomBlockBehavior(native[2 + layerBytes + index]), terrain.Behaviors[index],
                    $"room level {level} installed behaviour {index}");
                AssertEqual(background, terrain.Background[index],
                    $"room level {level} installed background {index}");
                AssertEqual(RoomLevelWordCodec.DecodeAppearance(foreground), layout.Foreground.Span[index],
                    $"room level {level} foreground appearance {index}");
                AssertEqual(background, layout.Background.Span[index],
                    $"room level {level} background appearance {index}");
            }

            // Past the grid: every level word the decompressed level reaches, the last possibly half the fill,
            // then the fill to the end of the cleared level space; the fill decodes to the game's edge-solid block.
            int payloadWords = (native.Length - 2 + 1) / 2;
            AssertEqual(Math.Max(0, payloadWords - count), terrain.Following.Length,
                $"room level {level} data following the grid");
            AssertEqual(Math.Max(LevelSpaceWords, payloadWords) - count, terrain.BeyondCount,
                $"room level {level} terrain beyond the grid");
            for (int index = 0; index < terrain.Following.Length; index++)
            {
                int offset = 2 + (count + index) * 2;
                ushort word = offset + 1 < native.Length
                    ? BinaryPrimitives.ReadUInt16LittleEndian(native.AsSpan(offset))
                    : (ushort)(native[offset] | (FillWord & 0xff00));
                AssertEqual(RoomLevelWordCodec.Decode(word), terrain.Following[index],
                    $"room level {level} block {count + index} past the grid");
            }
            AssertEqual(RoomTerrainBlocks.EdgeSolid, RoomLevelWordCodec.Decode(FillWord),
                "the level-space fill is the game's edge-solid block");
        }

        // Exercise the real room loader for every selectable state, not merely
        // representative addresses. Each state's compressed level source is
        // forbidden, so an accidental ROM fallback fails at its first read.
        RoomStaticPalettes palettes = installed.LoadRoomPalettes();
        int stateCount = 0;
        int slopeBlocks = 0;
        int spikeBlocks = 0;
        foreach (RoomHeaderDefinition header in RoomHeaderDefinitions.All)
        {
            SelectedRoom room = SelectedRoom.LoadUsingCompiledSelection(header);
            foreach (ushort statePointer in RoomStateSelection.GetStatePointers(header))
            {
                room = room with { State = RoomStateDefinitions.Get(statePointer) };
                int source = RoomLevelSourceDefinitions.Addresses[room.State.Level];
                byte[] native = RomDataReader.Decompress(bus, source);
                int layerBytes = BinaryPrimitives.ReadUInt16LittleEndian(native);
                int blockCount = layerBytes / 2;
                int bg2Offset = 2 + layerBytes + blockCount;
                int availableBg2 = Math.Min(layerBytes, native.Length - bg2Offset);
                RoomLevelData loaded = LoadFixtureRoomAssets(room,
                    paletteArt: palettes,
                    visualLayouts: stock, levelStreams: levelStreams).LevelData;
                AssertEqual(blockCount, loaded.Size.X * loaded.Size.Y,
                    $"room ${header.Pointer:X4} state ${statePointer:X4} physical allocation");
                for (int blockIndex = 0; blockIndex < blockCount; blockIndex++)
                {
                    RoomCollisionBlock block = loaded.GetCollisionBlockByIndex(blockIndex);
                    ushort nativeWord = BinaryPrimitives.ReadUInt16LittleEndian(
                        native.AsSpan(2 + blockIndex * 2));
                    byte nativeBts = native[2 + layerBytes + blockIndex];
                    ushort nativeBg2Word = blockIndex * 2 < availableBg2
                        ? BinaryPrimitives.ReadUInt16LittleEndian(
                            native.AsSpan(bg2Offset + blockIndex * 2))
                        : FillWord;
                    AssertEqual(RoomLevelWordCodec.Decode(nativeWord), block.Terrain,
                        $"room ${header.Pointer:X4} state ${statePointer:X4} terrain {blockIndex}");
                    AssertEqual(new RoomBlockBehavior(nativeBts), block.Behavior,
                        $"room ${header.Pointer:X4} state ${statePointer:X4} behaviour {blockIndex}");
                    AssertEqual(RoomLevelWordCodec.DecodeAppearance(nativeBg2Word), loaded.BackgroundPlane.Blocks[blockIndex],
                        $"room ${header.Pointer:X4} state ${statePointer:X4} background {blockIndex}");
                    if (block.CollisionType == RoomCollisionType.Slope) slopeBlocks++;
                    if (block.CollisionType is RoomCollisionType.SpikeAir or
                        RoomCollisionType.SpikeBlock) spikeBlocks++;
                }
                stateCount++;
            }
        }
        AssertEqual(RoomStateDefinitions.RetailStateCount, stateCount,
            "all retail room-state loads use the guarded installed level streams");
        AssertTrue(slopeBlocks > 0 && spikeBlocks > 0,
            "guarded installed-stream audit includes physical slope and spike-hazard blocks");

        RoomLevelId landingLevel = landing.State.Level;
        string name = RoomVisualLayoutFiles.LevelFileName(landingLevel);
        string stockPath = Path.Combine(installed.RoomVisualLayoutDirectory, name);
        JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
            ?? throw new InvalidDataException("Installed Landing Site layout is empty.");
        JsonObject foregroundPlane = document.Required("foreground").AsObject();
        JsonObject backgroundPlane = document.Required("background").AsObject();
        JsonArray foregroundBlocks = foregroundPlane.Required("blocks").AsArray();
        foregroundPlane["mirroring"] = MirrorFirstHorizontally(foregroundPlane.Required("mirroring").GetValue<string>());
        backgroundPlane["mirroring"] = MirrorFirstHorizontally(backgroundPlane.Required("mirroring").GetValue<string>());
        Directory.CreateDirectory(installed.RoomVisualLayoutOverrideDirectory);
        string overridePath = Path.Combine(installed.RoomVisualLayoutOverrideDirectory, name);
        File.WriteAllText(overridePath, document.ToJsonString());

        LoadedRoomAssets nativeRoom = LoadFixtureRoomAssets(landing);
        LoadedRoomAssets stockRoom = LoadFixtureRoomAssets(landing,
            visualLayouts: stock, levelStreams: levelStreams);
        LoadedRoomAssets editedRoom = LoadFixtureRoomAssets(landing,
            visualLayouts: installed.LoadRoomVisualLayouts(), levelStreams: levelStreams);
        AssertTrue(nativeRoom.LevelData.SameTerrainAs(editedRoom.LevelData),
            "an appearance override leaves the room's terrain and behaviours unchanged");

        // The renderers resolve each block from the level planes (#1346), so the shown first blocks, each a whole block and
        // its mirroring (#1431), are the planes'.
        RoomBlockCell nativeFirst = nativeRoom.LevelData.ForegroundPlane.Cells.Cells[0];
        AssertEqual(nativeFirst, stockRoom.LevelData.ForegroundPlane.Cells.Cells[0],
            "stock installed BG1 layout retains the native first block and mirroring");
        AssertTrue(nativeFirst != editedRoom.LevelData.ForegroundPlane.Cells.Cells[0],
            "edited BG1 layout changes the live renderer's first block");

        RoomBlockCell nativeBg2 = nativeRoom.LevelData.BackgroundPlane.Cells.Cells[0];
        AssertEqual(nativeBg2, stockRoom.LevelData.BackgroundPlane.Cells.Cells[0],
            "stock installed BG2 layout retains the native first block and mirroring");
        AssertTrue(nativeBg2 != editedRoom.LevelData.BackgroundPlane.Cells.Cells[0],
            "edited BG2 layout changes the live renderer's first block");

        // Ceres has no authored BG2 tail, while the Wrecked Ship allocation includes
        // an extra physical row beyond its camera. Both must survive the installed-stream path.
        foreach (ushort pointer in new ushort[] { 0xdf8d, 0xc98e })
        {
            SelectedRoom room = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, pointer);
            LoadedRoomAssets nativeOther = LoadFixtureRoomAssets(room);
            LoadedRoomAssets installedOther = LoadFixtureRoomAssets(room,
                visualLayouts: stock, levelStreams: levelStreams);
            AssertTrue(nativeOther.LevelData.SameTerrainAs(installedOther.LevelData) &&
                nativeOther.LevelData.BackgroundPlane.Blocks.SequenceEqual(installedOther.LevelData.BackgroundPlane.Blocks),
                $"installed room ${pointer:X4} retains its terrain, behaviours, background and level height");
        }

        // Existing user content is outside the replaceable stock installation. Reimport
        // must retain the selected appearance while the terrain remains unchanged.
        RoomBlockAppearance editedFirst = editedRoom.LevelData.ForegroundPlane.Blocks[0];
        _ = GameAssetInstaller.EnsureInstalled(installed.Root)
            ?? throw new InvalidOperationException("Room-layout repair lost the installation.");
        AssertEqual(editedFirst,
            installed.LoadRoomVisualLayouts().Get(landingLevel).Foreground.Span[0],
            "room-layout override survives stock installation validation");

        int editedForegroundBlock = foregroundBlocks.Required(0).GetValue<int>();
        foregroundBlocks[0] = RoomBlockAppearance.LastBlock + 1;
        File.WriteAllText(overridePath, document.ToJsonString());
        try
        {
            _ = installed.LoadRoomVisualLayouts();
            throw new InvalidOperationException("A block outside the tileset's set was accepted as an appearance.");
        }
        catch (InvalidDataException error)
        {
            AssertTrue(error.Message.Contains(name, StringComparison.Ordinal),
                "invalid visual-layout override identifies its file");
        }
        foregroundBlocks[0] = editedForegroundBlock;
        File.WriteAllText(overridePath, document.ToJsonString());
        Console.WriteLine($"  Room layouts: {RoomVisualLayoutFiles.RetailLevels.Count} levels decode to the installed terrain and " +
            $"{stateCount} guarded state loads keep it (including slopes and spikes) while appearance edits " +
            "affect only rendering, survive repair, and reject blocks outside the tileset's set.");
    }

    /// <summary>The cartridge's level-space fill word, which the import decodes as the game's edge-solid block.</summary>
    private const ushort FillWord = 0x8000;

    /// <summary>The level words the cartridge fills before decompressing a room.</summary>
    private const int LevelSpaceWords = 0x3200;

    /// <summary>A layout plane's mirroring codes with the first block's horizontal mirroring toggled.</summary>
    private static string MirrorFirstHorizontally(string mirroring) => mirroring[0] switch
    {
        '-' => "H",
        'H' => "-",
        'V' => "B",
        'B' => "V",
        _ => throw new InvalidDataException($"Unknown mirroring code '{mirroring[0]}'."),
    } + mirroring[1..];

    /// <summary>
    /// Confirms the stock-only level-stream loader rejects each corrupted installation with a
    /// path-specific error: a changed or resized stream, a missing file, an unsupported version
    /// or provenance, and an unknown or missing level.
    /// </summary>
    private static void VerifyRoomLevelStreamRejections(GameInstallation installed)
    {
        using var scratch = new TestScratchDirectory("room-level-stream-rejections");
        string firstName = RoomLevelStreamFiles.LevelFileName(
            RoomVisualLayoutFiles.RetailLevels.Keys.OrderBy(candidate => candidate.Name, StringComparer.Ordinal).First());
        const string unknownName = "level-UnknownLevel.bin";

        void Expect(string label, string namedFile, Action<string> corrupt)
        {
            string directory = Path.Combine(scratch.Root, label);
            Directory.CreateDirectory(directory);
            foreach (string file in Directory.GetFiles(installed.RoomLevelStreamDirectory))
                File.Copy(file, Path.Combine(directory, Path.GetFileName(file)));
            _ = RoomLevelStreamFiles.Load(directory);
            corrupt(directory);
            try
            {
                _ = RoomLevelStreamFiles.Load(directory);
                throw new InvalidOperationException($"Room-level streams accepted a {label} installation.");
            }
            catch (InvalidDataException error)
            {
                AssertTrue(error.Message.Contains(Path.Combine(directory, namedFile), StringComparison.Ordinal),
                    $"room-level stream {label} is rejected naming {namedFile}");
            }
        }

        static void EditManifest(string directory, Action<JsonObject> edit)
        {
            string path = Path.Combine(directory, RoomLevelStreamFiles.ManifestFileName);
            JsonObject manifest = RequiredJson.Parse(File.ReadAllText(path)).AsObject();
            edit(manifest);
            File.WriteAllText(path, manifest.ToJsonString());
        }

        Expect("hash-mismatch", firstName, directory =>
        {
            string path = Path.Combine(directory, firstName);
            byte[] stream = File.ReadAllBytes(path);
            stream[^1] ^= 1;
            File.WriteAllBytes(path, stream);
        });
        Expect("length-mismatch", firstName, directory =>
        {
            using var file = new FileStream(Path.Combine(directory, firstName), FileMode.Append, FileAccess.Write);
            file.WriteByte(0);
        });
        Expect("missing-file", firstName, directory => File.Delete(Path.Combine(directory, firstName)));
        Expect("wrong-version", RoomLevelStreamFiles.ManifestFileName,
            directory => EditManifest(directory, manifest => manifest["version"] = 1));
        Expect("wrong-provenance", RoomLevelStreamFiles.ManifestFileName,
            directory => EditManifest(directory, manifest => manifest["sourceCartridgeSha256"] = new string('0', 64)));
        Expect("extra-source", unknownName, directory =>
        {
            byte[] stream = [0, 0];
            File.WriteAllBytes(Path.Combine(directory, unknownName), stream);
            EditManifest(directory, manifest => manifest.Required("entries").AsObject()[unknownName] = new JsonObject
            {
                ["level"] = "UnknownLevel",
                ["byteCount"] = stream.Length,
                ["sha256"] = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream)),
            });
        });
        Expect("missing-source", firstName,
            directory => EditManifest(directory, manifest => manifest.Required("entries").AsObject().Remove(firstName)));
    }

}
