using System.Buffers.Binary;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;
using static RetailRuntimeFixtureVerification;

internal static class RoomVisualLayoutsVerification
{
    /// <summary>
    /// Checks every stock visual reference against its native source, then proves an
    /// installed override changes both rendered planes without changing collision/BTS.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for compressed room level streams")]
    internal static void VerifyRoomVisualLayouts(GameInstallation installed,
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus, CartridgeRoomHeader landing)
    {
        RoomVisualLayoutCatalog stock = installed.LoadRoomVisualLayouts();
        AssertEqual(246, RoomVisualLayoutFiles.RetailSources.Count,
            "distinct pinned room-level visual sources");
        RoomLevelStreamCatalog levelStreams = installed.LoadRoomLevelStreams();
        AssertEqual(RoomVisualLayoutFiles.RetailSources.Count, levelStreams.Count,
            "installed room-level source count");
        AssertTrue(!Directory.Exists(Path.Combine(installed.Root, "overrides",
                GameInstallationLayout.RoomLevelStreamDirectoryName)),
            "installed room-level streams have no override directory");
        VerifyRoomLevelStreamRejections(installed);
        try
        {
            _ = levelStreams.Get(0);
            throw new InvalidOperationException("Unknown installed room-level source was accepted.");
        }
        catch (InvalidDataException error)
        {
            AssertTrue(error.Message.Contains("lacks source", StringComparison.Ordinal),
                "unknown installed room-level source fails without ROM fallback");
        }
        foreach ((int source, int width) in RoomVisualLayoutFiles.RetailSources)
        {
            RoomVisualLayout layout = stock.Get(source);
            byte[] native = RomDataReader.Decompress(bus, source);
            AssertTrue(native.AsSpan().SequenceEqual(File.ReadAllBytes(Path.Combine(
                    installed.RoomLevelStreamDirectory, RoomLevelStreamFiles.SourceFileName(source)))),
                $"installed level file for ${source:X6} is the complete native decompressor output");
            AssertTrue(native.AsSpan().SequenceEqual(levelStreams.Get(source).Span),
                $"installed room-level source ${source:X6} matches every native allocation byte");
            int layerBytes = BinaryPrimitives.ReadUInt16LittleEndian(native);
            int count = layerBytes / 2;
            int backgroundOffset = 2 + layerBytes + count;
            int backgroundBytes = Math.Min(layerBytes, native.Length - backgroundOffset);
            AssertEqual(width, layout.Size.X, $"room ${source:X6} visual stride");
            AssertEqual(count / width, layout.Size.Y,
                $"room ${source:X6} visual allocation height");
            for (int index = 0; index < count; index++)
            {
                ushort foreground = unchecked((ushort)(
                    BinaryPrimitives.ReadUInt16LittleEndian(native.AsSpan(2 + index * 2)) & 0x0fff));
                ushort background = index * 2 < backgroundBytes
                    ? unchecked((ushort)(BinaryPrimitives.ReadUInt16LittleEndian(
                        native.AsSpan(backgroundOffset + index * 2)) & 0x0fff))
                    : (ushort)0;
                AssertEqual(foreground, layout.ForegroundVisualWords.Span[index],
                    $"room ${source:X6} BG1 visual word {index}");
                AssertEqual(background, layout.BackgroundVisualWords.Span[index],
                    $"room ${source:X6} BG2 visual word {index}");
            }
        }

        // Exercise the real room loader for every selectable state, not merely
        // representative addresses. Each state's compressed level source is
        // forbidden, so an accidental ROM fallback fails at its first read.
        RoomCharacterAtlasCatalog characters = installed.LoadRoomCharacters();
        RoomMetatileCatalog metatiles = installed.LoadRoomMetatiles();
        RoomStaticPaletteCatalog palettes = installed.LoadRoomPalettes();
        int stateCount = 0;
        int slopeBlocks = 0;
        int spikeBlocks = 0;
        foreach (RoomHeaderDefinition header in RoomHeaderDefinitions.All)
        {
            CartridgeRoomHeader room = CartridgeRoomHeader.LoadUsingCompiledSelection(
                header.Pointer);
            foreach (ushort statePointer in RoomStateSelectionDefinitions.GetStatePointers(
                header.Pointer))
            {
                room = room with { State = RoomStateDefinitions.Get(statePointer) };
                int source = room.State.CompressedLevelDataAddress;
                byte[] native = RomDataReader.Decompress(bus, source);
                int layerBytes = BinaryPrimitives.ReadUInt16LittleEndian(native);
                int blockCount = layerBytes / 2;
                int bg2Offset = 2 + layerBytes + blockCount;
                int availableBg2 = Math.Min(layerBytes, native.Length - bg2Offset);
                RoomLevelData loaded = LoadFixtureRoomAssets(
                    NewRoomLevelStreamReadGuard(bus, source), room,
                    characterArt: characters, paletteArt: palettes,
                    metatileArt: metatiles, visualLayouts: stock, levelStreams: levelStreams).LevelData;
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
                    AssertEqual(nativeWord, block.LevelWord,
                        $"room ${header.Pointer:X4} state ${statePointer:X4} collision word {blockIndex}");
                    AssertEqual(nativeBts, block.Behavior,
                        $"room ${header.Pointer:X4} state ${statePointer:X4} BTS {blockIndex}");
                    AssertEqual(nativeBg2Word, loaded.BackgroundEntries.Span[blockIndex],
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

        int landingSource = landing.State.CompressedLevelDataAddress;
        string name = RoomVisualLayoutFiles.SourceFileName(landingSource);
        string stockPath = Path.Combine(installed.RoomVisualLayoutDirectory, name);
        JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
            ?? throw new InvalidDataException("Installed Landing Site layout is empty.");
        JsonArray foregroundWords = document["foregroundVisualWords"]!.AsArray();
        JsonArray backgroundWords = document["backgroundVisualWords"]!.AsArray();
        foregroundWords[0] = foregroundWords[0]!.GetValue<int>() ^ 0x0400;
        backgroundWords[0] = backgroundWords[0]!.GetValue<int>() ^ 0x0400;
        Directory.CreateDirectory(installed.RoomVisualLayoutOverrideDirectory);
        string overridePath = Path.Combine(installed.RoomVisualLayoutOverrideDirectory, name);
        File.WriteAllText(overridePath, document.ToJsonString());

        CartridgeRoomAssets nativeRoom = LoadFixtureRoomAssets(bus, landing);
        CartridgeRoomAssets stockRoom = LoadFixtureRoomAssets(
            NewRoomLevelStreamReadGuard(bus, landingSource), landing,
            visualLayouts: stock, levelStreams: levelStreams);
        CartridgeRoomAssets editedRoom = LoadFixtureRoomAssets(
            NewRoomLevelStreamReadGuard(bus, landingSource), landing,
            visualLayouts: installed.LoadRoomVisualLayouts(), levelStreams: levelStreams);
        AssertTrue(nativeRoom.LevelData.ForegroundEntries.Span.SequenceEqual(
                editedRoom.LevelData.ForegroundEntries.Span) &&
            nativeRoom.LevelData.BehaviorBytes.Span.SequenceEqual(
                editedRoom.LevelData.BehaviorBytes.Span) &&
            nativeRoom.LevelData.BackgroundEntries.Span.SequenceEqual(
                editedRoom.LevelData.BackgroundEntries.Span),
            "visual room-layout override leaves full native BG1/BTS/BG2 allocations unchanged");

        BackgroundTilemapStreamer nativeStream = nativeRoom.LevelData.CreateBackgroundStreamer();
        BackgroundTilemapStreamer stockStream = stockRoom.LevelData.CreateBackgroundStreamer();
        BackgroundTilemapStreamer editedStream = editedRoom.LevelData.CreateBackgroundStreamer();
        PlmTilemapUpdate nativeFirst = nativeStream.BuildPlmLevelBlockUpdate(0, 0);
        PlmTilemapUpdate stockFirst = stockStream.BuildPlmLevelBlockUpdate(0, 0);
        PlmTilemapUpdate editedFirst = editedStream.BuildPlmLevelBlockUpdate(0, 0);
        AssertTrue(nativeFirst.TopRow.AsSpan().SequenceEqual(stockFirst.TopRow) &&
            nativeFirst.BottomRow.AsSpan().SequenceEqual(stockFirst.BottomRow),
            "stock installed BG1 layout retains native rendered tile words");
        AssertTrue(!nativeFirst.TopRow.AsSpan().SequenceEqual(editedFirst.TopRow) ||
            !nativeFirst.BottomRow.AsSpan().SequenceEqual(editedFirst.BottomRow),
            "edited BG1 layout changes the live renderer's first block");

        var bg2Request = new BackgroundUpdateRequest(BackgroundLayer.Background,
            BackgroundUpdateAxis.Row, SourceBlock: new(0, 0), VramBlock: new(0, 0));
        TilemapStreamUpdate nativeBg2 = nativeStream.Build(bg2Request)!
            ?? throw new InvalidOperationException("Native BG2 row was suppressed.");
        TilemapStreamUpdate stockBg2 = stockStream.Build(bg2Request)!
            ?? throw new InvalidOperationException("Stock BG2 row was suppressed.");
        TilemapStreamUpdate editedBg2 = editedStream.Build(bg2Request)!
            ?? throw new InvalidOperationException("Edited BG2 row was suppressed.");
        // A row update's first DMA segment sources the complete staged top-half tile row.
        AssertTrue(nativeBg2.Segments[0].SourceWords.AsSpan().SequenceEqual(stockBg2.Segments[0].SourceWords),
            "stock installed BG2 layout retains native rendered tile words");
        AssertTrue(!nativeBg2.Segments[0].SourceWords.AsSpan().SequenceEqual(editedBg2.Segments[0].SourceWords),
            "edited BG2 layout changes the live renderer's first row");

        // Ceres has no authored BG2 tail, while the Wrecked Ship allocation includes
        // an extra physical row beyond its camera. Both must survive the installed-stream path.
        foreach (ushort pointer in new ushort[] { 0xdf8d, 0xc98e })
        {
            CartridgeRoomHeader room = SuperMetroid.AssetExtraction.CartridgeRoomHeaderImporter.Load(bus, pointer);
            CartridgeRoomAssets nativeOther = LoadFixtureRoomAssets(bus, room);
            CartridgeRoomAssets installedOther = LoadFixtureRoomAssets(
                NewRoomLevelStreamReadGuard(bus, room.State.CompressedLevelDataAddress), room,
                visualLayouts: stock, levelStreams: levelStreams);
            AssertTrue(nativeOther.LevelData.ForegroundEntries.Span.SequenceEqual(
                    installedOther.LevelData.ForegroundEntries.Span) &&
                nativeOther.LevelData.BehaviorBytes.Span.SequenceEqual(
                    installedOther.LevelData.BehaviorBytes.Span) &&
                nativeOther.LevelData.BackgroundEntries.Span.SequenceEqual(
                    installedOther.LevelData.BackgroundEntries.Span) &&
                nativeOther.LevelData.Size.Y == installedOther.LevelData.Size.Y,
                $"installed room ${pointer:X4} retains native collision, BTS, BG2, and allocation height");
        }

        // Existing user content is outside the replaceable stock installation. Reimport
        // must retain the selected visual word while native collision remains unchanged.
        _ = GameAssetInstaller.EnsureInstalled(installed.Root)
            ?? throw new InvalidOperationException("Room-layout repair lost the installation.");
        AssertEqual(foregroundWords[0]!.GetValue<int>(),
            installed.LoadRoomVisualLayouts().Get(landingSource).ForegroundVisualWords.Span[0],
            "room-layout override survives stock installation validation");

        int editedForegroundWord = foregroundWords[0]!.GetValue<int>();
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
        Console.WriteLine($"  Room layouts: {RoomVisualLayoutFiles.RetailSources.Count} native sources and " +
            $"{stateCount} guarded state loads retain collision/BTS (including slopes and spikes) while BG1/BG2 edits " +
            "affect only rendering, survive repair, and reject collision bits.");
    }

    /// <summary>
    /// Confirms the stock-only level-stream loader rejects each corrupted installation with a
    /// path-specific error: a changed or resized stream, a missing file, an unsupported version
    /// or provenance, and an unknown or missing source.
    /// </summary>
    private static void VerifyRoomLevelStreamRejections(GameInstallation installed)
    {
        using var scratch = new TestScratchDirectory("room-level-stream-rejections");
        string firstName = RoomLevelStreamFiles.SourceFileName(RoomVisualLayoutFiles.RetailSources.Keys.Min());
        const string unknownName = "level-000000.bin";

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
            JsonObject manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
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
            directory => EditManifest(directory, manifest => manifest["version"] = 2));
        Expect("wrong-provenance", RoomLevelStreamFiles.ManifestFileName,
            directory => EditManifest(directory, manifest => manifest["sourceCartridgeSha256"] = new string('0', 64)));
        Expect("extra-source", unknownName, directory =>
        {
            byte[] stream = [0, 0];
            File.WriteAllBytes(Path.Combine(directory, unknownName), stream);
            EditManifest(directory, manifest => manifest["entries"]!.AsObject()[unknownName] = new JsonObject
            {
                ["sourceAddress"] = 0,
                ["byteCount"] = stream.Length,
                ["sha256"] = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream)),
            });
        });
        Expect("missing-source", firstName,
            directory => EditManifest(directory, manifest => manifest["entries"]!.AsObject().Remove(firstName)));
    }

    private static CartridgeReadGuard NewRoomLevelStreamReadGuard(ISnesAddressSpace source, int levelSource) =>
        new CartridgeReadGuard(source).Forbid(address => address == levelSource,
            address => $"Installed room reread native level source ${address:X6}.");
}
