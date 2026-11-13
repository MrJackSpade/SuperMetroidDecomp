using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Bosses.Crocomire;
using SuperMetroid.AssetExtraction.Bosses.Torizo;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Buffers.Binary;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Bosses.Ridley;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;
using SuperMetroid.AssetExtraction.Areas.Tourian.Artwork;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Assets;
using SuperMetroid.AssetExtraction.Escape.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Botwoon.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Crocomire.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Draygon.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Kraid.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Phantoon.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;
using SuperMetroid.AssetExtraction.Bosses.SporeSpawn.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Extracts every retail room-enemy graphics sheet (#1636): the characters and tile pages, and the enemy, boss and area
/// colour documents only the import pre-colours sheets from (#1637), go to the component's import data
/// (<see cref="SuperMetroid.AssetExtraction.Installation.ImportDataDirectory"/>), which the sheets compose from and the
/// installer deletes before it publishes. So do the boss layouts and the enemy compositions only the import composes the
/// body and enemy sheets from (#1638), whose entries are the original's packed placements; the installation holds the
/// pictures they compose and the enemy frames' display bindings. So does the manifest of all of them, keyed by the
/// cartridge's enemy headers (#1693). The game reads only the documents it binds, the frames' displays and Kraid's eye
/// colours (<see cref="Load"/>), which are installed, and the player edits them in place.
/// </summary>
public static class EnemyTileArtworkFiles
{
    /// <summary>Serializer settings shared by the stock enemy-art manifest and its supplemental JSON resources.</summary>
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    /// <summary>Writes the complete retail enemy-art installation: character sheets, palettes, sprite compositions, and supplemental boss and cutscene resources.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying room-enemy graphics definitions and the associated artwork sources.</param>
    /// <param name="directory">Stock output directory, created if needed; existing resource and manifest files are overwritten. Its pages and the manifest go to its import-data directory.</param>
    /// <param name="sourceCartridgeSha256">Nonempty source-cartridge SHA-256 recorded in the manifest alongside each stock resource's hash.</param>
    /// <remarks>Deduplicates ordinary sheets by enemy-definition identity and verifies retail coverage. Indexed PNG colors are diagnostic; RGB5 palettes and visual JSON remain separate.</remarks>
    /// <exception cref="InvalidDataException">Native definitions, coverage, artwork sizes, or palette representation are invalid.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        string importData = SuperMetroid.AssetExtraction.Installation.ImportDataDirectory.Of(directory);
        Directory.CreateDirectory(importData);

        // The artwork the compiled graphics sets load, each header once: every member enemy's, and the artwork three sets load
        // for no enemy (#1686). A header a set loads only for what nothing shows, as the Reflec's, is not extracted.
        var entries = new Dictionary<int, EnemyTileFileEntry>();
        foreach (int headerWord in EnemyTileSourceDefinitions.All.Select(source => (int)EnemyDefinitionIds.HeaderPointer(source.DefinitionId))
                     .Concat(EnemyTileSourceDefinitions.Unplaced.Select(artwork => (int)EnemyHeaderWords.UnplacedArtworkHeaders.WordsByIdentity[artwork].Single()))
                     .Order())
        {
            int byteCount = SuperMetroid.AssetExtraction.Enemies.RoomEnemyDefinitionImporter.TileSetAt(bus, headerWord).ByteCount;
            int tileCount = RoomCharacterAtlasFormat.ValidateTileCount(byteCount);
            byte[] planar = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                SuperMetroid.AssetExtraction.Enemies.RoomEnemyDefinitionImporter.TileDataSourceAt(bus, headerWord), byteCount);
            int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
            byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, columns,
                out Extent16<ImagePixels> size);
            using var png = new MemoryStream();
            IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
            byte[] encoded = png.ToArray();

            File.WriteAllBytes(Path.Combine(importData, FileName(headerWord)), encoded);
            byte[] nativeColors = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                SuperMetroid.AssetExtraction.Enemies.RoomEnemyDefinitionImporter.PaletteAddressAt(bus, headerWord),
                EnemyPaletteSheetFile.ColorCount * sizeof(ushort));
            var colors = new PaletteRgb5[EnemyPaletteSheetFile.ColorCount];
            for (int color = 0; color < colors.Length; color++)
            {
                int word = BinaryPrimitives.ReadUInt16LittleEndian(nativeColors.AsSpan(color * 2));
                if (!ColourWordCodec.IsColorWord(checked((ushort)word)))
                    throw new InvalidDataException($"Enemy ${headerWord:X4} palette has an unrepresentable high bit.");
                colors[color] = new PaletteRgb5
                {
                    Red = word & 31,
                    Green = word >> 5 & 31,
                    Blue = word >> 10 & 31,
                };
            }
            byte[] paletteJson = EnemyPaletteSheetFile.Write(new EnemyPaletteSheetDocument
            {
                Version = 1,
                Colors = colors,
            });
            File.WriteAllBytes(Path.Combine(importData,
                PaletteFileName(headerWord)), paletteJson);
            entries.Add(headerWord, new EnemyTileFileEntry(
                byteCount, Convert.ToHexString(SHA256.HashData(encoded)),
                Convert.ToHexString(SHA256.HashData(paletteJson))));
        }
        if (entries.Count != EnemyTileArtworkFormat.RetailEntryCount)
            throw new InvalidDataException($"Expected {EnemyTileArtworkFormat.RetailEntryCount} retail enemy sheets; found {entries.Count}.");
        ValidateDefinitionIds(entries.Keys);
        byte[] firstMelt = ExtractCrocomireMelt(bus,
            CrocomireMeltingIllustration.First,
            CrocomireMeltingArtworkFormat.FirstByteCount);
        byte[] secondMelt = ExtractCrocomireMelt(bus,
            CrocomireMeltingIllustration.Second,
            CrocomireMeltingArtworkFormat.SecondByteCount);
        byte[] firstMeltTilemap = ExtractCrocomireMeltTilemap(bus,
            CrocomireMeltingSources.Tilemap(CrocomireMeltingIllustration.First));
        byte[] secondMeltTilemap = ExtractCrocomireMeltTilemap(bus,
            CrocomireMeltingSources.Tilemap(CrocomireMeltingIllustration.Second));
        File.WriteAllBytes(Path.Combine(importData, CrocomireMeltingArtworkFormat.FirstFileName), firstMelt);
        File.WriteAllBytes(Path.Combine(importData, CrocomireMeltingArtworkFormat.SecondFileName), secondMelt);
        // The melt layouts, like Kraid's, only compose the boss body sheets, whose pictures the installation holds (#1638).
        File.WriteAllBytes(Path.Combine(importData,
            CrocomireMeltingArtworkFormat.FirstTilemapFileName), firstMeltTilemap);
        File.WriteAllBytes(Path.Combine(importData,
            CrocomireMeltingArtworkFormat.SecondTilemapFileName), secondMeltTilemap);
        byte[] skeletonPlanar = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            CrocomireSourceDefinitions.SkeletonChunk(0),
            CrocomireSkeletonCharacterDefinitions.TotalByteCount);
        byte[] skeletonPixels = SnesGraphics.DecodePlanarTiles(skeletonPlanar, 4,
            RoomCharacterAtlasFormat.TileColumns,
            out Extent16<ImagePixels> skeletonSize);
        using var skeletonImage = new MemoryStream();
        IndexedPng.Write(skeletonImage, skeletonSize,
            skeletonPixels, SnesGraphics.DiagnosticPalette(16));
        byte[] skeletonPng = skeletonImage.ToArray();
        File.WriteAllBytes(Path.Combine(importData,
            CrocomireSkeletonCharacterDefinitions.FileName), skeletonPng);
        // The enemy compositions only compose the enemy sheets: their parts are packed sprite placements (#1638). The game
        // reads only their display bindings, which are installed.
        byte[] spritemapJson = EnemySpritemapFiles.Extract(bus);
        File.WriteAllBytes(Path.Combine(importData, EnemySpritemapDefinitions.FileName),
            spritemapJson);
        byte[] frameDisplaysJson = EnemyFrameDisplayFiles.Extract();
        File.WriteAllBytes(Path.Combine(directory, EnemyFrameDisplayFiles.FileName), frameDisplaysJson);
        byte[] projectileSpritemapJson = EnemyProjectileSpritemapFiles.Extract(bus);
        // The enemy projectiles' compositions only compose the spawned-object sheets: their parts are packed sprite placements (#1638).
        File.WriteAllBytes(Path.Combine(importData,
            EnemyProjectileSpritemapDefinitions.FileName), projectileSpritemapJson);
        byte[] extendedJson = EnemyExtendedFrameFiles.Extract(bus);
        File.WriteAllBytes(Path.Combine(importData, EnemyExtendedFrameDefinitions.FileName),
            extendedJson);
        var torizoInstructionHashes = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (TorizoTileSheet page in
                 TorizoSwapArtworkDefinitions.All)
        {
            byte[] png = ExtractTorizoInstructionPage(bus, page);
            File.WriteAllBytes(Path.Combine(importData, page.FileName), png);
            torizoInstructionHashes.Add(page.FileName,
                Convert.ToHexString(SHA256.HashData(png)));
        }
        var ceresEscapeTileHashes = new Dictionary<string, string>(StringComparer.Ordinal);
        for (int pageIndex = 0; pageIndex < CeresEscapeTileArtworkDefinitions.All.Length; pageIndex++)
        {
            TileSheetDefinition page = CeresEscapeTileArtworkDefinitions.All[pageIndex];
            byte[] png = IndexedTilePageExtractor.Extract(bus, (int)EscapeSourceDefinitions.CeresEscapePages[pageIndex],
                page.ByteCount, $"Ceres escape page {page.FileName}");
            File.WriteAllBytes(Path.Combine(importData, page.FileName), png);
            ceresEscapeTileHashes.Add(page.FileName,
                Convert.ToHexString(SHA256.HashData(png)));
        }
        byte[] ceresEscapeOverlay = CeresEscapeOverlayTilemapFiles.Extract(bus);
        File.WriteAllBytes(Path.Combine(importData,
            CeresWarningGlyphDefinitions.FileName), ceresEscapeOverlay);
        byte[] phantoonBg2Json = PhantoonBg2FrameFiles.Extract(bus);
        File.WriteAllBytes(Path.Combine(importData, PhantoonBg2FrameDefinitions.FileName),
            phantoonBg2Json);
        byte[] draygonBg2Json = DraygonBg2FrameFiles.Extract(bus);
        File.WriteAllBytes(Path.Combine(importData, DraygonBg2FrameDefinitions.FileName),
            draygonBg2Json);
        byte[] crocomireBg2Json = CrocomireBg2FrameFiles.Extract(bus);
        File.WriteAllBytes(Path.Combine(importData, CrocomireBg2FrameDefinitions.FileName),
            crocomireBg2Json);
        byte[] motherBrainBodyBg2Json = MotherBrainBodyBg2FrameFiles.Extract(bus);
        File.WriteAllBytes(Path.Combine(importData, MotherBrainBodyBg2FrameDefinitions.FileName),
            motherBrainBodyBg2Json);
        var gunshipLiftoffHashes = new Dictionary<int, string>();
        for (int index = 0; index < GunshipLiftoffTransferDefinitions.Frames.Length; index++)
        {
            int pageSource = EnemySourceDefinitions.GunshipLiftoffPageSource(index);
            var planar = new byte[GunshipLiftoffCharacterDefinitions.ByteCount];
            for (int offset = 0; offset < planar.Length; offset++)
                planar[offset] = bus.ReadCartridgeByte(pageSource + offset);
            byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4,
                RoomCharacterAtlasFormat.TileColumns, out Extent16<ImagePixels> size);
            using var image = new MemoryStream();
            IndexedPng.Write(image, size, pixels,
                SnesGraphics.DiagnosticPalette(16));
            byte[] png = image.ToArray();
            File.WriteAllBytes(Path.Combine(importData,
                EnemyTileArtworkFormat.GunshipLiftoffFileName(index)), png);
            gunshipLiftoffHashes.Add(index,
                Convert.ToHexString(SHA256.HashData(png)));
        }
        // This bank-$B7 range is not in any ordinary enemy definition's VRAM set.
        // Preserve the complete tile-aligned source around the right-hand corpse
        // frame so edits can feed the native row-by-row WRAM decay processor.
        byte[] corpsePlanar = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            MotherBrainSourceDefinitions.Corpse.Characters,
            MotherBrainCorpseSheet.ByteCount);
        byte[] corpsePixels = SnesGraphics.DecodePlanarTiles(corpsePlanar, 4,
            RoomCharacterAtlasFormat.TileColumns, out Extent16<ImagePixels> corpseSize);
        using var corpseStream = new MemoryStream();
        IndexedPng.Write(corpseStream, corpseSize, corpsePixels,
            SnesGraphics.DiagnosticPalette(16));
        byte[] corpsePng = corpseStream.ToArray();
        File.WriteAllBytes(Path.Combine(importData, MotherBrainCorpseSheet.FileName),
            corpsePng);
        // The following bank-$B7 pages are a different visual owner: the escape
        // typewriter characters. Keep their file separate from corpse decay.
        byte[] escapeTextPlanar = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            EscapeSourceDefinitions.TextCharacters,
            EscapeTextSheetFormat.ByteCount);
        byte[] escapeTextPixels = SnesGraphics.DecodePlanarTiles(escapeTextPlanar, 4,
            RoomCharacterAtlasFormat.TileColumns,
            out Extent16<ImagePixels> escapeTextSize);
        using var escapeTextStream = new MemoryStream();
        IndexedPng.Write(escapeTextStream, escapeTextSize,
            escapeTextPixels, SnesGraphics.DiagnosticPalette(16));
        byte[] escapeTextPng = escapeTextStream.ToArray();
        File.WriteAllBytes(Path.Combine(importData,
            EscapeTextSheetFormat.FileName), escapeTextPng);
        var motherBrainSpecialHashes = new Dictionary<int, string>();
        foreach (MotherBrainSpecialSpriteSheetDefinition sheet in
                 MotherBrainSpecialSpriteSheetDefinitions.All)
        {
            byte[] png = ExtractMotherBrainSpecialSpritePng(bus, sheet);
            File.WriteAllBytes(Path.Combine(importData, sheet.FileName), png);
            motherBrainSpecialHashes.Add(MotherBrainSourceDefinitions.SpecialSprites.Sources[sheet.Sheet],
                Convert.ToHexString(SHA256.HashData(png)));
        }
        byte[] upperKraid = ExtractKraidTilemap(bus, KraidSourceDefinitions.UpperBodyTilemap);
        byte[] lowerKraid = ExtractKraidTilemap(bus, KraidSourceDefinitions.LowerBodyTilemap);
        File.WriteAllBytes(Path.Combine(importData, KraidBackgroundArtworkFormat.UpperFileName),
            upperKraid);
        File.WriteAllBytes(Path.Combine(importData, KraidBackgroundArtworkFormat.LowerFileName),
            lowerKraid);
        var kraidHeadHashes = new Dictionary<int, string>();
        foreach (KraidHeadFrame head in KraidHeadTilemapFormat.HeadsByMouthStage)
        {
            byte[] native = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                KraidSourceDefinitions.CodeBank | KraidHeadArtworkDefinitions.Address(head.MouthStage),
                KraidWorkingMapDefinitions.HeadImageCells * sizeof(ushort));
            byte[] json = KraidHeadTilemapFiles.Encode(native);
            File.WriteAllBytes(Path.Combine(importData,
                KraidBackgroundArtworkFormat.HeadFileName(head.MouthStage)), json);
            kraidHeadHashes.Add(head.MouthStage, Convert.ToHexString(SHA256.HashData(json)));
        }
        byte[] kraidRoomBackground = ExtractKraidRoomBackground(bus);
        File.WriteAllBytes(Path.Combine(importData,
            KraidBackgroundArtworkFormat.RoomBackgroundFileName), kraidRoomBackground);
        byte[] kraidColors = ExtractKraidColors(bus);
        File.WriteAllBytes(Path.Combine(importData, KraidColorFormat.FileName), kraidColors);
        // The game works with Kraid's eye colours alone (#1637): they are installed by what they colour.
        byte[] kraidEyeColours = KraidEyeColourFile.Write(KraidColorFile.Read(new MemoryStream(kraidColors, writable: false)).Eyes());
        File.WriteAllBytes(Path.Combine(directory, KraidEyeColourFile.FileName), kraidEyeColours);
        (string ceresDoorTilesHash, string ceresDoorColorsHash) =
            CeresDoorVisualFiles.Extract(bus, directory, importData);
        byte[] magdollitePaletteCycle = MagdollitePaletteCycleExtractor.Extract(bus);
        byte[] auxiliaryColors = EnemyAuxiliaryColorFiles.Extract(bus);
        // The enemies', bosses' and areas' colour documents only pre-colour their sheets: they are import data (#1637).
        File.WriteAllBytes(Path.Combine(importData, EnemyAuxiliaryColorFormat.FileName), auxiliaryColors);
        File.WriteAllBytes(Path.Combine(importData, MagdollitePaletteCycleFormat.FileName),
            magdollitePaletteCycle);
        byte[] workRobotPaletteCycle = WorkRobotPaletteCycleExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(importData, WorkRobotPaletteCycleFormat.FileName),
            workRobotPaletteCycle);
        byte[] crocomireColors = CrocomireColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(importData, CrocomireColorFormat.FileName),
            crocomireColors);
        byte[] draygonColors = DraygonColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(importData, DraygonColorFormat.FileName),
            draygonColors);
        byte[] phantoonColors = PhantoonColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(importData, PhantoonColorFormat.FileName),
            phantoonColors);
        byte[] chozoAndTubeColors = ChozoAndTubeColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(importData, ChozoAndTubeColorFormat.FileName),
            chozoAndTubeColors);
        byte[] sporeSpawnColors = SporeSpawnColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(importData, SporeSpawnColorFormat.FileName),
            sporeSpawnColors);
        byte[] dachoraColors = DachoraColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(importData, DachoraColorFormat.FileName),
            dachoraColors);
        byte[] shitroidColors = ShitroidColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(importData, ShitroidColorFormat.FileName),
            shitroidColors);
        byte[] babyMetroidCutsceneColors =
            BabyMetroidCutsceneColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(importData,
            BabyMetroidCutsceneColorFormat.FileName), babyMetroidCutsceneColors);
        byte[] botwoonColors = BotwoonColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(importData, BotwoonColorFormat.FileName),
            botwoonColors);
        byte[] motherBrainDeathColors = MotherBrainDeathColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(importData,
            MotherBrainDeathColorFormat.FileName), motherBrainDeathColors);
        byte[] zebetiteColors = ZebetiteColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(importData, ZebetiteColorFile.FileName),
            zebetiteColors);
        byte[] norfairRidleyColors = NorfairRidleyColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(importData, NorfairRidleyColorFormat.FileName),
            norfairRidleyColors);
        byte[] tourianStatueColors = TourianStatueColorExtractor.Extract(bus);
        File.WriteAllBytes(Path.Combine(importData, TourianStatueColorFormat.FileName),
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
            Convert.ToHexString(SHA256.HashData(frameDisplaysJson)),
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
            Convert.ToHexString(SHA256.HashData(kraidEyeColours)),
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
        // The manifest names every file by the cartridge's enemy headers and pins the import data's hashes: only the import reads it (#1693).
        File.WriteAllBytes(Path.Combine(importData, EnemyTileArtworkFormat.ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(manifest, JsonOptions));
    }

    /// <summary>
    /// Reads what the game binds from the installed documents (#1636): every frame's display (#1638) and Kraid's eye colours
    /// (#1637), with the identity of the documents the directory holds. It opens no manifest (#1693), character page or
    /// colour document only the import reads; the installation holds none.
    /// </summary>
    /// <param name="directory">The installed enemy directory.</param>
    /// <exception cref="InvalidDataException">An installed document is invalid.</exception>
    public static InstalledEnemyArtwork Load(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        return new(FrameDisplays(directory).Displays(), KraidEyes(directory),
            SelectedPresentationHash.Create("enemy-directory-v2", content =>
            {
                // Every installed document, framed by name in name order: the directory holds no character page (#1636).
                foreach (string path in Directory.EnumerateFiles(directory).Order(StringComparer.Ordinal))
                    content.Append(Path.GetFileName(path), File.ReadAllBytes(path));
            }));
    }

    /// <summary>
    /// The installed display bindings of every enemy frame in <paramref name="directory"/> (#1638), which the game reads and
    /// the enemy sheets' frames are named from.
    /// </summary>
    /// <exception cref="InvalidDataException">A binding is invalid.</exception>
    internal static InstalledEnemyFrameDisplays FrameDisplays(string directory)
    {
        try
        {
            return EnemyFrameDisplayFiles.Load(new MemoryStream(
                File.ReadAllBytes(Path.Combine(directory, EnemyFrameDisplayFiles.FileName)), writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid enemy display bindings in {directory}: {error.Message}",
                error);
        }
    }

    /// <summary>Kraid's installed eye colours in <paramref name="directory"/>, which gameplay's eye glow reads back (#1512, #1637).</summary>
    /// <exception cref="InvalidDataException">The installed file is invalid.</exception>
    private static KraidEyeColours KraidEyes(string directory)
    {
        string path = Path.Combine(directory, KraidEyeColourFile.FileName);
        try
        {
            return KraidEyeColourFile.Read(new MemoryStream(File.ReadAllBytes(path), writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException($"Invalid Kraid eye colour file {path}: {error.Message}", error);
        }
    }

    /// <summary>
    /// Checks the manifest, then compiles the characters, colour documents, layouts and compositions of the import data
    /// <paramref name="importData"/> into the enemy, boss and area catalogs the import composes from.
    /// </summary>
    /// <param name="importData">The component's import-data directory, holding the manifest, the character and tile pages, the colour documents, the layouts and the compositions.</param>
    public static ImportEnemyArtwork LoadForImport(string importData) =>
        EnemyTileArtworkReader.Open(importData).LoadForImport(importData);

    /// <summary>
    /// SHA-256 of the 121 sorted four-digit bank-$A0 header pointers joined with commas: the headers whose artwork the
    /// retail room states' enemy graphics sets load and some picture shows, 118 enemies' and three loaded for no enemy (#1686).
    /// This makes a missing or substituted sheet fail during installation validation.
    /// </summary>
    private const string RetailDefinitionIdsSha256 =
        "26EB49B1B4E24925A9A53D816B9FE5BBFB995EC4AD73B240A766E774E32A3D8B";

    /// <summary>The indexed-PNG import-data file name of <paramref name="definitionId"/>'s tiles, named by its header pointer.</summary>
    internal static string FileName(EnemyDefinitionId definitionId) => FileName(EnemyDefinitionIds.HeaderPointer(definitionId));

    /// <summary>The import-data RGB5 palette JSON file name of <paramref name="definitionId"/>'s colours, named by its header pointer.</summary>
    internal static string PaletteFileName(EnemyDefinitionId definitionId) => PaletteFileName(EnemyDefinitionIds.HeaderPointer(definitionId));

    /// <summary>The indexed-PNG import-data file name of the tiles the bank-$A0 header at <paramref name="headerPointer"/> names.</summary>
    internal static string FileName(int headerPointer) => $"enemy-{headerPointer:X4}-tiles.png";

    /// <summary>The import-data RGB5 palette JSON file name of the colours the bank-$A0 header at <paramref name="headerPointer"/> names.</summary>
    internal static string PaletteFileName(int headerPointer) => $"enemy-{headerPointer:X4}-colors.json";

    /// <summary>Rejects missing or substituted retail enemy graphics identities using the pinned ordered ID digest.</summary>
    internal static void ValidateDefinitionIds(IEnumerable<int> pointers)
    {
        string joined = string.Join(",", pointers.Order().Select(pointer => $"{pointer:X4}"));
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.ASCII.GetBytes(joined)));
        if (!string.Equals(hash, RetailDefinitionIdsSha256, StringComparison.Ordinal))
            throw new InvalidDataException("Enemy tile manifest omits or substitutes a retail graphics definition.");
    }

    /// <summary>Assembles one Crocomire melt image as the cartridge does, as indexed PNG.</summary>
    private static byte[] ExtractCrocomireMelt(ISnesAddressSpace bus,
        CrocomireMeltingIllustration illustration, int encodedByteCount)
    {
        var planar = new byte[encodedByteCount];
        int bank = CrocomireMeltingSources.Graphics(illustration).Bank;
        for (int band = 0; band < CrocomireMelting.BandCount(illustration); band++)
        {
            int imageByte = CrocomireMeltingSources.ImageByte(band);
            int source = CrocomireMeltingSources.SourceWord(illustration, band);
            for (int index = 0; index < CrocomireMeltingSourceDefinitions.CopyByteCount; index++)
            {
                planar[imageByte + index] = bus.ReadCartridgeByte(
                    ((int)new SnesAddress(checked((byte)bank), checked((ushort)(source + index)))));
            }
        }
        int tileCount = RoomCharacterAtlasFormat.ValidateTileCount(encodedByteCount);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4,
            Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount),
            out Extent16<ImagePixels> size);
        using var png = new MemoryStream();
        IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
        return png.ToArray();
    }

    /// <summary>Decompresses a native Kraid background tilemap and writes its editable document form.</summary>
    private static byte[] ExtractKraidTilemap(ISnesAddressSpace bus, int sourceAddress)
    {
        byte[] native = RomDataReader.Decompress(CartridgeImportSource.Require(bus), sourceAddress,
            KraidWorkingMapDefinitions.SourceTilemapBytes);
        return RoomBackgroundTilemapExtractor.Encode(native);
    }

    /// <summary>Exports Kraid's room background character data to PNG.</summary>
    private static byte[] ExtractKraidRoomBackground(ISnesAddressSpace bus)
    {
        byte[] planar = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            KraidSourceDefinitions.RoomBackgroundTiles,
            KraidSourceDefinitions.RoomBackgroundTileBytes);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4,
            KraidSourceDefinitions.RoomBackgroundTileBytes /
                RoomCharacterAtlasFormat.BytesPerTile,
            out Extent16<ImagePixels> size);
        using var png = new MemoryStream();
        IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
        return png.ToArray();
    }

    /// <summary>Exports a Mother Brain special sprite sheet as indexed PNG.</summary>
    private static byte[] ExtractMotherBrainSpecialSpritePng(ISnesAddressSpace bus,
        MotherBrainSpecialSpriteSheetDefinition sheet)
    {
        byte[] native = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            MotherBrainSourceDefinitions.SpecialSprites.Sources[sheet.Sheet], sheet.ByteCount);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(native, 4,
            RoomCharacterAtlasFormat.TileColumns, out Extent16<ImagePixels> size);
        using var output = new MemoryStream();
        IndexedPng.Write(output, size, pixels,
            SnesGraphics.DiagnosticPalette(16));
        return output.ToArray();
    }

    /// <summary>Exports one Torizo instruction tile page with its definition's source range.</summary>
    private static byte[] ExtractTorizoInstructionPage(ISnesAddressSpace bus,
        TorizoTileSheet page) =>
        IndexedTilePageExtractor.Extract(bus, TorizoSourceDefinitions.PageSources[page.Page], page.ByteCount,
            $"Torizo instruction page {page.FileName}");

    /// <summary>Writes Kraid's backdrop, target, health, secondary, and death-arm palettes as one editable color document.</summary>
    private static byte[] ExtractKraidColors(ISnesAddressSpace bus) =>
        KraidColorFile.Write(new KraidColorDocument
        {
            Version = KraidColorFormat.Version,
            RoomBackdrop = ReadKraidColors(bus, KraidPaletteSource.RoomBackdrop),
            InitialTarget = ReadKraidColors(bus, KraidPaletteSource.InitialTarget),
            Health = ReadKraidColors(bus, KraidPaletteSource.Health),
            Secondary = ReadKraidColors(bus, KraidPaletteSource.Secondary),
            DeathArm = ReadKraidColors(bus, KraidPaletteSource.DeathArm),
        });

    /// <summary>Decodes one native Kraid palette and rejects color words whose high bit cannot be represented.</summary>
    private static PaletteRgb5[] ReadKraidColors(ISnesAddressSpace bus,
        KraidPaletteSource source)
    {
        int count = KraidPalettes.ColorCount(source);
        byte[] native = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            KraidPaletteSources.SourceAddress(source), count * sizeof(ushort));
        var colors = new PaletteRgb5[count];
        for (int index = 0; index < count; index++)
        {
            int word = BinaryPrimitives.ReadUInt16LittleEndian(native.AsSpan(index * 2));
            if (!ColourWordCodec.IsColorWord(checked((ushort)word)))
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

    /// <summary>Decodes a Crocomire melt tilemap into editable cells and verifies its $FFFF terminator.</summary>
    private static byte[] ExtractCrocomireMeltTilemap(ISnesAddressSpace bus,
        int sourceAddress)
    {
        var cells = new CrocomireMeltingTilemapCell[
            CrocomireMeltingArtworkFormat.TilemapCellCount];
        for (int index = 0; index < cells.Length; index++)
        {
            int address = sourceAddress + index * 2;
            var word = new SnesBgTilemapWord(CartridgeImportSource.Require(bus).ReadCartridgeWord(address));
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
        if (CartridgeImportSource.Require(bus).ReadCartridgeWord(terminator) != CartridgeFormatDefinitions.ListTerminator)
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

    /// <summary>Records cartridge provenance and hashes for every stock enemy-art resource in the installation.</summary>
    internal sealed record EnemyTileManifest(int Version, string SourceCartridgeSha256,
        Dictionary<int, EnemyTileFileEntry> Entries,
        string CrocomireFirstSha256, string CrocomireSecondSha256,
        string CrocomireFirstTilemapSha256, string CrocomireSecondTilemapSha256,
        string EnemyCompositionsSha256, string EnemyProjectileCompositionsSha256,
        string EnemyExtendedCompositionsSha256,
        string EnemyFrameDisplaysSha256,
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
        Dictionary<int, string> KraidHeadsSha256,
        string KraidRoomBackgroundSha256,
        string KraidColorsSha256,
        string KraidEyeColoursSha256,
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

    /// <summary>Pairs an enemy sheet's native transfer size with its indexed PNG and palette digests.</summary>
    internal sealed record EnemyTileFileEntry(int NativeByteCount, string Sha256, string PaletteSha256);
}
