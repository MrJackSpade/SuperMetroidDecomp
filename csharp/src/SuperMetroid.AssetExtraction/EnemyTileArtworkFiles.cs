using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports every retail room-enemy graphics sheet; user PNG overrides live outside stock content.</summary>
public static class EnemyTileArtworkFiles
{
    /// <summary>Strict camel-case JSON settings used to validate enemy-art manifests.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    /// <summary>Writes the complete retail enemy-art installation: character sheets, palettes, sprite compositions, and supplemental boss and cutscene resources.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying room-enemy graphics definitions and the associated artwork sources.</param>
    /// <param name="directory">Stock output directory, created if needed; existing resource and manifest files are overwritten.</param>
    /// <param name="sourceCartridgeSha256">Nonempty source-cartridge SHA-256 recorded in the manifest alongside each stock resource's hash.</param>
    /// <remarks>Deduplicates ordinary sheets by enemy-definition identity and verifies retail coverage and native pixel/color roundtrips. Indexed PNG colors are diagnostic; RGB5 palettes and visual JSON remain separate. Does not read or modify player overrides.</remarks>
    /// <exception cref="InvalidDataException">Native definitions, coverage, artwork sizes, palette representation, or resource roundtrip validation are invalid.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);

        var entries = new Dictionary<ushort, EnemyTileFileEntry>();
        foreach (ushort graphicsSetPointer in RoomStateDefinitions.All
                     .Select(state => state.EnemyTilesetPointer).Distinct().Order())
        {
            int cursor = RoomEnemyRomLayout.TilesetBank | graphicsSetPointer;
            for (int slot = 0; slot <= 4; slot++, cursor += 4)
            {
                ushort definitionPointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), cursor);
                if (definitionPointer == 0xffff) break;
                if (slot == 4)
                    throw new InvalidDataException($"Enemy graphics set ${graphicsSetPointer:X4} exceeds four entries.");
                if (entries.ContainsKey(definitionPointer)) continue;

                RoomEnemyDefinition definition = SuperMetroid.AssetExtraction.RoomEnemyDefinitionImporter.Load(bus, definitionPointer);
                int byteCount = definition.TileDataSize & 0x7fff;
                int tileCount = RoomCharacterAtlasFormat.ValidateTileCount(byteCount);
                byte[] planar = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), definition.TileDataAddress, byteCount);
                int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
                byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, columns,
                    out int width, out int height);
                using var png = new MemoryStream();
                IndexedPng.Write(png, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
                byte[] encoded = png.ToArray();
                RoomCharacterAtlas roundtrip = RoomCharacterAtlas.Load(
                    new MemoryStream(encoded, writable: false), byteCount);
                if (!roundtrip.Transfer.Span.SequenceEqual(planar))
                    throw new InvalidDataException($"Enemy ${definitionPointer:X4} tile PNG changed native pixels.");

                File.WriteAllBytes(Path.Combine(directory, EnemyTileArtworkFormat.FileName(definitionPointer)), encoded);
                byte[] nativeColors = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                    (definition.Bank << 16) | definition.PalettePointer,
                    EnemyPaletteSheet.ColorCount * sizeof(ushort));
                var colors = new PaletteRgb5[EnemyPaletteSheet.ColorCount];
                for (int color = 0; color < colors.Length; color++)
                {
                    ushort word = BinaryPrimitives.ReadUInt16LittleEndian(nativeColors.AsSpan(color * 2));
                    if ((word & 0x8000) != 0)
                        throw new InvalidDataException($"Enemy ${definitionPointer:X4} palette has an unrepresentable high bit.");
                    colors[color] = new PaletteRgb5
                    {
                        Red = word & 31,
                        Green = word >> 5 & 31,
                        Blue = word >> 10 & 31,
                    };
                }
                byte[] paletteJson = EnemyPaletteSheet.Write(new EnemyPaletteSheetDocument
                {
                    Version = 1,
                    Colors = colors,
                });
                var nativeCgram = new SnesCgram();
                var compiledCgram = new SnesCgram();
                nativeCgram.LoadBytes(nativeColors);
                EnemyPaletteSheet.Load(new MemoryStream(paletteJson, writable: false))
                    .LoadTo(compiledCgram, 0);
                if (!nativeCgram.Colors.SequenceEqual(compiledCgram.Colors))
                    throw new InvalidDataException($"Enemy ${definitionPointer:X4} palette JSON changed native colors.");
                File.WriteAllBytes(Path.Combine(directory,
                    EnemyTileArtworkFormat.PaletteFileName(definitionPointer)), paletteJson);
                entries.Add(definitionPointer, new EnemyTileFileEntry(
                    byteCount, Convert.ToHexString(SHA256.HashData(encoded)),
                    Convert.ToHexString(SHA256.HashData(paletteJson))));
            }
        }
        if (entries.Count != EnemyTileArtworkFormat.RetailDefinitionCount)
            throw new InvalidDataException($"Expected {EnemyTileArtworkFormat.RetailDefinitionCount} retail enemy sheets; found {entries.Count}.");
        ValidateDefinitionIds(entries.Keys);
        byte[] firstMelt = ExtractCrocomireMelt(bus,
            CrocomireMeltingTransferDefinitions.Passes[0],
            CrocomireMeltingArtworkFormat.FirstByteCount);
        byte[] secondMelt = ExtractCrocomireMelt(bus,
            CrocomireMeltingTransferDefinitions.Passes[1],
            CrocomireMeltingArtworkFormat.SecondByteCount);
        byte[] firstMeltTilemap = ExtractCrocomireMeltTilemap(bus,
            CrocomireMeltingArtworkAddresses.FirstTilemap);
        byte[] secondMeltTilemap = ExtractCrocomireMeltTilemap(bus,
            CrocomireMeltingArtworkAddresses.SecondTilemap);
        File.WriteAllBytes(Path.Combine(directory, CrocomireMeltingArtworkFormat.FirstFileName), firstMelt);
        File.WriteAllBytes(Path.Combine(directory, CrocomireMeltingArtworkFormat.SecondFileName), secondMelt);
        File.WriteAllBytes(Path.Combine(directory,
            CrocomireMeltingArtworkFormat.FirstTilemapFileName), firstMeltTilemap);
        File.WriteAllBytes(Path.Combine(directory,
            CrocomireMeltingArtworkFormat.SecondTilemapFileName), secondMeltTilemap);
        byte[] skeletonPlanar = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            CrocomireSkeletonTransferDefinitions.Frames[0].SourceAddress,
            CrocomireSkeletonTransferDefinitions.TotalByteCount);
        byte[] skeletonPixels = SnesGraphics.DecodePlanarTiles(skeletonPlanar, 4,
            RoomCharacterAtlasFormat.TileColumns,
            out int skeletonWidth, out int skeletonHeight);
        using var skeletonImage = new MemoryStream();
        IndexedPng.Write(skeletonImage, skeletonWidth, skeletonHeight,
            skeletonPixels, SnesGraphics.DiagnosticPalette(16));
        byte[] skeletonPng = skeletonImage.ToArray();
        CrocomireSkeletonArtwork skeletonRoundtrip = CrocomireSkeletonArtwork.Load(
            new MemoryStream(skeletonPng, writable: false));
        for (int index = 0; index < CrocomireSkeletonTransferDefinitions.Frames.Length; index++)
        {
            ReadOnlySpan<byte> expected = skeletonPlanar.AsSpan(
                index * CrocomireSkeletonTransferDefinitions.ChunkByteCount,
                CrocomireSkeletonTransferDefinitions.ChunkByteCount);
            if (!skeletonRoundtrip.Chunk(index).Span.SequenceEqual(expected))
                throw new InvalidDataException(
                    $"Crocomire skeleton PNG changed native chunk {index} during extraction.");
        }
        File.WriteAllBytes(Path.Combine(directory,
            CrocomireSkeletonTransferDefinitions.FileName), skeletonPng);
        byte[] spritemapJson = EnemySpritemapFiles.Extract(bus);
        File.WriteAllBytes(Path.Combine(directory, EnemySpritemapDefinitions.FileName),
            spritemapJson);
        byte[] projectileSpritemapJson = EnemyProjectileSpritemapFiles.Extract(bus);
        File.WriteAllBytes(Path.Combine(directory,
            EnemyProjectileSpritemapDefinitions.FileName), projectileSpritemapJson);
        byte[] extendedJson = EnemyExtendedFrameFiles.Extract(bus);
        File.WriteAllBytes(Path.Combine(directory, EnemyExtendedFrameDefinitions.FileName),
            extendedJson);
        var torizoInstructionHashes = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (TorizoInstructionTileSheetDefinition page in
                 TorizoInstructionVramArtworkDefinitions.All)
        {
            byte[] png = ExtractTorizoInstructionPage(bus, page);
            File.WriteAllBytes(Path.Combine(directory, page.FileName), png);
            torizoInstructionHashes.Add(page.FileName,
                Convert.ToHexString(SHA256.HashData(png)));
        }
        var ceresEscapeTileHashes = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (CeresEscapeTileSheetDefinition page in
                 CeresEscapeTileArtworkDefinitions.All)
        {
            byte[] png = IndexedTilePageExtractor.Extract(bus, page.SourceAddress,
                page.ByteCount, $"Ceres escape page {page.FileName}");
            File.WriteAllBytes(Path.Combine(directory, page.FileName), png);
            ceresEscapeTileHashes.Add(page.FileName,
                Convert.ToHexString(SHA256.HashData(png)));
        }
        byte[] ceresEscapeOverlay = CeresEscapeOverlayTilemapFiles.Extract(bus);
        File.WriteAllBytes(Path.Combine(directory,
            CeresEscapeOverlayTilemapDefinitions.FileName), ceresEscapeOverlay);
        byte[] phantoonBg2Json = PhantoonBg2FrameFiles.Extract(bus);
        File.WriteAllBytes(Path.Combine(directory, PhantoonBg2FrameDefinitions.FileName),
            phantoonBg2Json);
        byte[] draygonBg2Json = DraygonBg2FrameFiles.Extract(bus);
        File.WriteAllBytes(Path.Combine(directory, DraygonBg2FrameDefinitions.FileName),
            draygonBg2Json);
        byte[] crocomireBg2Json = CrocomireBg2FrameFiles.Extract(bus);
        File.WriteAllBytes(Path.Combine(directory, CrocomireBg2FrameDefinitions.FileName),
            crocomireBg2Json);
        byte[] motherBrainBodyBg2Json = MotherBrainBodyBg2FrameFiles.Extract(bus);
        File.WriteAllBytes(Path.Combine(directory, MotherBrainBodyVisualDefinitions.Bg2FileName),
            motherBrainBodyBg2Json);
        var gunshipLiftoffHashes = new Dictionary<int, string>();
        for (int index = 0; index < GunshipLiftoffTransferDefinitions.Frames.Length; index++)
        {
            GunshipLiftoffTransferDefinition transfer =
                GunshipLiftoffTransferDefinitions.Frames[index];
            var planar = new byte[GunshipLiftoffTransferDefinitions.ByteCount];
            for (int offset = 0; offset < planar.Length; offset++)
                planar[offset] = bus.ReadCartridgeByte(transfer.SourceAddress + offset);
            byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4,
                RoomCharacterAtlasFormat.TileColumns, out int width, out int height);
            using var image = new MemoryStream();
            IndexedPng.Write(image, width, height, pixels,
                SnesGraphics.DiagnosticPalette(16));
            byte[] png = image.ToArray();
            RoomCharacterAtlas roundtrip = RoomCharacterAtlas.Load(
                new MemoryStream(png, writable: false), planar.Length);
            if (!roundtrip.Transfer.Span.SequenceEqual(planar))
                throw new InvalidDataException(
                    $"Gunship takeoff frame {index} changed native pixels during extraction.");
            File.WriteAllBytes(Path.Combine(directory,
                EnemyTileArtworkFormat.GunshipLiftoffFileName(index)), png);
            gunshipLiftoffHashes.Add(index,
                Convert.ToHexString(SHA256.HashData(png)));
        }
        // This bank-$B7 range is not in any ordinary enemy definition's VRAM set.
        // Preserve the complete tile-aligned source around the right-hand corpse
        // frame so edits can feed the native row-by-row WRAM decay processor.
        byte[] corpsePlanar = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            MotherBrainCorpseArtworkDefinitions.SourceAddress,
            MotherBrainCorpseArtworkDefinitions.ByteCount);
        byte[] corpsePixels = SnesGraphics.DecodePlanarTiles(corpsePlanar, 4,
            RoomCharacterAtlasFormat.TileColumns, out int corpseWidth, out int corpseHeight);
        using var corpseStream = new MemoryStream();
        IndexedPng.Write(corpseStream, corpseWidth, corpseHeight, corpsePixels,
            SnesGraphics.DiagnosticPalette(16));
        byte[] corpsePng = corpseStream.ToArray();
        RoomCharacterAtlas corpseRoundtrip = RoomCharacterAtlas.Load(
            new MemoryStream(corpsePng, writable: false),
            MotherBrainCorpseArtworkDefinitions.ByteCount);
        if (!corpseRoundtrip.Transfer.Span.SequenceEqual(corpsePlanar))
            throw new InvalidDataException("Mother Brain corpse PNG changed native pixels during extraction.");
        File.WriteAllBytes(Path.Combine(directory, MotherBrainCorpseArtworkDefinitions.FileName),
            corpsePng);
        // The following bank-$B7 pages are a different visual owner: the escape
        // typewriter characters. Keep their override separate from corpse decay.
        byte[] escapeTextPlanar = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            MotherBrainEscapeTextArtworkDefinitions.SourceAddress,
            MotherBrainEscapeTextArtworkDefinitions.ByteCount);
        byte[] escapeTextPixels = SnesGraphics.DecodePlanarTiles(escapeTextPlanar, 4,
            RoomCharacterAtlasFormat.TileColumns,
            out int escapeTextWidth, out int escapeTextHeight);
        using var escapeTextStream = new MemoryStream();
        IndexedPng.Write(escapeTextStream, escapeTextWidth, escapeTextHeight,
            escapeTextPixels, SnesGraphics.DiagnosticPalette(16));
        byte[] escapeTextPng = escapeTextStream.ToArray();
        RoomCharacterAtlas escapeTextRoundtrip = RoomCharacterAtlas.Load(
            new MemoryStream(escapeTextPng, writable: false),
            MotherBrainEscapeTextArtworkDefinitions.ByteCount);
        if (!escapeTextRoundtrip.Transfer.Span.SequenceEqual(escapeTextPlanar))
            throw new InvalidDataException(
                "Mother Brain escape-text PNG changed native pixels during extraction.");
        File.WriteAllBytes(Path.Combine(directory,
            MotherBrainEscapeTextArtworkDefinitions.FileName), escapeTextPng);
        var motherBrainSpecialHashes = new Dictionary<int, string>();
        foreach (MotherBrainSpecialSpriteSheetDefinition sheet in
                 MotherBrainSpecialSpriteArtworkDefinitions.All)
        {
            byte[] png = ExtractMotherBrainSpecialSpritePng(bus, sheet);
            File.WriteAllBytes(Path.Combine(directory, sheet.FileName), png);
            motherBrainSpecialHashes.Add(sheet.SourceAddress,
                Convert.ToHexString(SHA256.HashData(png)));
        }
        byte[] upperKraid = ExtractKraidTilemap(bus, KraidBackgroundRomData.UpperTilemap);
        byte[] lowerKraid = ExtractKraidTilemap(bus, KraidBackgroundRomData.LowerTilemap);
        File.WriteAllBytes(Path.Combine(directory, KraidBackgroundArtworkFormat.UpperFileName),
            upperKraid);
        File.WriteAllBytes(Path.Combine(directory, KraidBackgroundArtworkFormat.LowerFileName),
            lowerKraid);
        ushort[] headPointers = KraidHeadInstructionDefinitions.All.ToArray()
            .Where(frame => frame.Kind == KraidHeadInstructionKind.Frame)
            .Select(frame => frame.Tilemap).Distinct().Order().ToArray();
        var kraidHeadHashes = new Dictionary<ushort, string>();
        foreach (ushort pointer in headPointers)
        {
            byte[] native = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                KraidBackgroundRomData.NativeBank | pointer,
                KraidBackgroundRomData.HeadTilemapWords * sizeof(ushort));
            byte[] json = KraidHeadTilemapAtlas.Encode(native);
            File.WriteAllBytes(Path.Combine(directory,
                KraidBackgroundArtworkFormat.HeadFileName(pointer)), json);
            kraidHeadHashes.Add(pointer, Convert.ToHexString(SHA256.HashData(json)));
        }
        byte[] kraidRoomBackground = ExtractKraidRoomBackground(bus);
        File.WriteAllBytes(Path.Combine(directory,
            KraidBackgroundArtworkFormat.RoomBackgroundFileName), kraidRoomBackground);
        byte[] kraidColors = ExtractKraidColors(bus);
        File.WriteAllBytes(Path.Combine(directory, KraidColorFormat.FileName), kraidColors);
        (string ceresDoorTilesHash, string ceresDoorColorsHash) =
            CeresDoorVisualFiles.Extract(bus, directory);
        byte[] magdollitePaletteCycle = MagdollitePaletteCycleExtractor.Extract(bus);
        byte[] auxiliaryColors = EnemyAuxiliaryColorFiles.Extract(bus);
        File.WriteAllBytes(Path.Combine(directory, EnemyAuxiliaryColorFormat.FileName), auxiliaryColors);
        File.WriteAllBytes(Path.Combine(directory, MagdollitePaletteCycleFormat.FileName),
            magdollitePaletteCycle);
        byte[] workRobotPaletteCycle = WorkRobotPaletteCycleExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(directory, WorkRobotPaletteCycleFormat.FileName),
            workRobotPaletteCycle);
        byte[] crocomireColors = CrocomireColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(directory, CrocomireColorFormat.FileName),
            crocomireColors);
        byte[] draygonColors = DraygonColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(directory, DraygonColorFormat.FileName),
            draygonColors);
        byte[] phantoonColors = PhantoonColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(directory, PhantoonColorFormat.FileName),
            phantoonColors);
        byte[] chozoAndTubeColors = ChozoAndTubeColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(directory, ChozoAndTubeColorFormat.FileName),
            chozoAndTubeColors);
        byte[] sporeSpawnColors = SporeSpawnColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(directory, SporeSpawnColorFormat.FileName),
            sporeSpawnColors);
        byte[] dachoraColors = DachoraColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(directory, DachoraColorFormat.FileName),
            dachoraColors);
        byte[] shitroidColors = ShitroidColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(directory, ShitroidColorFormat.FileName),
            shitroidColors);
        byte[] babyMetroidCutsceneColors =
            BabyMetroidCutsceneColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(directory,
            BabyMetroidCutsceneColorFormat.FileName), babyMetroidCutsceneColors);
        byte[] botwoonColors = BotwoonColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(directory, BotwoonColorFormat.FileName),
            botwoonColors);
        byte[] motherBrainDeathColors = MotherBrainDeathColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(directory,
            MotherBrainDeathColorFormat.FileName), motherBrainDeathColors);
        byte[] zebetiteColors = ZebetiteColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(directory, ZebetiteColorFormat.FileName),
            zebetiteColors);
        byte[] norfairRidleyColors = NorfairRidleyColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(directory, NorfairRidleyColorFormat.FileName),
            norfairRidleyColors);
        byte[] tourianStatueColors = TourianStatueColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(directory, TourianStatueColorFormat.FileName),
            tourianStatueColors);
        var manifest = new EnemyTileManifest(EnemyTileArtworkFormat.Version,
            sourceCartridgeSha256, entries,
            Convert.ToHexString(SHA256.HashData(firstMelt)),
            Convert.ToHexString(SHA256.HashData(secondMelt)),
            Convert.ToHexString(SHA256.HashData(firstMeltTilemap)),
            Convert.ToHexString(SHA256.HashData(secondMeltTilemap)),
            Convert.ToHexString(SHA256.HashData(spritemapJson)),
            Convert.ToHexString(SHA256.HashData(projectileSpritemapJson)),
            Convert.ToHexString(SHA256.HashData(extendedJson)),
            Convert.ToHexString(SHA256.HashData(phantoonBg2Json)),
            Convert.ToHexString(SHA256.HashData(draygonBg2Json)),
            Convert.ToHexString(SHA256.HashData(crocomireBg2Json)),
            Convert.ToHexString(SHA256.HashData(motherBrainBodyBg2Json)),
            gunshipLiftoffHashes,
            torizoInstructionHashes,
            ceresEscapeTileHashes,
            Convert.ToHexString(SHA256.HashData(ceresEscapeOverlay)),
            Convert.ToHexString(SHA256.HashData(corpsePng)),
            Convert.ToHexString(SHA256.HashData(escapeTextPng)),
            motherBrainSpecialHashes,
            Convert.ToHexString(SHA256.HashData(upperKraid)),
            Convert.ToHexString(SHA256.HashData(lowerKraid)),
            kraidHeadHashes,
            Convert.ToHexString(SHA256.HashData(kraidRoomBackground)),
            Convert.ToHexString(SHA256.HashData(kraidColors)),
            ceresDoorTilesHash, ceresDoorColorsHash,
            Convert.ToHexString(SHA256.HashData(magdollitePaletteCycle)),
            Convert.ToHexString(SHA256.HashData(workRobotPaletteCycle)),
            Convert.ToHexString(SHA256.HashData(crocomireColors)),
            Convert.ToHexString(SHA256.HashData(draygonColors)),
            Convert.ToHexString(SHA256.HashData(phantoonColors)),
            Convert.ToHexString(SHA256.HashData(chozoAndTubeColors)),
            Convert.ToHexString(SHA256.HashData(sporeSpawnColors)),
            Convert.ToHexString(SHA256.HashData(dachoraColors)),
            Convert.ToHexString(SHA256.HashData(shitroidColors)),
            Convert.ToHexString(SHA256.HashData(babyMetroidCutsceneColors)),
            Convert.ToHexString(SHA256.HashData(botwoonColors)),
            Convert.ToHexString(SHA256.HashData(motherBrainDeathColors)),
            Convert.ToHexString(SHA256.HashData(zebetiteColors)),
            Convert.ToHexString(SHA256.HashData(norfairRidleyColors)),
            Convert.ToHexString(SHA256.HashData(tourianStatueColors)),
            Convert.ToHexString(SHA256.HashData(skeletonPng)),
            Convert.ToHexString(SHA256.HashData(auxiliaryColors)));
        File.WriteAllBytes(Path.Combine(directory, EnemyTileArtworkFormat.ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(manifest, JsonOptions));
    }

    /// <summary>Checks stock hashes, then compiles selected PNGs without reading a cartridge.</summary>
    public static EnemyTileArtworkCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, EnemyTileArtworkFormat.ManifestFileName);
        EnemyTileManifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<EnemyTileManifest>(File.ReadAllBytes(manifestPath), JsonOptions)
                ?? throw new InvalidDataException("Enemy tile manifest is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid enemy tile manifest {manifestPath}.", error);
        }
        if (manifest.Version != EnemyTileArtworkFormat.Version ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase) ||
            manifest.Entries is null ||
            manifest.Entries.Count != EnemyTileArtworkFormat.RetailDefinitionCount ||
            string.IsNullOrWhiteSpace(manifest.CrocomireFirstSha256) ||
            string.IsNullOrWhiteSpace(manifest.CrocomireSecondSha256) ||
            string.IsNullOrWhiteSpace(manifest.CrocomireFirstTilemapSha256) ||
            string.IsNullOrWhiteSpace(manifest.CrocomireSecondTilemapSha256) ||
            string.IsNullOrWhiteSpace(manifest.CrocomireSkeletonSha256) ||
            string.IsNullOrWhiteSpace(manifest.EnemyCompositionsSha256) ||
            string.IsNullOrWhiteSpace(manifest.EnemyProjectileCompositionsSha256) ||
            string.IsNullOrWhiteSpace(manifest.EnemyExtendedCompositionsSha256) ||
            string.IsNullOrWhiteSpace(manifest.PhantoonBg2FramesSha256) ||
            string.IsNullOrWhiteSpace(manifest.DraygonBg2FramesSha256) ||
            string.IsNullOrWhiteSpace(manifest.CrocomireBg2FramesSha256) ||
            string.IsNullOrWhiteSpace(manifest.MotherBrainBodyBg2FramesSha256) ||
            manifest.GunshipLiftoffSha256 is null ||
            manifest.GunshipLiftoffSha256.Count !=
                GunshipLiftoffTransferDefinitions.Frames.Length ||
            manifest.TorizoInstructionTilesSha256 is null ||
            manifest.TorizoInstructionTilesSha256.Count !=
                TorizoInstructionVramArtworkDefinitions.All.Length ||
            manifest.CeresEscapeTilesSha256 is null ||
            manifest.CeresEscapeTilesSha256.Count !=
                CeresEscapeTileArtworkDefinitions.All.Length ||
            string.IsNullOrWhiteSpace(manifest.CeresEscapeOverlaySha256) ||
            string.IsNullOrWhiteSpace(manifest.MotherBrainCorpseSha256) ||
            string.IsNullOrWhiteSpace(manifest.MotherBrainEscapeTextSha256) ||
            manifest.MotherBrainSpecialSpritesSha256 is null ||
            manifest.MotherBrainSpecialSpritesSha256.Count !=
                MotherBrainSpecialSpriteArtworkDefinitions.All.Count ||
            string.IsNullOrWhiteSpace(manifest.KraidUpperSha256) ||
            string.IsNullOrWhiteSpace(manifest.KraidLowerSha256) ||
            manifest.KraidHeadsSha256 is null ||
            string.IsNullOrWhiteSpace(manifest.KraidRoomBackgroundSha256) ||
            string.IsNullOrWhiteSpace(manifest.KraidColorsSha256) ||
            string.IsNullOrWhiteSpace(manifest.CeresDoorTilesSha256) ||
            string.IsNullOrWhiteSpace(manifest.CeresDoorColorsSha256) ||
            string.IsNullOrWhiteSpace(manifest.MagdollitePaletteCycleSha256) ||
            string.IsNullOrWhiteSpace(manifest.AuxiliaryColorsSha256) ||
            string.IsNullOrWhiteSpace(manifest.WorkRobotPaletteCycleSha256) ||
            string.IsNullOrWhiteSpace(manifest.CrocomireColorsSha256) ||
            string.IsNullOrWhiteSpace(manifest.DraygonColorsSha256) ||
            string.IsNullOrWhiteSpace(manifest.PhantoonColorsSha256) ||
            string.IsNullOrWhiteSpace(manifest.ChozoAndTubeColorsSha256) ||
            string.IsNullOrWhiteSpace(manifest.SporeSpawnColorsSha256) ||
            string.IsNullOrWhiteSpace(manifest.DachoraColorsSha256) ||
            string.IsNullOrWhiteSpace(manifest.ShitroidColorsSha256) ||
            string.IsNullOrWhiteSpace(manifest.BabyMetroidCutsceneColorsSha256) ||
            string.IsNullOrWhiteSpace(manifest.BotwoonColorsSha256) ||
            string.IsNullOrWhiteSpace(manifest.MotherBrainDeathColorsSha256) ||
            string.IsNullOrWhiteSpace(manifest.ZebetiteColorsSha256) ||
            string.IsNullOrWhiteSpace(manifest.NorfairRidleyColorsSha256) ||
            string.IsNullOrWhiteSpace(manifest.TourianStatueColorsSha256))
            throw new InvalidDataException($"Enemy tile manifest {manifestPath} does not describe this installation.");
        ValidateDefinitionIds(manifest.Entries.Keys);
        ushort[] expectedHeadPointers = KraidHeadInstructionDefinitions.All.ToArray()
            .Where(frame => frame.Kind == KraidHeadInstructionKind.Frame)
            .Select(frame => frame.Tilemap).Distinct().Order().ToArray();
        if (!manifest.KraidHeadsSha256.Keys.Order().SequenceEqual(expectedHeadPointers))
            throw new InvalidDataException("Enemy tile manifest omits or substitutes a Kraid head frame.");
        if (!manifest.TorizoInstructionTilesSha256.Keys.Order(StringComparer.Ordinal)
                .SequenceEqual(TorizoInstructionVramArtworkDefinitions.All.ToArray()
                    .Select(page => page.FileName).Order(StringComparer.Ordinal)))
            throw new InvalidDataException(
                "Enemy tile manifest omits or substitutes a Torizo instruction tile page.");
        if (!manifest.CeresEscapeTilesSha256.Keys.Order(StringComparer.Ordinal)
                .SequenceEqual(CeresEscapeTileArtworkDefinitions.All.ToArray()
                    .Select(page => page.FileName).Order(StringComparer.Ordinal)))
            throw new InvalidDataException(
                "Enemy tile manifest omits or substitutes a Ceres escape tile page.");

        var sheets = new Dictionary<ushort, RoomCharacterAtlas>();
        var palettes = new Dictionary<ushort, EnemyPaletteSheet>();
        var dmaSources = new Dictionary<ushort, int>();
        foreach ((ushort definitionPointer, EnemyTileFileEntry entry) in manifest.Entries)
        {
            RoomEnemyDefinition definition = RoomEnemyDefinitionCatalog.Get(definitionPointer);
            if (entry.NativeByteCount != (definition.TileDataSize & 0x7fff))
                throw new InvalidDataException(
                    $"Enemy ${definitionPointer:X4} manifest DMA size differs from retail.");
            dmaSources.Add(definitionPointer, definition.TileDataAddress);
            RoomCharacterAtlasFormat.ValidateTileCount(entry.NativeByteCount);
            string fileName = EnemyTileArtworkFormat.FileName(definitionPointer);
            string stockPath = Path.Combine(stockDirectory, fileName);
            byte[] stock = File.ReadAllBytes(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock)), entry.Sha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock enemy tile PNG {stockPath} failed its manifest hash.");
            string? overridePath = overrideDirectory is null ? null : Path.Combine(overrideDirectory, fileName);
            string selectedPath = overridePath is not null && File.Exists(overridePath) ? overridePath : stockPath;
            try
            {
                byte[] selected = selectedPath == stockPath ? stock : File.ReadAllBytes(selectedPath);
                sheets.Add(definitionPointer, RoomCharacterAtlas.Load(
                    new MemoryStream(selected, writable: false), entry.NativeByteCount));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid enemy tile PNG {selectedPath}: {error.Message}", error);
            }
            string paletteName = EnemyTileArtworkFormat.PaletteFileName(definitionPointer);
            string stockPalettePath = Path.Combine(stockDirectory, paletteName);
            byte[] stockPalette = File.ReadAllBytes(stockPalettePath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stockPalette)), entry.PaletteSha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock enemy palette {stockPalettePath} failed its manifest hash.");
            string? paletteOverridePath = overrideDirectory is null ? null : Path.Combine(overrideDirectory, paletteName);
            string selectedPalettePath = paletteOverridePath is not null && File.Exists(paletteOverridePath)
                ? paletteOverridePath : stockPalettePath;
            try
            {
                byte[] selected = selectedPalettePath == stockPalettePath
                    ? stockPalette : File.ReadAllBytes(selectedPalettePath);
                palettes.Add(definitionPointer, EnemyPaletteSheet.Load(
                    new MemoryStream(selected, writable: false)));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid enemy palette {selectedPalettePath}: {error.Message}", error);
            }
        }
        byte[] first = ReadCrocomireAsset(CrocomireMeltingArtworkFormat.FirstFileName,
            manifest.CrocomireFirstSha256);
        byte[] second = ReadCrocomireAsset(CrocomireMeltingArtworkFormat.SecondFileName,
            manifest.CrocomireSecondSha256);
        byte[] firstTilemap = ReadCrocomireAsset(
            CrocomireMeltingArtworkFormat.FirstTilemapFileName,
            manifest.CrocomireFirstTilemapSha256);
        byte[] secondTilemap = ReadCrocomireAsset(
            CrocomireMeltingArtworkFormat.SecondTilemapFileName,
            manifest.CrocomireSecondTilemapSha256);
        byte[] skeletonPng = ReadStockOrOverride(
            CrocomireSkeletonTransferDefinitions.FileName,
            manifest.CrocomireSkeletonSha256);
        CrocomireSkeletonArtwork skeleton;
        try
        {
            skeleton = CrocomireSkeletonArtwork.Load(
                new MemoryStream(skeletonPng, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Crocomire skeleton artwork in {overrideDirectory ?? stockDirectory}: " +
                error.Message, error);
        }
        CrocomireMeltingArtwork crocomire;
        try
        {
            crocomire = CrocomireMeltingArtwork.Load(
                new MemoryStream(first, writable: false),
                new MemoryStream(second, writable: false),
                new MemoryStream(firstTilemap, writable: false),
                new MemoryStream(secondTilemap, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Crocomire melt artwork in {overrideDirectory ?? stockDirectory}: {error.Message}", error);
        }
        EnemySpritemapCatalog spritemaps;
        try
        {
            // Older overrides cannot know the later Skultera, Waver, and
            // Skree/Metaree/Zoa, Pipe Bug, Fake Kraid, Kraid nail, Owtch, Stoke, and Ripper identities.
            // Merge only their validated frames onto verified current stock content,
            // preserving existing user edits through an extraction upgrade. Stock is
            // compiled once; without an override that compiled stock is the selection.
            byte[] stockComposition = ReadVerifiedStock(
                EnemySpritemapDefinitions.FileName, manifest.EnemyCompositionsSha256);
            EnemySpritemapCatalog stockSpritemaps = EnemySpritemapCatalog.Load(
                new MemoryStream(stockComposition, writable: false));
            spritemaps = OverridePath(EnemySpritemapDefinitions.FileName) is { } compositionOverride
                ? EnemySpritemapCatalog.Load(
                    new MemoryStream(File.ReadAllBytes(compositionOverride), writable: false), stockSpritemaps)
                : stockSpritemaps;
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid enemy compositions in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
        EnemyProjectileSpritemapCatalog projectileSpritemaps;
        try
        {
            EnemyProjectileSpritemapCatalog stockProjectiles =
                EnemyProjectileSpritemapCatalog.Load(new MemoryStream(ReadVerifiedStock(
                    EnemyProjectileSpritemapDefinitions.FileName,
                    manifest.EnemyProjectileCompositionsSha256), writable: false));
            projectileSpritemaps = OverridePath(EnemyProjectileSpritemapDefinitions.FileName) is { } projectileOverride
                ? EnemyProjectileSpritemapCatalog.Load(
                    new MemoryStream(File.ReadAllBytes(projectileOverride), writable: false), stockProjectiles)
                : stockProjectiles;
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                "Invalid installed enemy-projectile compositions.", error);
        }
        EnemyExtendedFrameCatalog extendedFrames;
        try
        {
            // V1 has walking frames and v2 adds wall frames. Overlay either
            // validated legacy file onto complete, hash-checked v3 stock so
            // newly added Ninja art cannot discard the user's existing edits.
            byte[] stockExtendedJson = ReadVerifiedStock(
                EnemyExtendedFrameDefinitions.FileName, manifest.EnemyExtendedCompositionsSha256);
            EnemyExtendedFrameCatalog stockExtended = EnemyExtendedFrameCatalog.Load(
                new MemoryStream(stockExtendedJson, writable: false));
            extendedFrames = OverridePath(EnemyExtendedFrameDefinitions.FileName) is { } extendedOverride
                ? EnemyExtendedFrameCatalog.Load(
                    new MemoryStream(File.ReadAllBytes(extendedOverride), writable: false), stockExtended)
                : stockExtended;
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid extended enemy compositions in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
        PhantoonBg2FrameCatalog phantoonBg2Frames;
        try
        {
            byte[] selected = ReadStockOrOverride(
                PhantoonBg2FrameDefinitions.FileName,
                manifest.PhantoonBg2FramesSha256);
            phantoonBg2Frames = PhantoonBg2FrameCatalog.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Phantoon BG2 frames in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
        DraygonBg2FrameCatalog draygonBg2Frames;
        try
        {
            byte[] selected = ReadStockOrOverride(
                DraygonBg2FrameDefinitions.FileName,
                manifest.DraygonBg2FramesSha256);
            draygonBg2Frames = DraygonBg2FrameCatalog.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Draygon BG2 frames in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
        CrocomireBg2FrameCatalog crocomireBg2Frames;
        try
        {
            byte[] selected = ReadStockOrOverride(
                CrocomireBg2FrameDefinitions.FileName,
                manifest.CrocomireBg2FramesSha256);
            crocomireBg2Frames = CrocomireBg2FrameCatalog.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Crocomire BG2 frames in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
        MotherBrainBodyBg2FrameCatalog motherBrainBodyBg2Frames;
        try
        {
            motherBrainBodyBg2Frames = MotherBrainBodyBg2FrameCatalog.Load(new MemoryStream(
                ReadStockOrOverride(MotherBrainBodyVisualDefinitions.Bg2FileName,
                    manifest.MotherBrainBodyBg2FramesSha256), writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Mother Brain body BG2 frames in {overrideDirectory ?? stockDirectory}: {error.Message}", error);
        }
        var gunshipFrames = new RoomCharacterAtlas[
            GunshipLiftoffTransferDefinitions.Frames.Length];
        for (int index = 0; index < gunshipFrames.Length; index++)
        {
            if (!manifest.GunshipLiftoffSha256.TryGetValue(index,
                    out string? expectedHash) || string.IsNullOrWhiteSpace(expectedHash))
                throw new InvalidDataException(
                    $"Enemy tile manifest omits gunship takeoff frame {index}.");
            string fileName = EnemyTileArtworkFormat.GunshipLiftoffFileName(index);
            byte[] selected = ReadStockOrOverride(fileName, expectedHash);
            try
            {
                gunshipFrames[index] = RoomCharacterAtlas.Load(
                    new MemoryStream(selected, writable: false),
                    GunshipLiftoffTransferDefinitions.ByteCount);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid gunship takeoff PNG {fileName}: {error.Message}", error);
            }
        }
        var gunshipLiftoff = new GunshipLiftoffArtworkCatalog(gunshipFrames);
        var torizoPages = new RoomCharacterAtlas[
            TorizoInstructionVramArtworkDefinitions.All.Length];
        for (int index = 0; index < torizoPages.Length; index++)
        {
            TorizoInstructionTileSheetDefinition page =
                TorizoInstructionVramArtworkDefinitions.All[index];
            if (!manifest.TorizoInstructionTilesSha256.TryGetValue(page.FileName,
                    out string? expectedHash) || string.IsNullOrWhiteSpace(expectedHash))
                throw new InvalidDataException(
                    $"Enemy tile manifest omits Torizo page {page.FileName}.");
            byte[] selected = ReadStockOrOverride(page.FileName, expectedHash);
            try
            {
                torizoPages[index] = RoomCharacterAtlas.Load(
                    new MemoryStream(selected, writable: false), page.ByteCount);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid Torizo instruction PNG {page.FileName}: {error.Message}",
                    error);
            }
        }
        var torizoInstructionVram = new TorizoInstructionVramArtwork(torizoPages);
        var ceresEscapePages = new RoomCharacterAtlas[
            CeresEscapeTileArtworkDefinitions.All.Length];
        for (int index = 0; index < ceresEscapePages.Length; index++)
        {
            CeresEscapeTileSheetDefinition page =
                CeresEscapeTileArtworkDefinitions.All[index];
            if (!manifest.CeresEscapeTilesSha256.TryGetValue(page.FileName,
                    out string? expectedHash) || string.IsNullOrWhiteSpace(expectedHash))
                throw new InvalidDataException(
                    $"Enemy tile manifest omits Ceres escape page {page.FileName}.");
            byte[] selected = ReadStockOrOverride(page.FileName, expectedHash);
            try
            {
                ceresEscapePages[index] = RoomCharacterAtlas.Load(
                    new MemoryStream(selected, writable: false), page.ByteCount);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid Ceres escape PNG {page.FileName}: {error.Message}",
                    error);
            }
        }
        var ceresEscapeTiles = new CeresEscapeTileArtwork(ceresEscapePages);
        CeresEscapeOverlayTilemapCatalog ceresEscapeOverlay;
        try
        {
            byte[] selected = ReadStockOrOverride(
                CeresEscapeOverlayTilemapDefinitions.FileName,
                manifest.CeresEscapeOverlaySha256);
            ceresEscapeOverlay = CeresEscapeOverlayTilemapCatalog.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                "Invalid installed Ceres escape overlay tilemaps.", error);
        }
        RoomCharacterAtlas motherBrainCorpse;
        try
        {
            byte[] selected = ReadStockOrOverride(
                MotherBrainCorpseArtworkDefinitions.FileName,
                manifest.MotherBrainCorpseSha256);
            motherBrainCorpse = RoomCharacterAtlas.Load(
                new MemoryStream(selected, writable: false),
                MotherBrainCorpseArtworkDefinitions.ByteCount);
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Mother Brain corpse PNG {MotherBrainCorpseArtworkDefinitions.FileName}: {error.Message}",
                error);
        }
        RoomCharacterAtlas motherBrainEscapeText;
        try
        {
            byte[] selected = ReadStockOrOverride(
                MotherBrainEscapeTextArtworkDefinitions.FileName,
                manifest.MotherBrainEscapeTextSha256);
            motherBrainEscapeText = RoomCharacterAtlas.Load(
                new MemoryStream(selected, writable: false),
                MotherBrainEscapeTextArtworkDefinitions.ByteCount);
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Mother Brain escape-text PNG {MotherBrainEscapeTextArtworkDefinitions.FileName}: {error.Message}",
                error);
        }
        var motherBrainSpecialSheets = new Dictionary<int, RoomCharacterAtlas>();
        foreach (MotherBrainSpecialSpriteSheetDefinition sheet in
                 MotherBrainSpecialSpriteArtworkDefinitions.All)
        {
            if (!manifest.MotherBrainSpecialSpritesSha256.TryGetValue(
                    sheet.SourceAddress, out string? expectedHash) ||
                string.IsNullOrWhiteSpace(expectedHash))
                throw new InvalidDataException(
                    $"Enemy tile manifest omits {sheet.FileName}.");
            try
            {
                byte[] selected = ReadStockOrOverride(sheet.FileName, expectedHash);
                motherBrainSpecialSheets.Add(sheet.SourceAddress,
                    RoomCharacterAtlas.Load(
                        new MemoryStream(selected, writable: false), sheet.ByteCount));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid Mother Brain sprite PNG {sheet.FileName}: {error.Message}",
                    error);
            }
        }
        var motherBrainSpecialSprites = new MotherBrainSpecialSpriteArtworkCatalog(
            motherBrainSpecialSheets);
        RoomBackgroundTilemapAtlas upperKraid = LoadKraidTilemap(
            KraidBackgroundArtworkFormat.UpperFileName, manifest.KraidUpperSha256);
        RoomBackgroundTilemapAtlas lowerKraid = LoadKraidTilemap(
            KraidBackgroundArtworkFormat.LowerFileName, manifest.KraidLowerSha256);
        var kraidHeads = new Dictionary<ushort, KraidHeadTilemapAtlas>();
        foreach ((ushort pointer, string sha256) in manifest.KraidHeadsSha256)
        {
            string fileName = KraidBackgroundArtworkFormat.HeadFileName(pointer);
            byte[] selected = ReadStockOrOverride(fileName, sha256);
            try
            {
                kraidHeads.Add(pointer, KraidHeadTilemapAtlas.Load(
                    new MemoryStream(selected, writable: false)));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid Kraid head tilemap {fileName}: {error.Message}", error);
            }
        }
        string roomBackgroundName = KraidBackgroundArtworkFormat.RoomBackgroundFileName;
        byte[] roomBackgroundPng = ReadStockOrOverride(
            roomBackgroundName, manifest.KraidRoomBackgroundSha256);
        RoomCharacterAtlas roomBackground;
        try
        {
            roomBackground = RoomCharacterAtlas.Load(
                new MemoryStream(roomBackgroundPng, writable: false),
                KraidBackgroundRomData.RoomBackgroundTileBytes);
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Kraid room-background PNG {roomBackgroundName}: {error.Message}", error);
        }
        KraidColorCatalog kraidColors;
        try
        {
            byte[] selected = ReadStockOrOverride(KraidColorFormat.FileName,
                manifest.KraidColorsSha256);
            kraidColors = KraidColorCatalog.Load(new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Kraid color file {KraidColorFormat.FileName}: {error.Message}", error);
        }
        CeresDoorVisualCatalog ceresDoorVisual;
        try
        {
            ceresDoorVisual = CeresDoorVisualFiles.Load(
                ReadStockOrOverride(CeresDoorVisualFormat.TilesFileName,
                    manifest.CeresDoorTilesSha256),
                ReadStockOrOverride(CeresDoorVisualFormat.ColorsFileName,
                    manifest.CeresDoorColorsSha256));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException("Invalid installed Ceres-door visuals.", error);
        }
        MagdollitePaletteCycle magdollitePaletteCycle;
        try
        {
            byte[] selected = ReadStockOrOverride(MagdollitePaletteCycleFormat.FileName,
                manifest.MagdollitePaletteCycleSha256);
            magdollitePaletteCycle = MagdollitePaletteCycle.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Magdollite palette cycle in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
        WorkRobotPaletteCycle workRobotPaletteCycle;
        try
        {
            byte[] selected = ReadStockOrOverride(WorkRobotPaletteCycleFormat.FileName,
                manifest.WorkRobotPaletteCycleSha256);
            workRobotPaletteCycle = WorkRobotPaletteCycle.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Work Robot palette cycle in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
        CrocomireColorCatalog crocomireColors;
        try
        {
            byte[] selected = ReadStockOrOverride(CrocomireColorFormat.FileName,
                manifest.CrocomireColorsSha256);
            crocomireColors = CrocomireColorCatalog.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Crocomire colors in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
        DraygonColorCatalog draygonColors;
        try
        {
            byte[] selected = ReadStockOrOverride(DraygonColorFormat.FileName,
                manifest.DraygonColorsSha256);
            draygonColors = DraygonColorCatalog.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Draygon colors in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
        PhantoonColorCatalog phantoonColors;
        try
        {
            byte[] selected = ReadStockOrOverride(PhantoonColorFormat.FileName,
                manifest.PhantoonColorsSha256);
            phantoonColors = PhantoonColorCatalog.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Phantoon colors in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
        ChozoAndTubeColorCatalog chozoAndTubeColors;
        try
        {
            byte[] selected = ReadStockOrOverride(ChozoAndTubeColorFormat.FileName,
                manifest.ChozoAndTubeColorsSha256);
            chozoAndTubeColors = ChozoAndTubeColorCatalog.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Chozo/tube colors in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
        SporeSpawnColorCatalog sporeSpawnColors;
        try
        {
            byte[] selected = ReadStockOrOverride(SporeSpawnColorFormat.FileName,
                manifest.SporeSpawnColorsSha256);
            sporeSpawnColors = SporeSpawnColorCatalog.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Spore Spawn colors in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
        DachoraColorCatalog dachoraColors;
        try
        {
            byte[] selected = ReadStockOrOverride(DachoraColorFormat.FileName,
                manifest.DachoraColorsSha256);
            dachoraColors = DachoraColorCatalog.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Dachora colors in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
        ShitroidColorCatalog shitroidColors;
        try
        {
            byte[] selected = ReadStockOrOverride(ShitroidColorFormat.FileName,
                manifest.ShitroidColorsSha256);
            shitroidColors = ShitroidColorCatalog.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Shitroid colors in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
        BabyMetroidCutsceneColorCatalog babyMetroidCutsceneColors;
        try
        {
            byte[] selected = ReadStockOrOverride(
                BabyMetroidCutsceneColorFormat.FileName,
                manifest.BabyMetroidCutsceneColorsSha256);
            babyMetroidCutsceneColors = BabyMetroidCutsceneColorCatalog.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid cutscene Baby colors in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
        BotwoonColorCatalog botwoonColors;
        try
        {
            byte[] selected = ReadStockOrOverride(BotwoonColorFormat.FileName,
                manifest.BotwoonColorsSha256);
            botwoonColors = BotwoonColorCatalog.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Botwoon colors in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
        MotherBrainDeathColorCatalog motherBrainDeathColors;
        try
        {
            byte[] selected = ReadStockOrOverride(
                MotherBrainDeathColorFormat.FileName,
                manifest.MotherBrainDeathColorsSha256);
            motherBrainDeathColors = MotherBrainDeathColorCatalog.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Mother Brain death colors in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
        ZebetiteColorCatalog zebetiteColors;
        try
        {
            byte[] selected = ReadStockOrOverride(ZebetiteColorFormat.FileName,
                manifest.ZebetiteColorsSha256);
            zebetiteColors = ZebetiteColorCatalog.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Zebetite colors in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
        NorfairRidleyColorCatalog norfairRidleyColors;
        try
        {
            byte[] selected = ReadStockOrOverride(NorfairRidleyColorFormat.FileName,
                manifest.NorfairRidleyColorsSha256);
            norfairRidleyColors = NorfairRidleyColorCatalog.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Norfair Ridley colors in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
        TourianStatueColorCatalog tourianStatueColors;
        try
        {
            byte[] selected = ReadStockOrOverride(TourianStatueColorFormat.FileName,
                manifest.TourianStatueColorsSha256);
            tourianStatueColors = TourianStatueColorCatalog.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Tourian statue colors in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
        var auxiliaryColors = EnemyAuxiliaryColorCatalog.Load(new MemoryStream(
            ReadStockOrOverride(EnemyAuxiliaryColorFormat.FileName, manifest.AuxiliaryColorsSha256), writable: false));
        return EnemyTileArtworkCatalog.FromInstalledArtwork(sheets, palettes, crocomire,
            spritemaps, extendedFrames, new KraidBackgroundArtwork(upperKraid, lowerKraid,
                kraidHeads, roomBackground), kraidColors, gunshipLiftoff, ceresDoorVisual,
            dmaSources, projectileSpritemaps, magdollitePaletteCycle,
            workRobotPaletteCycle, crocomireColors, draygonColors, phantoonColors,
            chozoAndTubeColors, sporeSpawnColors, dachoraColors, shitroidColors,
            babyMetroidCutsceneColors, botwoonColors, motherBrainDeathColors,
            zebetiteColors, norfairRidleyColors, tourianStatueColors,
            phantoonBg2Frames, draygonBg2Frames, motherBrainCorpse,
            motherBrainEscapeText, motherBrainSpecialSprites, skeleton,
            crocomireBg2Frames, torizoInstructionVram, ceresEscapeTiles,
            ceresEscapeOverlay, auxiliaryColors, motherBrainBodyBg2Frames);

        RoomBackgroundTilemapAtlas LoadKraidTilemap(string fileName, string expectedSha256)
        {
            byte[] selected = ReadStockOrOverride(fileName, expectedSha256);
            try
            {
                return RoomBackgroundTilemapAtlas.Load(
                    new MemoryStream(selected, writable: false),
                    KraidBackgroundRomData.DecompressedTilemapBytes);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid Kraid BG2 tilemap {fileName}: {error.Message}", error);
            }
        }

        byte[] ReadCrocomireAsset(string fileName, string expectedSha256)
            => ReadStockOrOverride(fileName, expectedSha256);

        byte[] ReadStockOrOverride(string fileName, string expectedSha256)
        {
            byte[] stock = ReadVerifiedStock(fileName, expectedSha256);
            return OverridePath(fileName) is { } overridePath ? File.ReadAllBytes(overridePath) : stock;
        }

        byte[] ReadVerifiedStock(string fileName, string expectedSha256)
        {
            string stockPath = Path.Combine(stockDirectory, fileName);
            byte[] stock = File.ReadAllBytes(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock)), expectedSha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock enemy asset {stockPath} failed its manifest hash.");
            return stock;
        }

        string? OverridePath(string fileName)
        {
            string? overridePath = overrideDirectory is null ? null :
                Path.Combine(overrideDirectory, fileName);
            return overridePath is not null && File.Exists(overridePath) ? overridePath : null;
        }
    }

    /// <summary>Validates and compiles the complete installed enemy-art catalog without selecting any player overrides.</summary>
    /// <param name="stockDirectory">Directory containing the enemy manifest and all ordinary and supplemental stock resources.</param>
    /// <remarks>Checks the supported cartridge identity, retail definition coverage, manifest hashes, and resource schemas without cartridge access or file writes.</remarks>
    /// <exception cref="InvalidDataException">Stock provenance, coverage, dimensions, or an artwork document is invalid.</exception>
    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    /// <summary>Checks that the exported definition pointers match the pinned retail set.</summary>
    /// <param name="pointers">Native enemy graphics definition pointers in the catalog.</param>
    private static void ValidateDefinitionIds(IEnumerable<ushort> pointers)
    {
        string joined = string.Join(",", pointers.Order().Select(pointer => $"{pointer:X4}"));
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.ASCII.GetBytes(joined)));
        if (!string.Equals(hash, EnemyTileArtworkFormat.RetailDefinitionIdsSha256,
                StringComparison.Ordinal))
            throw new InvalidDataException("Enemy tile manifest omits or substitutes a retail graphics definition.");
    }

    /// <summary>Reconstructs one native Crocomire melt-copy pass and encodes its planar tiles as PNG.</summary>
    /// <param name="bus">Cartridge address space supplying source bytes.</param>
    /// <param name="pass">Native source-bank and copy-range description.</param>
    /// <param name="encodedByteCount">Planar buffer size represented by the exported image.</param>
    /// <returns>PNG bytes that round-trip to the reconstructed planar data.</returns>
    private static byte[] ExtractCrocomireMelt(ISnesAddressSpace bus,
        CrocomireMeltingPass pass, int encodedByteCount)
    {
        var planar = new byte[encodedByteCount];
        foreach (CrocomireMeltingCopy copy in pass.Copies)
        {
            int destination = copy.DestinationWord - 0x4000;
            for (int index = 0; index < (pass.WordsToCopy + 1) * 2; index++)
            {
                planar[destination + index] = bus.ReadCartridgeByte(
                    (pass.SourceBank << 16) | unchecked((ushort)(copy.SourceWord + index)));
            }
        }
        int tileCount = RoomCharacterAtlasFormat.ValidateTileCount(encodedByteCount);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4,
            Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount),
            out int width, out int height);
        using var png = new MemoryStream();
        IndexedPng.Write(png, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
        byte[] encoded = png.ToArray();
        RoomCharacterAtlas roundtrip = RoomCharacterAtlas.Load(
            new MemoryStream(encoded, writable: false), encodedByteCount);
        if (!roundtrip.Transfer.Span.SequenceEqual(planar))
            throw new InvalidDataException(
                $"Crocomire melt pass ${pass.HeaderOffset:X4} PNG changed native graphics bytes.");
        return encoded;
    }

    /// <summary>Decompresses the native Kraid tilemap and encodes its editable document.</summary>
    /// <param name="bus">Cartridge address space containing compressed tilemap data.</param>
    /// <param name="sourceAddress">Native compressed source address.</param>
    /// <returns>Serialized tilemap document bytes.</returns>
    private static byte[] ExtractKraidTilemap(ISnesAddressSpace bus, int sourceAddress)
    {
        byte[] native = RomDataReader.Decompress(CartridgeImportSource.Require(bus), sourceAddress,
            KraidBackgroundRomData.DecompressedTilemapBytes);
        return RoomBackgroundTilemapExtractor.Encode(native);
    }

    /// <summary>Exports Kraid's room-background graphics as a lossless indexed PNG.</summary>
    /// <param name="bus">Cartridge address space containing the fixed-bank planar tiles.</param>
    /// <returns>PNG bytes whose decoded transfer matches the native graphics.</returns>
    private static byte[] ExtractKraidRoomBackground(ISnesAddressSpace bus)
    {
        byte[] planar = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            KraidBackgroundRomData.RoomBackgroundTileAddress,
            KraidBackgroundRomData.RoomBackgroundTileBytes);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4,
            KraidBackgroundRomData.RoomBackgroundTileBytes /
                RoomCharacterAtlasFormat.BytesPerTile,
            out int width, out int height);
        using var png = new MemoryStream();
        IndexedPng.Write(png, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
        byte[] encoded = png.ToArray();
        RoomCharacterAtlas roundtrip = RoomCharacterAtlas.Load(
            new MemoryStream(encoded, writable: false),
            KraidBackgroundRomData.RoomBackgroundTileBytes);
        if (!roundtrip.Transfer.Span.SequenceEqual(planar))
            throw new InvalidDataException("Kraid room-background PNG changed native tile bytes.");
        return encoded;
    }

    /// <summary>Encodes one Mother Brain special-sprite sheet as a round-trippable indexed PNG.</summary>
    /// <param name="bus">Cartridge address space containing the sheet graphics.</param>
    /// <param name="sheet">Native source address, byte count, and output filename.</param>
    /// <returns>PNG bytes preserving the source planar transfer.</returns>
    private static byte[] ExtractMotherBrainSpecialSpritePng(ISnesAddressSpace bus,
        MotherBrainSpecialSpriteSheetDefinition sheet)
    {
        byte[] native = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            sheet.SourceAddress, sheet.ByteCount);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(native, 4,
            RoomCharacterAtlasFormat.TileColumns, out int width, out int height);
        using var output = new MemoryStream();
        IndexedPng.Write(output, width, height, pixels,
            SnesGraphics.DiagnosticPalette(16));
        byte[] png = output.ToArray();
        RoomCharacterAtlas roundtrip = RoomCharacterAtlas.Load(
            new MemoryStream(png, writable: false), sheet.ByteCount);
        if (!roundtrip.Transfer.Span.SequenceEqual(native))
            throw new InvalidDataException(
                $"Mother Brain {sheet.FileName} PNG changed native pixels during extraction.");
        return png;
    }

    /// <summary>Extracts one Torizo instruction-page tile sheet using the shared indexed-page codec.</summary>
    /// <param name="bus">Cartridge address space containing the page graphics.</param>
    /// <param name="page">Source address, byte count, and page filename.</param>
    /// <returns>Encoded page PNG bytes.</returns>
    private static byte[] ExtractTorizoInstructionPage(ISnesAddressSpace bus,
        TorizoInstructionTileSheetDefinition page) =>
        IndexedTilePageExtractor.Extract(bus, page.SourceAddress, page.ByteCount,
            $"Torizo instruction page {page.FileName}");

    /// <summary>Serializes the five native Kraid palette groups into the editable color document.</summary>
    /// <param name="bus">Cartridge address space containing Kraid's palette words.</param>
    /// <returns>Serialized RGB5 palette document bytes.</returns>
    private static byte[] ExtractKraidColors(ISnesAddressSpace bus) =>
        KraidColorCatalog.Write(new KraidColorDocument
        {
            Version = KraidColorFormat.Version,
            RoomBackdrop = ReadKraidColors(bus, KraidPaletteSource.RoomBackdrop),
            InitialTarget = ReadKraidColors(bus, KraidPaletteSource.InitialTarget),
            Health = ReadKraidColors(bus, KraidPaletteSource.Health),
            Secondary = ReadKraidColors(bus, KraidPaletteSource.Secondary),
            DeathArm = ReadKraidColors(bus, KraidPaletteSource.DeathArm),
        });

    /// <summary>Reads and converts one native Kraid RGB5 palette group.</summary>
    /// <param name="bus">Cartridge address space containing the palette words.</param>
    /// <param name="source">Palette group whose address and color count are defined by the cartridge catalog.</param>
    /// <returns>Decoded RGB5 colors in native order.</returns>
    private static PaletteRgb5[] ReadKraidColors(ISnesAddressSpace bus,
        KraidPaletteSource source)
    {
        int count = KraidPaletteRomData.ColorCount(source);
        byte[] native = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            KraidPaletteRomData.SourceAddress(source), count * sizeof(ushort));
        var colors = new PaletteRgb5[count];
        for (int index = 0; index < count; index++)
        {
            ushort word = BinaryPrimitives.ReadUInt16LittleEndian(native.AsSpan(index * 2));
            if ((word & 0x8000) != 0)
                throw new InvalidDataException(
                    $"Kraid {source} color {index} has an unrepresentable high bit.");
            colors[index] = new PaletteRgb5
            {
                Red = word & 31,
                Green = word >> 5 & 31,
                Blue = word >> 10 & 31,
            };
        }
        return colors;
    }

    /// <summary>Converts the native Crocomire melt background tilemap into its visual JSON form.</summary>
    /// <param name="bus">Cartridge address space containing tilemap words and the terminator.</param>
    /// <param name="sourceAddress">Native tilemap source address.</param>
    /// <returns>Serialized tilemap document bytes.</returns>
    private static byte[] ExtractCrocomireMeltTilemap(ISnesAddressSpace bus,
        int sourceAddress)
    {
        var cells = new CrocomireMeltingTilemapCell[
            CrocomireMeltingArtworkFormat.TilemapCellCount];
        for (int index = 0; index < cells.Length; index++)
        {
            int address = sourceAddress + index * 2;
            ushort raw = (ushort)(bus.ReadCartridgeByte(address) | bus.ReadCartridgeByte(address + 1) << 8);
            var word = new SnesBgTilemapWord(raw);
            cells[index] = new CrocomireMeltingTilemapCell
            {
                TileIndex = word.CharacterIndex,
                Palette = word.PaletteIndex,
                Priority = word.HasPriority,
                FlipX = word.FlipHorizontally,
                FlipY = word.FlipVertically,
            };
        }
        int terminator = sourceAddress + cells.Length * 2;
        if ((ushort)(bus.ReadCartridgeByte(terminator) | bus.ReadCartridgeByte(terminator + 1) << 8) != 0xffff)
            throw new InvalidDataException(
                $"Crocomire melt tilemap ${sourceAddress:X6} lacks its native terminator.");
        using var json = new MemoryStream();
        CrocomireMeltingArtwork.WriteTilemap(json, new CrocomireMeltingTilemapDocument
        {
            Version = CrocomireMeltingArtworkFormat.TilemapVersion,
            Width = CrocomireMeltingArtworkFormat.TilemapWidth,
            Height = CrocomireMeltingArtworkFormat.TilemapHeight,
            Cells = cells,
        });
        return json.ToArray();
    }

    /// <summary>Stock provenance and per-resource hashes for ordinary and supplemental enemy artwork.</summary>
    /// <param name="Version">Enemy artwork manifest schema version.</param>
    /// <param name="SourceCartridgeSha256">SHA-256 of the extraction cartridge revision.</param>
    /// <param name="Entries">Ordinary enemy sheets keyed by native definition pointer.</param>
    /// <param name="CrocomireFirstSha256">Hash for the first Crocomire melt sheet.</param>
    /// <param name="CrocomireSecondSha256">Hash for the second Crocomire melt sheet.</param>
    /// <param name="CrocomireFirstTilemapSha256">Hash for the first Crocomire melt tilemap.</param>
    /// <param name="CrocomireSecondTilemapSha256">Hash for the second Crocomire melt tilemap.</param>
    /// <param name="EnemyCompositionsSha256">Hash for enemy sprite composition definitions.</param>
    /// <param name="EnemyProjectileCompositionsSha256">Hash for enemy projectile compositions.</param>
    /// <param name="EnemyExtendedCompositionsSha256">Hash for extended enemy composition definitions.</param>
    /// <param name="PhantoonBg2FramesSha256">Hash for Phantoon background-layer frames.</param>
    /// <param name="DraygonBg2FramesSha256">Hash for Draygon background-layer frames.</param>
    /// <param name="CrocomireBg2FramesSha256">Hash for Crocomire background-layer frames.</param>
    /// <param name="MotherBrainBodyBg2FramesSha256">Hash for Mother Brain body background-layer frames.</param>
    /// <param name="GunshipLiftoffSha256">Hashes for Gunship liftoff resources keyed by frame.</param>
    /// <param name="TorizoInstructionTilesSha256">Hashes for Torizo instruction-page sheets.</param>
    /// <param name="CeresEscapeTilesSha256">Hashes for Ceres escape tile sheets.</param>
    /// <param name="CeresEscapeOverlaySha256">Hash for the Ceres escape overlay.</param>
    /// <param name="MotherBrainCorpseSha256">Hash for Mother Brain corpse artwork.</param>
    /// <param name="MotherBrainEscapeTextSha256">Hash for Mother Brain escape text artwork.</param>
    /// <param name="MotherBrainSpecialSpritesSha256">Hashes for Mother Brain special-sprite sheets.</param>
    /// <param name="KraidUpperSha256">Hash for Kraid's upper body sheet.</param>
    /// <param name="KraidLowerSha256">Hash for Kraid's lower body sheet.</param>
    /// <param name="KraidHeadsSha256">Hashes for Kraid head variants keyed by native identity.</param>
    /// <param name="KraidRoomBackgroundSha256">Hash for Kraid room-background tiles.</param>
    /// <param name="KraidColorsSha256">Hash for Kraid's editable palette document.</param>
    /// <param name="CeresDoorTilesSha256">Hash for Ceres door tiles.</param>
    /// <param name="CeresDoorColorsSha256">Hash for Ceres door palette colors.</param>
    /// <param name="MagdollitePaletteCycleSha256">Hash for Magdollite palette-cycle data.</param>
    /// <param name="WorkRobotPaletteCycleSha256">Hash for Work Robot palette-cycle data.</param>
    /// <param name="CrocomireColorsSha256">Hash for Crocomire palette data.</param>
    /// <param name="DraygonColorsSha256">Hash for Draygon palette data.</param>
    /// <param name="PhantoonColorsSha256">Hash for Phantoon palette data.</param>
    /// <param name="ChozoAndTubeColorsSha256">Hash for Chozo and glass-tube palette data.</param>
    /// <param name="SporeSpawnColorsSha256">Hash for Spore Spawn palette data.</param>
    /// <param name="DachoraColorsSha256">Hash for Dachora palette data.</param>
    /// <param name="ShitroidColorsSha256">Hash for Shitroid palette data.</param>
    /// <param name="BabyMetroidCutsceneColorsSha256">Hash for Baby Metroid cutscene palette data.</param>
    /// <param name="BotwoonColorsSha256">Hash for Botwoon palette data.</param>
    /// <param name="MotherBrainDeathColorsSha256">Hash for Mother Brain death palette data.</param>
    /// <param name="ZebetiteColorsSha256">Hash for Zebetite palette data.</param>
    /// <param name="NorfairRidleyColorsSha256">Hash for Norfair Ridley palette data.</param>
    /// <param name="TourianStatueColorsSha256">Hash for Tourian statue palette data.</param>
    /// <param name="CrocomireSkeletonSha256">Hash for Crocomire skeleton artwork.</param>
    /// <param name="AuxiliaryColorsSha256">Hash for auxiliary palette resources.</param>
    private sealed record EnemyTileManifest(int Version, string SourceCartridgeSha256,
        Dictionary<ushort, EnemyTileFileEntry> Entries,
        string CrocomireFirstSha256, string CrocomireSecondSha256,
        string CrocomireFirstTilemapSha256, string CrocomireSecondTilemapSha256,
        string EnemyCompositionsSha256, string EnemyProjectileCompositionsSha256,
        string EnemyExtendedCompositionsSha256,
        string PhantoonBg2FramesSha256,
        string DraygonBg2FramesSha256,
        string CrocomireBg2FramesSha256,
        string MotherBrainBodyBg2FramesSha256,
        Dictionary<int, string> GunshipLiftoffSha256,
        Dictionary<string, string> TorizoInstructionTilesSha256,
        Dictionary<string, string> CeresEscapeTilesSha256,
        string CeresEscapeOverlaySha256,
        string MotherBrainCorpseSha256,
        string MotherBrainEscapeTextSha256,
        Dictionary<int, string> MotherBrainSpecialSpritesSha256,
        string KraidUpperSha256, string KraidLowerSha256,
        Dictionary<ushort, string> KraidHeadsSha256,
        string KraidRoomBackgroundSha256,
        string KraidColorsSha256,
        string CeresDoorTilesSha256,
        string CeresDoorColorsSha256,
        string MagdollitePaletteCycleSha256,
        string WorkRobotPaletteCycleSha256,
        string CrocomireColorsSha256,
        string DraygonColorsSha256,
        string PhantoonColorsSha256,
        string ChozoAndTubeColorsSha256,
        string SporeSpawnColorsSha256,
        string DachoraColorsSha256,
        string ShitroidColorsSha256,
        string BabyMetroidCutsceneColorsSha256,
        string BotwoonColorsSha256,
        string MotherBrainDeathColorsSha256,
        string ZebetiteColorsSha256,
        string NorfairRidleyColorsSha256,
        string TourianStatueColorsSha256,
        string CrocomireSkeletonSha256,
        string AuxiliaryColorsSha256);

    /// <summary>Stock integrity metadata for one ordinary enemy graphics sheet.</summary>
    /// <param name="NativeByteCount">Native planar byte count represented by the PNG.</param>
    /// <param name="Sha256">SHA-256 of the stock PNG bytes.</param>
    /// <param name="PaletteSha256">SHA-256 of the indexed-color palette metadata.</param>
    private sealed record EnemyTileFileEntry(int NativeByteCount, string Sha256, string PaletteSha256);
}
