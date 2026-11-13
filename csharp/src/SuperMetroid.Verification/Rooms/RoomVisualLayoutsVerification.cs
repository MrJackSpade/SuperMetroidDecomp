using SuperMetroid.AssetExtraction;
using SuperMetroid.Tooling;
using System.Buffers.Binary;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.AssetExtraction.World.RoomStates;

namespace SuperMetroid.Verification.Rooms;

/// <summary>Checks the installed room terrain and appearances against the cartridge's level words, decoded by the import's codec.</summary>
internal static class RoomVisualLayoutsVerification
{
    /// <summary>
    /// Checks every installed level's terrain, behaviours, background and terrain beyond the room, and every stock
    /// appearance, against the cartridge's level words decoded by the import's codec.
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
        // Each level's cartridge words are decompressed once, for its own check and for every state that loads it.
        var decompressed = new Dictionary<int, byte[]>();
        byte[] NativeLevel(int source)
        {
            if (!decompressed.TryGetValue(source, out byte[]? native))
                decompressed[source] = native = RomDataReader.Decompress(bus, source);
            return native;
        }
        foreach ((RoomLevelId level, int width) in RoomVisualLayoutFiles.RetailLevels)
        {
            RoomLevelArtwork layout = stock.Get(level);
            InstalledRoomLevel terrain = levelStreams.Get(level);
            byte[] native = NativeLevel(RoomLevelSourceDefinitions.Addresses[level]);
            int layerBytes = BinaryPrimitives.ReadUInt16LittleEndian(native);
            int count = layerBytes / 2;
            int backgroundOffset = 2 + layerBytes + count;
            int backgroundBytes = Math.Min(layerBytes, native.Length - backgroundOffset);
            AssertEqual(width, layout.Size.X, $"room level {level} visual stride");
            AssertEqual(count / width, layout.Size.Y,
                $"room level {level} visual allocation height");
            AssertEqual(layout.Size, terrain.Size, $"room level {level} terrain grid size");
            string levelName = $"room level {level}";
            for (int index = 0; index < count; index++)
            {
                ushort foreground = BinaryPrimitives.ReadUInt16LittleEndian(native.AsSpan(2 + index * 2));
                RoomBlockAppearance background = RoomLevelWordCodec.DecodeAppearance(index * 2 < backgroundBytes
                    ? BinaryPrimitives.ReadUInt16LittleEndian(native.AsSpan(backgroundOffset + index * 2))
                    : (ushort)0);
                AssertBlockEqual(RoomLevelWordCodec.Decode(foreground), terrain.Foreground[index], levelName, "installed terrain", index);
                AssertBlockEqual(RoomBlockBehaviorCodec.Decode(native[2 + layerBytes + index]), terrain.Behaviors[index],
                    levelName, "installed behaviour", index);
                AssertBlockEqual(background, terrain.Background[index], levelName, "installed background", index);
                AssertBlockEqual(RoomLevelWordCodec.DecodeAppearance(foreground), layout.Foreground.Span[index],
                    levelName, "foreground appearance", index);
                AssertBlockEqual(background, layout.Background.Span[index], levelName, "background appearance", index);
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
        int stateCount = 0;
        int slopeBlocks = 0;
        int spikeBlocks = 0;
        foreach (RoomDefinition header in RetailRooms.All)
        {
            SelectedRoom room = SelectedRoom.LoadUsingCompiledSelection(header);
            ushort headerPointer = header.Pointer;
            foreach (ushort statePointer in RoomStateSelection.GetStatePointers(header))
            {
                string stateName = $"room ${headerPointer:X4} state ${statePointer:X4}";
                room = room with { State = RoomStateDefinitions.Get(statePointer) };
                int source = RoomLevelSourceDefinitions.Addresses[room.State.Level];
                byte[] native = NativeLevel(source);
                int layerBytes = BinaryPrimitives.ReadUInt16LittleEndian(native);
                int blockCount = layerBytes / 2;
                int bg2Offset = 2 + layerBytes + blockCount;
                int availableBg2 = Math.Min(layerBytes, native.Length - bg2Offset);
                RoomLevelData loaded = LoadFixtureRoomAssets(room,
                    visualLayouts: stock, levelStreams: levelStreams).LevelData;
                AssertEqual(blockCount, loaded.Size.X * loaded.Size.Y, $"{stateName} physical allocation");
                // The plane's shown blocks are built once per state, not once per block read.
                RoomBlockAppearance[] shownBackground = loaded.BackgroundPlane.Blocks;
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
                    AssertBlockEqual(RoomLevelWordCodec.Decode(nativeWord), block.Terrain, stateName, "terrain", blockIndex);
                    AssertBlockEqual(RoomBlockBehaviorCodec.Decode(nativeBts), block.Behavior, stateName, "behaviour", blockIndex);
                    AssertBlockEqual(RoomLevelWordCodec.DecodeAppearance(nativeBg2Word), shownBackground[blockIndex],
                        stateName, "background", blockIndex);
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

        LoadedRoomAssets nativeRoom = LoadFixtureRoomAssets(landing);
        LoadedRoomAssets stockRoom = LoadFixtureRoomAssets(landing,
            visualLayouts: stock, levelStreams: levelStreams);

        // The renderers resolve each block from the level planes (#1346), so the shown first blocks, each a whole block and
        // its mirroring (#1431), are the planes'.
        RoomBlockCell nativeFirst = nativeRoom.LevelData.ForegroundPlane.Cells.Cells[0];
        AssertEqual(nativeFirst, stockRoom.LevelData.ForegroundPlane.Cells.Cells[0],
            "stock installed BG1 layout retains the native first block and mirroring");

        RoomBlockCell nativeBg2 = nativeRoom.LevelData.BackgroundPlane.Cells.Cells[0];
        AssertEqual(nativeBg2, stockRoom.LevelData.BackgroundPlane.Cells.Cells[0],
            "stock installed BG2 layout retains the native first block and mirroring");

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
        Console.WriteLine($"  Room layouts: {RoomVisualLayoutFiles.RetailLevels.Count} levels decode to the installed terrain and " +
            $"{stateCount} guarded state loads keep it (including slopes and spikes), and the installed appearances are the native ones.");
    }

    /// <summary>
    /// Asserts block <paramref name="index"/>'s <paramref name="property"/> in <paramref name="where"/> is
    /// <paramref name="expected"/>, building the failure's context only for a mismatch, since every block of every level
    /// and state is compared.
    /// </summary>
    private static void AssertBlockEqual<T>(T expected, T actual, string where, string property, int index)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            AssertEqual(expected, actual, $"{where} {property} {index}");
    }

    /// <summary>The cartridge's level-space fill word, which the import decodes as the game's edge-solid block.</summary>
    private const ushort FillWord = 0x8000;

    /// <summary>The level words the cartridge fills before decompressing a room.</summary>
    private const int LevelSpaceWords = 0x3200;

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
