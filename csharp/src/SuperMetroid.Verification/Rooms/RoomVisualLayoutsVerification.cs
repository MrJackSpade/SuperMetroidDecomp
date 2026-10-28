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

/// <summary>Checks compiled room visuals against native level streams while keeping collision and BTS authoritative.</summary>
internal static class RoomVisualLayoutsVerification
{
    /// <summary>
    /// Checks every stock visual reference against its native source, then proves an
    /// installed override changes both rendered planes without changing collision/BTS.
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
            byte[] native = RomDataReader.Decompress(bus, RoomLevelSourceDefinitions.Addresses[level]);
            AssertTrue(native.SequenceEqual(File.ReadAllBytes(Path.Combine(
                    installed.RoomLevelStreamDirectory, RoomLevelStreamFiles.LevelFileName(level)))),
                $"installed level file for {level} is the complete native decompressor output");
            AssertTrue(native.SequenceEqual(levelStreams.Get(level).Span),
                $"installed room level {level} matches every native allocation byte");
            int layerBytes = BinaryPrimitives.ReadUInt16LittleEndian(native);
            int count = layerBytes / 2;
            int backgroundOffset = 2 + layerBytes + count;
            int backgroundBytes = Math.Min(layerBytes, native.Length - backgroundOffset);
            AssertEqual(width, layout.Size.X, $"room level {level} visual stride");
            AssertEqual(count / width, layout.Size.Y,
                $"room level {level} visual allocation height");
            for (int index = 0; index < count; index++)
            {
                ushort foreground = unchecked((ushort)(
                    BinaryPrimitives.ReadUInt16LittleEndian(native.AsSpan(2 + index * 2)) & 0x0fff));
                ushort background = index * 2 < backgroundBytes
                    ? unchecked((ushort)(BinaryPrimitives.ReadUInt16LittleEndian(
                        native.AsSpan(backgroundOffset + index * 2)) & 0x0fff))
                    : (ushort)0;
                AssertEqual(foreground, layout.ForegroundVisualWords.Span[index],
                    $"room level {level} BG1 visual word {index}");
                AssertEqual(background, layout.BackgroundVisualWords.Span[index],
                    $"room level {level} BG2 visual word {index}");
            }
        }

        // Exercise the real room loader for every selectable state, not merely
        // representative addresses. Each state's compressed level source is
        // forbidden, so an accidental ROM fallback fails at its first read.
        RoomCharacterAtlases characters = installed.LoadRoomCharacters();
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
                    characterArt: characters, paletteArt: palettes,
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
                        : RoomLevelMemoryLayout.PrefilledLevelWord;
                    AssertEqual(nativeWord, block.PackedWord.Raw,
                        $"room ${header.Pointer:X4} state ${statePointer:X4} collision word {blockIndex}");
                    AssertEqual(nativeBts, block.Bts.Value,
                        $"room ${header.Pointer:X4} state ${statePointer:X4} BTS {blockIndex}");
                    AssertEqual(nativeBg2Word, loaded.BackgroundEntries.AsSpan()[blockIndex],
                        $"room ${header.Pointer:X4} state ${statePointer:X4} BG2 word {blockIndex}");
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
        JsonArray foregroundWords = document.Required("foregroundVisualWords").AsArray();
        JsonArray backgroundWords = document.Required("backgroundVisualWords").AsArray();
        foregroundWords[0] = foregroundWords.Required(0).GetValue<int>() ^ 0x0400;
        backgroundWords[0] = backgroundWords.Required(0).GetValue<int>() ^ 0x0400;
        Directory.CreateDirectory(installed.RoomVisualLayoutOverrideDirectory);
        string overridePath = Path.Combine(installed.RoomVisualLayoutOverrideDirectory, name);
        File.WriteAllText(overridePath, document.ToJsonString());

        LoadedRoomAssets nativeRoom = LoadFixtureRoomAssets(landing);
        LoadedRoomAssets stockRoom = LoadFixtureRoomAssets(landing,
            visualLayouts: stock, levelStreams: levelStreams);
        LoadedRoomAssets editedRoom = LoadFixtureRoomAssets(landing,
            visualLayouts: installed.LoadRoomVisualLayouts(), levelStreams: levelStreams);
        AssertTrue(nativeRoom.LevelData.ForegroundEntries.Span.SequenceEqual(
                editedRoom.LevelData.ForegroundEntries.Span) &&
            nativeRoom.LevelData.BehaviorBytes.AsSpan().SequenceEqual(
                editedRoom.LevelData.BehaviorBytes.AsSpan()) &&
            nativeRoom.LevelData.BackgroundEntries.AsSpan().SequenceEqual(
                editedRoom.LevelData.BackgroundEntries.AsSpan()),
            "visual room-layout override leaves full native BG1/BTS/BG2 allocations unchanged");

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
            AssertTrue(nativeOther.LevelData.ForegroundEntries.Span.SequenceEqual(
                    installedOther.LevelData.ForegroundEntries.Span) &&
                nativeOther.LevelData.BehaviorBytes.AsSpan().SequenceEqual(
                    installedOther.LevelData.BehaviorBytes.AsSpan()) &&
                nativeOther.LevelData.BackgroundEntries.AsSpan().SequenceEqual(
                    installedOther.LevelData.BackgroundEntries.AsSpan()) &&
                nativeOther.LevelData.Size.Y == installedOther.LevelData.Size.Y,
                $"installed room ${pointer:X4} retains native collision, BTS, BG2, and allocation height");
        }

        // Existing user content is outside the replaceable stock installation. Reimport
        // must retain the selected visual word while native collision remains unchanged.
        _ = GameAssetInstaller.EnsureInstalled(installed.Root)
            ?? throw new InvalidOperationException("Room-layout repair lost the installation.");
        AssertEqual(foregroundWords.Required(0).GetValue<int>(),
            installed.LoadRoomVisualLayouts().Get(landingLevel).ForegroundVisualWords.Span[0],
            "room-layout override survives stock installation validation");

        int editedForegroundWord = foregroundWords.Required(0).GetValue<int>();
        foregroundWords[0] = 0x8000;
        File.WriteAllText(overridePath, document.ToJsonString());
        try
        {
            _ = installed.LoadRoomVisualLayouts();
            throw new InvalidOperationException("A collision bit was accepted as visual artwork.");
        }
        catch (InvalidDataException error)
        {
            AssertTrue(error.Message.Contains(name, StringComparison.Ordinal),
                "invalid visual-layout override identifies its file");
        }
        foregroundWords[0] = editedForegroundWord;
        File.WriteAllText(overridePath, document.ToJsonString());
        Console.WriteLine($"  Room layouts: {RoomVisualLayoutFiles.RetailLevels.Count} native levels and " +
            $"{stateCount} guarded state loads retain collision/BTS (including slopes and spikes) while BG1/BG2 edits " +
            "affect only rendering, survive repair, and reject collision bits.");
    }

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
