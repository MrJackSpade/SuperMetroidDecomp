using SuperMetroid.AssetExtraction.Bosses.Crocomire;
using SuperMetroid.AssetExtraction.Bosses.Torizo;
using SuperMetroid.AssetExtraction.World.Artwork;
using System.Text.Json;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Kraid;
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
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// An enemy-art directory's files, read one component at a time from a directory whose manifest has been checked. Each
/// component is read from its own files, as the player may have edited them, so the import's whole catalog
/// (<see cref="LoadForImport"/>) is its components read in turn, with their characters from the component's import data
/// (#1636), and a reader of one component reads only that component's files. The game's read (<see cref="LoadInstalled"/>)
/// reads only the documents it binds.
/// </summary>
internal sealed class EnemyTileArtworkReader
{
    /// <summary>The checked manifest of the stock directory.</summary>
    private readonly EnemyTileArtworkFiles.EnemyTileManifest manifest;

    /// <summary>The installed enemy-art directory.</summary>
    private readonly string directory;

    /// <summary>A reader of <paramref name="directory"/>, whose manifest is <paramref name="manifest"/>.</summary>
    private EnemyTileArtworkReader(EnemyTileArtworkFiles.EnemyTileManifest manifest, string directory)
    {
        this.manifest = manifest;
        this.directory = directory;
    }

    /// <summary>Reads and checks the manifest of <paramref name="directory"/>, whose components are then read from it.</summary>
    /// <exception cref="InvalidDataException">The manifest is missing, malformed, or does not describe this installation.</exception>
    internal static EnemyTileArtworkReader Open(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string manifestPath = Path.Combine(directory, EnemyTileArtworkFormat.ManifestFileName);
        EnemyTileArtworkFiles.EnemyTileManifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<EnemyTileArtworkFiles.EnemyTileManifest>(File.ReadAllBytes(manifestPath), EnemyTileArtworkFiles.JsonOptions)
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
            string.IsNullOrWhiteSpace(manifest.EnemyFrameDisplaysSha256) ||
            string.IsNullOrWhiteSpace(manifest.PhantoonBg2FramesSha256) ||
            string.IsNullOrWhiteSpace(manifest.DraygonBg2FramesSha256) ||
            string.IsNullOrWhiteSpace(manifest.CrocomireBg2FramesSha256) ||
            string.IsNullOrWhiteSpace(manifest.MotherBrainBodyBg2FramesSha256) ||
            manifest.GunshipLiftoffSha256 is null ||
            manifest.GunshipLiftoffSha256.Count !=
                GunshipLiftoffTransferDefinitions.Frames.Length ||
            manifest.TorizoInstructionTilesSha256 is null ||
            manifest.TorizoInstructionTilesSha256.Count !=
                TorizoSwapArtworkDefinitions.All.Length ||
            manifest.CeresEscapeTilesSha256 is null ||
            manifest.CeresEscapeTilesSha256.Count !=
                CeresEscapeTileArtworkDefinitions.All.Length ||
            string.IsNullOrWhiteSpace(manifest.CeresEscapeOverlaySha256) ||
            string.IsNullOrWhiteSpace(manifest.MotherBrainCorpseSha256) ||
            string.IsNullOrWhiteSpace(manifest.MotherBrainEscapeTextSha256) ||
            manifest.MotherBrainSpecialSpritesSha256 is null ||
            manifest.MotherBrainSpecialSpritesSha256.Count !=
                MotherBrainSpecialSpriteSheetDefinitions.All.Count ||
            string.IsNullOrWhiteSpace(manifest.KraidUpperSha256) ||
            string.IsNullOrWhiteSpace(manifest.KraidLowerSha256) ||
            manifest.KraidHeadsSha256 is null ||
            string.IsNullOrWhiteSpace(manifest.KraidRoomBackgroundSha256) ||
            string.IsNullOrWhiteSpace(manifest.KraidColorsSha256) ||
            string.IsNullOrWhiteSpace(manifest.KraidEyeColoursSha256) ||
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
        EnemyTileArtworkFiles.ValidateDefinitionIds(manifest.Entries.Keys);
        // The compiled graphics sets must name the same pinned identities the installation was built from.
        EnemyTileArtworkFiles.ValidateDefinitionIds(EnemyTileSourceDefinitions.All.Select(source => (int)EnemyDefinitionIds.HeaderPointer(source.DefinitionId)));
        if (!manifest.KraidHeadsSha256.Keys.Order().SequenceEqual(KraidHeadTilemapFormat.HeadsByMouthStage.Select(head => head.MouthStage)))
            throw new InvalidDataException("Enemy tile manifest omits or substitutes a Kraid head frame.");
        if (!manifest.TorizoInstructionTilesSha256.Keys.Order(StringComparer.Ordinal)
                .SequenceEqual(TorizoSwapArtworkDefinitions.All
                    .Select(page => page.FileName).Order(StringComparer.Ordinal)))
            throw new InvalidDataException(
                "Enemy tile manifest omits or substitutes a Torizo instruction tile page.");
        if (!manifest.CeresEscapeTilesSha256.Keys.Order(StringComparer.Ordinal)
                .SequenceEqual(CeresEscapeTileArtworkDefinitions.All
                    .Select(page => page.FileName).Order(StringComparer.Ordinal)))
            throw new InvalidDataException(
                "Enemy tile manifest omits or substitutes a Ceres escape tile page.");
        return new EnemyTileArtworkReader(manifest, directory);
    }

    /// <summary>
    /// Reads what the game binds from the installed documents: every enemy frame's display, from the display bindings
    /// (#1638), and Kraid's eye colours (#1637). No character page (#1636), colour document or composition only the import
    /// reads is read.
    /// </summary>
    /// <exception cref="InvalidDataException">An installed document is invalid.</exception>
    internal InstalledEnemyArtwork LoadInstalled() =>
        new(FrameDisplays().Displays(), KraidEyes(),
            SelectedPresentationHash.Create("enemy-directory-v2", content =>
            {
                // Every installed document, framed by name in name order: the directory holds no character page (#1636).
                foreach (string path in Directory.EnumerateFiles(directory).Order(StringComparer.Ordinal))
                    content.Append(Path.GetFileName(path), File.ReadAllBytes(path));
            }));

    /// <summary>
    /// Reads every component and compiles the enemy, boss and area catalogs without reading a cartridge, the characters
    /// and the colour documents from <paramref name="importData"/>.
    /// </summary>
    /// <param name="importData">The component's import-data directory, holding its character and tile pages and colour documents.</param>
    /// <exception cref="InvalidDataException">A file is invalid.</exception>
    internal ImportEnemyArtwork LoadForImport(string importData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
        EnemyTileArtworkParts enemySheets = EnemySheets(importData);
        CrocomireMeltingArtwork crocomire = CrocomireMelting(importData);
        CrocomireSkeletonArtwork skeleton = CrocomireSkeleton(importData);
        EnemySpritemapCatalog spritemaps = Spritemaps(importData);
        EnemyProjectileSpritemapCatalog projectileSpritemaps = ProjectileSpritemaps(importData);
        EnemyExtendedFrameCatalog extendedFrames = ExtendedFrames(importData);
        PhantoonBg2FrameCatalog phantoonBg2Frames = PhantoonBg2Frames(importData);
        DraygonBg2FrameCatalog draygonBg2Frames = DraygonBg2Frames(importData);
        CrocomireBg2FrameCatalog crocomireBg2Frames = CrocomireBg2Frames(importData);
        MotherBrainBodyBg2FrameCatalog motherBrainBodyBg2Frames = MotherBrainBodyBg2Frames(importData);
        GunshipLiftoffArtworkCatalog gunshipLiftoff = GunshipLiftoff(importData);
        TorizoSwapArtwork torizoInstructionVram = TorizoSwaps(importData);
        CeresEscapeTileArtwork ceresEscapeTiles = CeresEscapeTiles(importData);
        CeresEscapeOverlays ceresEscapeOverlay = CeresEscapeOverlayTilemaps(importData);
        RoomCharacterAtlas motherBrainCorpse = MotherBrainCorpse(importData);
        RoomCharacterAtlas motherBrainEscapeText = MotherBrainEscapeText(importData);
        MotherBrainSpecialSpriteArtworkCatalog motherBrainSpecialSprites = MotherBrainSpecialSprites(importData);
        KraidBackgroundArtwork kraidBackground = KraidBackground(importData);
        KraidColorCatalog kraidColors = KraidColors(importData);
        CeresDoorSourceArtwork ceresDoor = CeresDoor(importData);
        MagdollitePaletteCycle magdollitePaletteCycle = MagdollitePaletteCycle(importData);
        WorkRobotPaletteCycle workRobotPaletteCycle = WorkRobotPaletteCycle(importData);
        CrocomireColorCatalog crocomireColors = CrocomireColors(importData);
        DraygonColorCatalog draygonColors = DraygonColors(importData);
        PhantoonColorCatalog phantoonColors = PhantoonColors(importData);
        ChozoAndTubeColorCatalog chozoAndTubeColors = ChozoAndTubeColors(importData);
        SporeSpawnColorCatalog sporeSpawnColors = SporeSpawnColors(importData);
        DachoraColorCatalog dachoraColors = DachoraColors(importData);
        ShitroidColorCatalog shitroidColors = ShitroidColors(importData);
        BabyMetroidCutsceneColorCatalog babyMetroidCutsceneColors = BabyMetroidCutsceneColors(importData);
        BotwoonColorCatalog botwoonColors = BotwoonColors(importData);
        MotherBrainDeathColorCatalog motherBrainDeathColors = MotherBrainDeathColors(importData);
        ZebetiteColorCatalog zebetiteColors = ZebetiteColors(importData);
        NorfairRidleyColorCatalog norfairRidleyColors = NorfairRidleyColors(importData);
        TourianStatuePalettes tourianStatueColors = TourianStatueColors(importData);
        EnemyAuxiliaryColorCatalog auxiliaryColors = AuxiliaryColors(importData);
        return new ImportEnemyArtwork(
            EnemyTileSourceArtwork.FromInstalledArtwork(enemySheets with
            {
                Spritemaps = spritemaps, ExtendedFrames = extendedFrames,
                GunshipLiftoff = gunshipLiftoff, ProjectileSpritemaps = projectileSpritemaps,
                MagdollitePaletteCycle = magdollitePaletteCycle, WorkRobotPaletteCycle = workRobotPaletteCycle,
                ChozoAndTubeColors = chozoAndTubeColors, DachoraColors = dachoraColors, AuxiliaryColors = auxiliaryColors,
            }),
            BossSourceArtwork.FromInstalledArtwork(new BossSourceArtwork
            {
                CrocomireMelting = crocomire, CrocomireSkeleton = skeleton, CrocomireBg2Frames = crocomireBg2Frames,
                CrocomireColors = crocomireColors, TorizoSwaps = torizoInstructionVram,
                PhantoonBg2Frames = phantoonBg2Frames, PhantoonColors = phantoonColors,
                DraygonBg2Frames = draygonBg2Frames, DraygonColors = draygonColors,
                KraidBackground = kraidBackground,
                KraidColors = kraidColors, MotherBrainBodyBg2Frames = motherBrainBodyBg2Frames,
                MotherBrainCorpse = motherBrainCorpse, MotherBrainEscapeText = motherBrainEscapeText,
                MotherBrainSpecialSprites = motherBrainSpecialSprites, MotherBrainDeathColors = motherBrainDeathColors,
                BabyMetroidCutsceneColors = babyMetroidCutsceneColors, ShitroidColors = shitroidColors,
                ZebetiteColors = zebetiteColors, SporeSpawnColors = sporeSpawnColors, BotwoonColors = botwoonColors,
                NorfairRidleyColors = norfairRidleyColors,
            }),
            AreaSourceArtwork.FromInstalledArtwork(new AreaSourceArtwork
            {
                CeresDoor = ceresDoor, CeresEscapeTiles = ceresEscapeTiles,
                CeresEscapeOverlayTilemaps = ceresEscapeOverlay, TourianStatueColors = tourianStatueColors,
            }));
    }

    /// <summary>
    /// Every retail enemy's characters and colours, read from its character page and its colour document in
    /// <paramref name="importData"/>: the parts of the enemy catalog that hold no frames or colour cycles.
    /// </summary>
    /// <exception cref="InvalidDataException">A file is invalid.</exception>
    internal EnemyTileArtworkParts EnemySheets(string importData)
    {
        var sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>();
        var palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>();
        foreach ((int manifestKey, EnemyTileArtworkFiles.EnemyTileFileEntry entry) in manifest.Entries)
        {
            EnemyDefinitionId definitionPointer = EnemyDefinitionIds.FromHeaderPointer(manifestKey);
            if (entry.NativeByteCount != EnemyTileSetDefinitions.ByteCounts[definitionPointer])
                throw new InvalidDataException(
                    $"Enemy ${manifestKey:X4} manifest tile size differs from retail.");
            RoomCharacterAtlasFormat.ValidateTileCount(entry.NativeByteCount);
            string tilePath = Path.Combine(importData, EnemyTileArtworkFiles.FileName(definitionPointer));
            try
            {
                sheets.Add(definitionPointer, RoomCharacterAtlasPng.Read(
                    new MemoryStream(File.ReadAllBytes(tilePath), writable: false), entry.NativeByteCount));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid enemy tile PNG {tilePath}: {error.Message}", error);
            }
            string palettePath = Path.Combine(importData, EnemyTileArtworkFiles.PaletteFileName(definitionPointer));
            try
            {
                palettes.Add(definitionPointer, EnemyPaletteSheetFile.Read(
                    new MemoryStream(File.ReadAllBytes(palettePath), writable: false)));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid enemy palette {palettePath}: {error.Message}", error);
            }
        }
        return new EnemyTileArtworkParts { Sheets = sheets, Palettes = palettes };
    }

    /// <summary>Crocomire's two melt images and their layouts, all import data (#1636, #1638).</summary>
    /// <exception cref="InvalidDataException">An extracted file is invalid.</exception>
    internal CrocomireMeltingArtwork CrocomireMelting(string importData)
    {
        byte[] first = ReadImportData(importData, CrocomireMeltingArtworkFormat.FirstFileName);
        byte[] second = ReadImportData(importData, CrocomireMeltingArtworkFormat.SecondFileName);
        byte[] firstTilemap = ReadImportData(importData,
            CrocomireMeltingArtworkFormat.FirstTilemapFileName);
        byte[] secondTilemap = ReadImportData(importData,
            CrocomireMeltingArtworkFormat.SecondTilemapFileName);
        try
        {
            return CrocomireMeltingArtwork.Load(
                RoomCharacterAtlasPng.Read(new MemoryStream(first, writable: false), CrocomireMeltingArtworkFormat.FirstByteCount),
                RoomCharacterAtlasPng.Read(new MemoryStream(second, writable: false), CrocomireMeltingArtworkFormat.SecondByteCount),
                new MemoryStream(firstTilemap, writable: false),
                new MemoryStream(secondTilemap, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Crocomire melt artwork in {directory}: {error.Message}", error);
        }
    }

    /// <summary>Crocomire's skeleton characters.</summary>
    /// <exception cref="InvalidDataException">The installed file is invalid.</exception>
    internal CrocomireSkeletonArtwork CrocomireSkeleton(string importData)
    {
        byte[] skeletonPng = ReadImportData(importData,
            CrocomireSkeletonCharacterDefinitions.FileName);
        try
        {
            return CrocomireSkeletonArtwork.Load(RoomCharacterAtlasPng.Read(
                new MemoryStream(skeletonPng, writable: false), CrocomireSkeletonCharacterDefinitions.TotalByteCount));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Crocomire skeleton artwork in {directory}: " +
                error.Message, error);
        }
    }

    /// <summary>
    /// The installed display bindings of every enemy frame (#1638), which the game reads and the enemy sheets' frames are
    /// named from.
    /// </summary>
    /// <exception cref="InvalidDataException">A binding is invalid.</exception>
    internal InstalledEnemyFrameDisplays FrameDisplays()
    {
        try
        {
            return EnemyFrameDisplayFiles.Load(new MemoryStream(Read(EnemyFrameDisplayFiles.FileName), writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid enemy display bindings in {directory}: {error.Message}",
                error);
        }
    }

    /// <summary>The enemies' compositions, import data whose parts are packed sprite placements (#1638), from <paramref name="importData"/>.</summary>
    /// <exception cref="InvalidDataException">A composition is invalid.</exception>
    internal static EnemySpritemapCatalog Spritemaps(string importData)
    {
        try
        {
            return EnemyFrameCartridgeLookup.RequireSameBankDisplayBindings(EnemySpritemapDocuments.Load(
                new MemoryStream(ReadImportData(importData, EnemySpritemapDefinitions.FileName), writable: false)));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid extracted enemy compositions in {importData}: {error.Message}",
                error);
        }
    }

    /// <summary>The enemy projectiles' compositions, import data whose parts are packed sprite placements (#1638).</summary>
    /// <exception cref="InvalidDataException">A composition is invalid.</exception>
    internal static EnemyProjectileSpritemapCatalog ProjectileSpritemaps(string importData)
    {
        try
        {
            return EnemyProjectileSpritemapDocuments.Load(new MemoryStream(
                ReadImportData(importData, EnemyProjectileSpritemapDefinitions.FileName), writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                "Invalid extracted enemy-projectile compositions.", error);
        }
    }

    /// <summary>The enemies' extended frames, import data whose parts are packed sprite placements (#1638), from <paramref name="importData"/>.</summary>
    /// <exception cref="InvalidDataException">A frame is invalid.</exception>
    internal static EnemyExtendedFrameCatalog ExtendedFrames(string importData)
    {
        try
        {
            return EnemyExtendedFrameDocuments.Load(new MemoryStream(
                ReadImportData(importData, EnemyExtendedFrameDefinitions.FileName), writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid extracted extended enemy compositions in {importData}: {error.Message}",
                error);
        }
    }

    /// <summary>Phantoon's BG2 frames, import data whose writes are packed tile placements (#1638).</summary>
    /// <exception cref="InvalidDataException">The extracted file is invalid.</exception>
    internal PhantoonBg2FrameCatalog PhantoonBg2Frames(string importData)
    {
        try
        {
            byte[] selected = ReadImportData(importData,
                PhantoonBg2FrameDefinitions.FileName);
            return PhantoonBg2FrameCatalog.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Phantoon BG2 frames in {directory}: {error.Message}",
                error);
        }
    }

    /// <summary>Draygon's BG2 frames, import data whose writes are packed tile placements (#1638).</summary>
    /// <exception cref="InvalidDataException">The extracted file is invalid.</exception>
    internal DraygonBg2FrameCatalog DraygonBg2Frames(string importData)
    {
        try
        {
            byte[] selected = ReadImportData(importData, DraygonBg2FrameDefinitions.FileName);
            return DraygonBg2FrameCatalog.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Draygon BG2 frames in {directory}: {error.Message}",
                error);
        }
    }

    /// <summary>Crocomire's BG2 frames, import data whose writes are packed tile placements (#1638).</summary>
    /// <exception cref="InvalidDataException">The extracted file is invalid.</exception>
    internal CrocomireBg2FrameCatalog CrocomireBg2Frames(string importData)
    {
        try
        {
            byte[] selected = ReadImportData(importData, CrocomireBg2FrameDefinitions.FileName);
            return CrocomireBg2FrameCatalog.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Crocomire BG2 frames in {directory}: {error.Message}",
                error);
        }
    }

    /// <summary>Mother Brain's body BG2 frames, import data whose writes are packed tile placements (#1638).</summary>
    /// <exception cref="InvalidDataException">The extracted file is invalid.</exception>
    internal MotherBrainBodyBg2FrameCatalog MotherBrainBodyBg2Frames(string importData)
    {
        try
        {
            return MotherBrainBodyBg2FrameCatalog.Load(new MemoryStream(
                ReadImportData(importData, MotherBrainBodyBg2FrameDefinitions.FileName), writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Mother Brain body BG2 frames in {directory}: {error.Message}", error);
        }
    }

    /// <summary>The gunship's takeoff frames.</summary>
    /// <exception cref="InvalidDataException">The manifest omits a frame, or an installed file is invalid.</exception>
    internal GunshipLiftoffArtworkCatalog GunshipLiftoff(string importData)
    {
        var gunshipFrames = new RoomCharacterAtlas[
            GunshipLiftoffTransferDefinitions.Frames.Length];
        for (int index = 0; index < gunshipFrames.Length; index++)
        {
            if (!manifest.GunshipLiftoffSha256.TryGetValue(index,
                    out string? expectedHash) || string.IsNullOrWhiteSpace(expectedHash))
                throw new InvalidDataException(
                    $"Enemy tile manifest omits gunship takeoff frame {index}.");
            string fileName = EnemyTileArtworkFormat.GunshipLiftoffFileName(index);
            byte[] selected = ReadImportData(importData, fileName);
            try
            {
                gunshipFrames[index] = RoomCharacterAtlasPng.Read(
                    new MemoryStream(selected, writable: false),
                    GunshipLiftoffCharacterDefinitions.ByteCount);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid gunship takeoff PNG {fileName}: {error.Message}", error);
            }
        }
        return new GunshipLiftoffArtworkCatalog(gunshipFrames);
    }

    /// <summary>The Torizos' instruction tile pages.</summary>
    /// <exception cref="InvalidDataException">The manifest omits a page, or an installed file is invalid.</exception>
    internal TorizoSwapArtwork TorizoSwaps(string importData)
    {
        var torizoPages = new RoomCharacterAtlas[
            TorizoSwapArtworkDefinitions.All.Length];
        for (int index = 0; index < torizoPages.Length; index++)
        {
            TorizoTileSheet page =
                TorizoSwapArtworkDefinitions.All[index];
            if (!manifest.TorizoInstructionTilesSha256.TryGetValue(page.FileName,
                    out string? expectedHash) || string.IsNullOrWhiteSpace(expectedHash))
                throw new InvalidDataException(
                    $"Enemy tile manifest omits Torizo page {page.FileName}.");
            byte[] selected = ReadImportData(importData, page.FileName);
            try
            {
                torizoPages[index] = RoomCharacterAtlasPng.Read(
                    new MemoryStream(selected, writable: false), page.ByteCount);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid Torizo instruction PNG {page.FileName}: {error.Message}",
                    error);
            }
        }
        return new TorizoSwapArtwork(torizoPages);
    }

    /// <summary>The Ceres escape's tile pages.</summary>
    /// <exception cref="InvalidDataException">The manifest omits a page, or an installed file is invalid.</exception>
    internal CeresEscapeTileArtwork CeresEscapeTiles(string importData)
    {
        var ceresEscapePages = new RoomCharacterAtlas[
            CeresEscapeTileArtworkDefinitions.All.Length];
        for (int index = 0; index < ceresEscapePages.Length; index++)
        {
            TileSheetDefinition page =
                CeresEscapeTileArtworkDefinitions.All[index];
            if (!manifest.CeresEscapeTilesSha256.TryGetValue(page.FileName,
                    out string? expectedHash) || string.IsNullOrWhiteSpace(expectedHash))
                throw new InvalidDataException(
                    $"Enemy tile manifest omits Ceres escape page {page.FileName}.");
            byte[] selected = ReadImportData(importData, page.FileName);
            try
            {
                ceresEscapePages[index] = RoomCharacterAtlasPng.Read(
                    new MemoryStream(selected, writable: false), page.ByteCount);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid Ceres escape PNG {page.FileName}: {error.Message}",
                    error);
            }
        }
        return new CeresEscapeTileArtwork(ceresEscapePages);
    }

    /// <summary>The Ceres escape's overlay layouts, import data only the import composes the warning pictures from (#1638).</summary>
    /// <exception cref="InvalidDataException">The extracted file is invalid.</exception>
    internal static CeresEscapeOverlays CeresEscapeOverlayTilemaps(string importData)
    {
        try
        {
            byte[] selected = ReadImportData(importData,
                CeresWarningGlyphDefinitions.FileName);
            return CeresEscapeOverlays.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                "Invalid extracted Ceres escape overlay layouts.", error);
        }
    }

    /// <summary>Mother Brain's corpse characters.</summary>
    /// <exception cref="InvalidDataException">The installed file is invalid.</exception>
    internal static RoomCharacterAtlas MotherBrainCorpse(string importData)
    {
        try
        {
            byte[] selected = ReadImportData(importData,
                MotherBrainCorpseSheet.FileName);
            return RoomCharacterAtlasPng.Read(
                new MemoryStream(selected, writable: false),
                MotherBrainCorpseSheet.ByteCount);
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Mother Brain corpse PNG {MotherBrainCorpseSheet.FileName}: {error.Message}",
                error);
        }
    }

    /// <summary>Mother Brain's escape-text characters.</summary>
    /// <exception cref="InvalidDataException">The installed file is invalid.</exception>
    internal static RoomCharacterAtlas MotherBrainEscapeText(string importData)
    {
        try
        {
            byte[] selected = ReadImportData(importData,
                EscapeTextSheetFormat.FileName);
            return RoomCharacterAtlasPng.Read(
                new MemoryStream(selected, writable: false),
                EscapeTextSheetFormat.ByteCount);
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Mother Brain escape-text PNG {EscapeTextSheetFormat.FileName}: {error.Message}",
                error);
        }
    }

    /// <summary>Mother Brain's special sprite sheets.</summary>
    /// <exception cref="InvalidDataException">The manifest omits a sheet, or an installed file is invalid.</exception>
    internal MotherBrainSpecialSpriteArtworkCatalog MotherBrainSpecialSprites(string importData)
    {
        var motherBrainSpecialSheets = new Dictionary<MotherBrainSpriteSheet, RoomCharacterAtlas>();
        foreach (MotherBrainSpecialSpriteSheetDefinition sheet in
                 MotherBrainSpecialSpriteSheetDefinitions.All)
        {
            if (!manifest.MotherBrainSpecialSpritesSha256.TryGetValue(
                    MotherBrainSourceDefinitions.SpecialSprites.Sources[sheet.Sheet], out string? expectedHash) ||
                string.IsNullOrWhiteSpace(expectedHash))
                throw new InvalidDataException(
                    $"Enemy tile manifest omits {sheet.FileName}.");
            try
            {
                byte[] selected = ReadImportData(importData, sheet.FileName);
                motherBrainSpecialSheets.Add(sheet.Sheet,
                    RoomCharacterAtlasPng.Read(
                        new MemoryStream(selected, writable: false), sheet.ByteCount));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid Mother Brain sprite PNG {sheet.FileName}: {error.Message}",
                    error);
            }
        }
        return new MotherBrainSpecialSpriteArtworkCatalog(
            motherBrainSpecialSheets);
    }

    /// <summary>Kraid's body and head layouts and room background, all import data (#1636, #1638).</summary>
    /// <exception cref="InvalidDataException">An extracted file is invalid.</exception>
    internal KraidBackgroundArtwork KraidBackground(string importData)
    {
        RoomBackgroundTilemapAtlas upperKraid = LoadKraidTilemap(importData,
            KraidBackgroundArtworkFormat.UpperFileName);
        RoomBackgroundTilemapAtlas lowerKraid = LoadKraidTilemap(importData,
            KraidBackgroundArtworkFormat.LowerFileName);
        var kraidHeads = new Dictionary<int, KraidHeadTilemapAtlas>();
        foreach (int mouthStage in manifest.KraidHeadsSha256.Keys)
        {
            string fileName = KraidBackgroundArtworkFormat.HeadFileName(mouthStage);
            byte[] selected = ReadImportData(importData, fileName);
            try
            {
                kraidHeads.Add(mouthStage, KraidHeadTilemapAtlas.Load(
                    new MemoryStream(selected, writable: false)));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException(
                    $"Invalid Kraid head tilemap {fileName}: {error.Message}", error);
            }
        }
        string roomBackgroundName = KraidBackgroundArtworkFormat.RoomBackgroundFileName;
        byte[] roomBackgroundPng = ReadImportData(importData,
            roomBackgroundName);
        RoomCharacterAtlas roomBackground;
        try
        {
            roomBackground = RoomCharacterAtlasPng.Read(
                new MemoryStream(roomBackgroundPng, writable: false),
                KraidSourceDefinitions.RoomBackgroundTileBytes);
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Kraid room-background PNG {roomBackgroundName}: {error.Message}", error);
        }
        return new KraidBackgroundArtwork(upperKraid, lowerKraid, kraidHeads, roomBackground);
    }

    /// <summary>Kraid's colours, which only the import pre-colours his pictures from (#1637).</summary>
    /// <exception cref="InvalidDataException">The extracted file is invalid.</exception>
    internal static KraidColorCatalog KraidColors(string importData)
    {
        try
        {
            return KraidColorFile.Read(new MemoryStream(ReadImportData(importData, KraidColorFormat.FileName), writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Kraid color file {KraidColorFormat.FileName}: {error.Message}", error);
        }
    }

    /// <summary>Kraid's installed eye colours, which gameplay's eye glow reads back (#1512, #1637).</summary>
    /// <exception cref="InvalidDataException">The installed file is invalid.</exception>
    internal KraidEyeColours KraidEyes()
    {
        try
        {
            return KraidEyeColourFile.Read(new MemoryStream(Read(KraidEyeColourFile.FileName), writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Kraid eye colour file {Path.Combine(directory, KraidEyeColourFile.FileName)}: {error.Message}", error);
        }
    }

    /// <summary>The Ceres door's characters and colours.</summary>
    /// <exception cref="InvalidDataException">An installed file is invalid.</exception>
    internal static CeresDoorSourceArtwork CeresDoor(string importData)
    {
        try
        {
            return CeresDoorVisualFiles.Load(
                ReadImportData(importData, CeresDoorVisualFormat.TilesFileName),
                ReadImportData(importData, CeresDoorVisualFormat.ColorsFileName));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException("Invalid installed Ceres-door visuals.", error);
        }
    }

    /// <summary>The Magdollite's palette cycle.</summary>
    /// <exception cref="InvalidDataException">The installed file is invalid.</exception>
    internal MagdollitePaletteCycle MagdollitePaletteCycle(string importData) =>
        ReadColors(importData, MagdollitePaletteCycleFormat.FileName, "Magdollite palette cycle",
            stream => MagdollitePaletteCycleFile.Read(stream));

    /// <summary>The Work Robot's palette cycle.</summary>
    /// <exception cref="InvalidDataException">The installed file is invalid.</exception>
    internal WorkRobotPaletteCycle WorkRobotPaletteCycle(string importData) =>
        ReadColors(importData, WorkRobotPaletteCycleFormat.FileName, "Work Robot palette cycle",
            stream => WorkRobotPaletteCycleFile.Read(stream));

    /// <summary>Crocomire's colours.</summary>
    /// <exception cref="InvalidDataException">The installed file is invalid.</exception>
    internal CrocomireColorCatalog CrocomireColors(string importData) =>
        ReadColors(importData, CrocomireColorFormat.FileName, "Crocomire colors",
            stream => CrocomireColorFile.Read(stream));

    /// <summary>Draygon's colours.</summary>
    /// <exception cref="InvalidDataException">The installed file is invalid.</exception>
    internal DraygonColorCatalog DraygonColors(string importData) =>
        ReadColors(importData, DraygonColorFormat.FileName, "Draygon colors",
            stream => DraygonColorFile.Read(stream));

    /// <summary>Phantoon's colours.</summary>
    /// <exception cref="InvalidDataException">The installed file is invalid.</exception>
    internal PhantoonColorCatalog PhantoonColors(string importData) =>
        ReadColors(importData, PhantoonColorFormat.FileName, "Phantoon colors",
            stream => PhantoonColorFile.Read(stream));

    /// <summary>The Chozo statues' and glass tube's colours.</summary>
    /// <exception cref="InvalidDataException">The installed file is invalid.</exception>
    internal ChozoAndTubeColorCatalog ChozoAndTubeColors(string importData) =>
        ReadColors(importData, ChozoAndTubeColorFormat.FileName, "Chozo/tube colors",
            stream => ChozoAndTubeColorFile.Read(stream));

    /// <summary>Spore Spawn's colours.</summary>
    /// <exception cref="InvalidDataException">The installed file is invalid.</exception>
    internal SporeSpawnColorCatalog SporeSpawnColors(string importData) =>
        ReadColors(importData, SporeSpawnColorFormat.FileName, "Spore Spawn colors",
            stream => SporeSpawnColorFile.Read(stream));

    /// <summary>The Dachora's colours.</summary>
    /// <exception cref="InvalidDataException">The installed file is invalid.</exception>
    internal DachoraColorCatalog DachoraColors(string importData) =>
        ReadColors(importData, DachoraColorFormat.FileName, "Dachora colors",
            stream => DachoraColorFile.Read(stream));

    /// <summary>The Shitroid's colours.</summary>
    /// <exception cref="InvalidDataException">The installed file is invalid.</exception>
    internal ShitroidColorCatalog ShitroidColors(string importData) =>
        ReadColors(importData, ShitroidColorFormat.FileName, "Shitroid colors",
            stream => ShitroidColorFile.Read(stream));

    /// <summary>The cutscene Baby Metroid's colours.</summary>
    /// <exception cref="InvalidDataException">The installed file is invalid.</exception>
    internal BabyMetroidCutsceneColorCatalog BabyMetroidCutsceneColors(string importData) =>
        ReadColors(importData, BabyMetroidCutsceneColorFormat.FileName, "cutscene Baby colors",
            stream => BabyMetroidCutsceneColorFile.Read(stream));

    /// <summary>Botwoon's colours.</summary>
    /// <exception cref="InvalidDataException">The installed file is invalid.</exception>
    internal BotwoonColorCatalog BotwoonColors(string importData) =>
        ReadColors(importData, BotwoonColorFormat.FileName, "Botwoon colors",
            stream => BotwoonColorFile.Read(stream));

    /// <summary>Mother Brain's death colours.</summary>
    /// <exception cref="InvalidDataException">The installed file is invalid.</exception>
    internal MotherBrainDeathColorCatalog MotherBrainDeathColors(string importData) =>
        ReadColors(importData, MotherBrainDeathColorFormat.FileName, "Mother Brain death colors",
            stream => MotherBrainDeathColorFile.Read(stream));

    /// <summary>The Zebetites' colours.</summary>
    /// <exception cref="InvalidDataException">The installed file is invalid.</exception>
    internal ZebetiteColorCatalog ZebetiteColors(string importData) =>
        ReadColors(importData, ZebetiteColorFile.FileName, "Zebetite colors",
            stream => ZebetiteColorFile.Read(stream));

    /// <summary>Norfair Ridley's colours.</summary>
    /// <exception cref="InvalidDataException">The installed file is invalid.</exception>
    internal NorfairRidleyColorCatalog NorfairRidleyColors(string importData) =>
        ReadColors(importData, NorfairRidleyColorFormat.FileName, "Norfair Ridley colors",
            stream => NorfairRidleyColorFile.Read(stream));

    /// <summary>The Tourian statues' colours.</summary>
    /// <exception cref="InvalidDataException">The installed file is invalid.</exception>
    internal TourianStatuePalettes TourianStatueColors(string importData) =>
        ReadColors(importData, TourianStatueColorFormat.FileName, "Tourian statue colors",
            stream => TourianStatueColorFile.Read(stream));

    /// <summary>The enemies' auxiliary colours.</summary>
    /// <exception cref="InvalidDataException">The extracted file is invalid.</exception>
    internal static EnemyAuxiliaryColorCatalog AuxiliaryColors(string importData) =>
        EnemyAuxiliaryColorFiles.Read(new MemoryStream(
            ReadImportData(importData, EnemyAuxiliaryColorFormat.FileName), writable: false));

    /// <summary>
    /// Reads the colour document <paramref name="fileName"/> of the import data <paramref name="importData"/> (#1637) with
    /// <paramref name="read"/>; a document that does not read is reported as invalid <paramref name="colors"/>.
    /// </summary>
    private T ReadColors<T>(string importData, string fileName, string colors, Func<MemoryStream, T> read)
    {
        try
        {
            byte[] selected = ReadImportData(importData, fileName);
            return read(new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid {colors} in {directory}: {error.Message}",
                error);
        }
    }

    /// <summary>Reads one of Kraid's body layouts from the import data <paramref name="importData"/>.</summary>
    private static RoomBackgroundTilemapAtlas LoadKraidTilemap(string importData, string fileName)
    {
        byte[] selected = ReadImportData(importData, fileName);
        try
        {
            return RoomBackgroundTilemapAtlas.Load(
                new MemoryStream(selected, writable: false),
                KraidWorkingMapDefinitions.SourceTilemapBytes);
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Kraid BG2 tilemap {fileName}: {error.Message}", error);
        }
    }

    /// <summary>The installed <paramref name="fileName"/>, as the installation holds it.</summary>
    private byte[] Read(string fileName) => File.ReadAllBytes(Path.Combine(directory, fileName));

    /// <summary>
    /// The character or tile page (#1636), colour document (#1637) or layout only the import composes from (#1638)
    /// <paramref name="fileName"/> of the import data <paramref name="importData"/>.
    /// </summary>
    private static byte[] ReadImportData(string importData, string fileName) => File.ReadAllBytes(Path.Combine(importData, fileName));
}
