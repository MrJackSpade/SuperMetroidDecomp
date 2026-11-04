using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Bosses.Crocomire;
using SuperMetroid.AssetExtraction.Bosses.Torizo;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
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
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// An installation's enemy-art files, read one component at a time: a stock directory whose manifest has been checked,
/// and the player's override directory, if any. Each component is read from its own files, its stock checked against
/// the manifest's hash and its override, when present, selected over the stock, so the whole catalog
/// (<see cref="LoadAll"/>) is its components read in turn, and a reader of one component reads only that component's files.
/// </summary>
internal sealed class EnemyTileArtworkReader
{
    /// <summary>The checked manifest of the stock directory.</summary>
    private readonly EnemyTileArtworkFiles.EnemyTileManifest manifest;

    /// <summary>The stock enemy-art directory.</summary>
    private readonly string stockDirectory;

    /// <summary>The player's override directory, or null for stock content alone.</summary>
    private readonly string? overrideDirectory;

    /// <summary>A reader of <paramref name="stockDirectory"/>, whose manifest is <paramref name="manifest"/>, under <paramref name="overrideDirectory"/>.</summary>
    private EnemyTileArtworkReader(EnemyTileArtworkFiles.EnemyTileManifest manifest, string stockDirectory, string? overrideDirectory)
    {
        this.manifest = manifest;
        this.stockDirectory = stockDirectory;
        this.overrideDirectory = overrideDirectory;
    }

    /// <summary>Reads and checks the manifest of <paramref name="stockDirectory"/>, whose components are then read under <paramref name="overrideDirectory"/>.</summary>
    /// <exception cref="InvalidDataException">The manifest is missing, malformed, or does not describe this installation.</exception>
    internal static EnemyTileArtworkReader Open(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, EnemyTileArtworkFormat.ManifestFileName);
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
        return new EnemyTileArtworkReader(manifest, stockDirectory, overrideDirectory);
    }

    /// <summary>Reads every component and compiles the enemy, boss and area catalogs without reading a cartridge.</summary>
    /// <exception cref="InvalidDataException">A stock file fails its manifest hash, or a selected file is invalid.</exception>
    internal InstalledEnemyArtwork LoadAll()
    {
        EnemyTileArtworkParts enemySheets = EnemySheets();
        CrocomireMeltingArtwork crocomire = CrocomireMelting();
        CrocomireSkeletonArtwork skeleton = CrocomireSkeleton();
        EnemySpritemapCatalog spritemaps = Spritemaps();
        EnemyProjectileSpritemapCatalog projectileSpritemaps = ProjectileSpritemaps();
        EnemyExtendedFrameCatalog extendedFrames = ExtendedFrames();
        PhantoonBg2FrameCatalog phantoonBg2Frames = PhantoonBg2Frames();
        DraygonBg2FrameCatalog draygonBg2Frames = DraygonBg2Frames();
        CrocomireBg2FrameCatalog crocomireBg2Frames = CrocomireBg2Frames();
        MotherBrainBodyBg2FrameCatalog motherBrainBodyBg2Frames = MotherBrainBodyBg2Frames();
        GunshipLiftoffArtworkCatalog gunshipLiftoff = GunshipLiftoff();
        TorizoSwapArtwork torizoInstructionVram = TorizoSwaps();
        CeresEscapeTileArtwork ceresEscapeTiles = CeresEscapeTiles();
        CeresEscapeOverlays ceresEscapeOverlay = CeresEscapeOverlayTilemaps();
        RoomCharacterAtlas motherBrainCorpse = MotherBrainCorpse();
        RoomCharacterAtlas motherBrainEscapeText = MotherBrainEscapeText();
        MotherBrainSpecialSpriteArtworkCatalog motherBrainSpecialSprites = MotherBrainSpecialSprites();
        KraidBackgroundArtwork kraidBackground = KraidBackground();
        KraidColorCatalog kraidColors = KraidColors();
        CeresDoorSourceArtwork ceresDoor = CeresDoor();
        MagdollitePaletteCycle magdollitePaletteCycle = MagdollitePaletteCycle();
        WorkRobotPaletteCycle workRobotPaletteCycle = WorkRobotPaletteCycle();
        CrocomireColorCatalog crocomireColors = CrocomireColors();
        DraygonColorCatalog draygonColors = DraygonColors();
        PhantoonColorCatalog phantoonColors = PhantoonColors();
        ChozoAndTubeColorCatalog chozoAndTubeColors = ChozoAndTubeColors();
        SporeSpawnColorCatalog sporeSpawnColors = SporeSpawnColors();
        DachoraColorCatalog dachoraColors = DachoraColors();
        ShitroidColorCatalog shitroidColors = ShitroidColors();
        BabyMetroidCutsceneColorCatalog babyMetroidCutsceneColors = BabyMetroidCutsceneColors();
        BotwoonColorCatalog botwoonColors = BotwoonColors();
        MotherBrainDeathColorCatalog motherBrainDeathColors = MotherBrainDeathColors();
        ZebetiteColorCatalog zebetiteColors = ZebetiteColors();
        NorfairRidleyColorCatalog norfairRidleyColors = NorfairRidleyColors();
        TourianStatuePalettes tourianStatueColors = TourianStatueColors();
        EnemyAuxiliaryColorCatalog auxiliaryColors = AuxiliaryColors();
        return new InstalledEnemyArtwork(
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
    /// Every retail enemy's characters and colours, read from its character PNG and colour document, each stock file
    /// checked against its manifest hash: the parts of the enemy catalog that hold no frames or colour cycles.
    /// </summary>
    /// <exception cref="InvalidDataException">A stock file fails its manifest hash, or a selected file is invalid.</exception>
    internal EnemyTileArtworkParts EnemySheets()
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
            string fileName = EnemyTileArtworkFiles.FileName(definitionPointer);
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
                sheets.Add(definitionPointer, RoomCharacterAtlasPng.Read(
                    new MemoryStream(selected, writable: false), entry.NativeByteCount));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid enemy tile PNG {selectedPath}: {error.Message}", error);
            }
            string paletteName = EnemyTileArtworkFiles.PaletteFileName(definitionPointer);
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
                palettes.Add(definitionPointer, EnemyPaletteSheetFile.Read(
                    new MemoryStream(selected, writable: false)));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid enemy palette {selectedPalettePath}: {error.Message}", error);
            }
        }
        return new EnemyTileArtworkParts { Sheets = sheets, Palettes = palettes };
    }

    /// <summary>Crocomire's two melt images and their tilemaps.</summary>
    /// <exception cref="InvalidDataException">A stock file fails its manifest hash, or a selected file is invalid.</exception>
    internal CrocomireMeltingArtwork CrocomireMelting()
    {
        byte[] first = ReadStockOrOverride(CrocomireMeltingArtworkFormat.FirstFileName,
            manifest.CrocomireFirstSha256);
        byte[] second = ReadStockOrOverride(CrocomireMeltingArtworkFormat.SecondFileName,
            manifest.CrocomireSecondSha256);
        byte[] firstTilemap = ReadStockOrOverride(
            CrocomireMeltingArtworkFormat.FirstTilemapFileName,
            manifest.CrocomireFirstTilemapSha256);
        byte[] secondTilemap = ReadStockOrOverride(
            CrocomireMeltingArtworkFormat.SecondTilemapFileName,
            manifest.CrocomireSecondTilemapSha256);
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
                $"Invalid Crocomire melt artwork in {overrideDirectory ?? stockDirectory}: {error.Message}", error);
        }
    }

    /// <summary>Crocomire's skeleton characters.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or the selected file is invalid.</exception>
    internal CrocomireSkeletonArtwork CrocomireSkeleton()
    {
        byte[] skeletonPng = ReadStockOrOverride(
            CrocomireSkeletonCharacterDefinitions.FileName,
            manifest.CrocomireSkeletonSha256);
        try
        {
            return CrocomireSkeletonArtwork.Load(RoomCharacterAtlasPng.Read(
                new MemoryStream(skeletonPng, writable: false), CrocomireSkeletonCharacterDefinitions.TotalByteCount));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Crocomire skeleton artwork in {overrideDirectory ?? stockDirectory}: " +
                error.Message, error);
        }
    }

    /// <summary>The enemies' compositions, an override's frames merged over the verified stock.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or a composition is invalid.</exception>
    internal EnemySpritemapCatalog Spritemaps()
    {
        try
        {
            // Older overrides cannot know the later Skultera, Waver, and
            // Skree/Metaree/Zoa, Pipe Bug, Fake Kraid, Kraid nail, Owtch, Stoke, and Ripper identities.
            // Merge only their validated frames onto verified current stock content,
            // preserving existing user edits through an extraction upgrade. Stock is
            // compiled once; without an override that compiled stock is the selection.
            byte[] stockComposition = ReadVerifiedStock(
                EnemySpritemapDefinitions.FileName, manifest.EnemyCompositionsSha256);
            EnemySpritemapCatalog stockSpritemaps = EnemyFrameCartridgeLookup.RequireSameBankDisplayBindings(
                EnemySpritemapDocuments.Load(new MemoryStream(stockComposition, writable: false)));
            return OverridePath(EnemySpritemapDefinitions.FileName) is { } compositionOverride
                ? EnemyFrameCartridgeLookup.RequireSameBankDisplayBindings(EnemySpritemapDocuments.Load(
                    new MemoryStream(File.ReadAllBytes(compositionOverride), writable: false), stockSpritemaps))
                : stockSpritemaps;
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid enemy compositions in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
    }

    /// <summary>The enemy projectiles' compositions, an override's frames merged over the verified stock.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or a composition is invalid.</exception>
    internal EnemyProjectileSpritemapCatalog ProjectileSpritemaps()
    {
        try
        {
            EnemyProjectileSpritemapCatalog stockProjectiles =
                EnemyProjectileSpritemapDocuments.Load(new MemoryStream(ReadVerifiedStock(
                    EnemyProjectileSpritemapDefinitions.FileName,
                    manifest.EnemyProjectileCompositionsSha256), writable: false));
            return OverridePath(EnemyProjectileSpritemapDefinitions.FileName) is { } projectileOverride
                ? EnemyProjectileSpritemapDocuments.Load(
                    new MemoryStream(File.ReadAllBytes(projectileOverride), writable: false), stockProjectiles)
                : stockProjectiles;
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                "Invalid installed enemy-projectile compositions.", error);
        }
    }

    /// <summary>The enemies' extended frames, an override's frames overlaid on the verified stock.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or a frame is invalid.</exception>
    internal EnemyExtendedFrameCatalog ExtendedFrames()
    {
        try
        {
            // V1 has walking frames and v2 adds wall frames. Overlay either
            // validated legacy file onto complete, hash-checked v3 stock so
            // newly added Ninja art cannot discard the user's existing edits.
            byte[] stockExtendedJson = ReadVerifiedStock(
                EnemyExtendedFrameDefinitions.FileName, manifest.EnemyExtendedCompositionsSha256);
            EnemyExtendedFrameCatalog stockExtended = EnemyExtendedFrameDocuments.Load(
                new MemoryStream(stockExtendedJson, writable: false));
            return OverridePath(EnemyExtendedFrameDefinitions.FileName) is { } extendedOverride
                ? EnemyExtendedFrameDocuments.Load(
                    new MemoryStream(File.ReadAllBytes(extendedOverride), writable: false), stockExtended)
                : stockExtended;
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid extended enemy compositions in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
    }

    /// <summary>Phantoon's BG2 frames.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or the selected file is invalid.</exception>
    internal PhantoonBg2FrameCatalog PhantoonBg2Frames()
    {
        try
        {
            byte[] selected = ReadStockOrOverride(
                PhantoonBg2FrameDefinitions.FileName,
                manifest.PhantoonBg2FramesSha256);
            return PhantoonBg2FrameCatalog.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Phantoon BG2 frames in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
    }

    /// <summary>Draygon's BG2 frames, an override written before they were named for their poses upgraded.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or the selected file is invalid.</exception>
    internal DraygonBg2FrameCatalog DraygonBg2Frames()
    {
        try
        {
            // A player's override written before Draygon's frames were named for their poses keeps loading (#1465).
            byte[] selected = EnemyBg2FrameDocumentUpgrade.Upgrade(ReadStockOrOverride(
                DraygonBg2FrameDefinitions.FileName,
                manifest.DraygonBg2FramesSha256), DraygonBg2FrameDefinitions.Frames, DraygonBg2FrameDefinitions.Version,
                EnemyFrameLegacyNameDefinitions.ExtendedCompositions, "Draygon");
            return DraygonBg2FrameCatalog.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Draygon BG2 frames in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
    }

    /// <summary>Crocomire's BG2 frames, an override written before they were named for their poses upgraded.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or the selected file is invalid.</exception>
    internal CrocomireBg2FrameCatalog CrocomireBg2Frames()
    {
        try
        {
            // A player's override written before Crocomire's frames were named for their poses keeps loading (#1465).
            byte[] selected = EnemyBg2FrameDocumentUpgrade.Upgrade(ReadStockOrOverride(
                CrocomireBg2FrameDefinitions.FileName,
                manifest.CrocomireBg2FramesSha256), CrocomireBg2FrameDefinitions.Frames, CrocomireBg2FrameDefinitions.Version,
                EnemyFrameLegacyNameDefinitions.CrocomireBg2Frames, "Crocomire");
            return CrocomireBg2FrameCatalog.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Crocomire BG2 frames in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
    }

    /// <summary>Mother Brain's body BG2 frames.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or the selected file is invalid.</exception>
    internal MotherBrainBodyBg2FrameCatalog MotherBrainBodyBg2Frames()
    {
        try
        {
            return MotherBrainBodyBg2FrameCatalog.Load(new MemoryStream(
                ReadStockOrOverride(MotherBrainBodyBg2FrameDefinitions.FileName,
                    manifest.MotherBrainBodyBg2FramesSha256), writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Mother Brain body BG2 frames in {overrideDirectory ?? stockDirectory}: {error.Message}", error);
        }
    }

    /// <summary>The gunship's takeoff frames.</summary>
    /// <exception cref="InvalidDataException">The manifest omits a frame, a stock file fails its hash, or a selected file is invalid.</exception>
    internal GunshipLiftoffArtworkCatalog GunshipLiftoff()
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
            byte[] selected = ReadStockOrOverride(fileName, expectedHash);
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
    /// <exception cref="InvalidDataException">The manifest omits a page, a stock file fails its hash, or a selected file is invalid.</exception>
    internal TorizoSwapArtwork TorizoSwaps()
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
            byte[] selected = ReadStockOrOverride(page.FileName, expectedHash);
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
    /// <exception cref="InvalidDataException">The manifest omits a page, a stock file fails its hash, or a selected file is invalid.</exception>
    internal CeresEscapeTileArtwork CeresEscapeTiles()
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
            byte[] selected = ReadStockOrOverride(page.FileName, expectedHash);
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

    /// <summary>The Ceres escape's overlay tilemaps.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or the selected file is invalid.</exception>
    internal CeresEscapeOverlays CeresEscapeOverlayTilemaps()
    {
        try
        {
            byte[] selected = ReadStockOrOverride(
                CeresWarningGlyphDefinitions.FileName,
                manifest.CeresEscapeOverlaySha256);
            return CeresEscapeOverlays.Load(
                new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                "Invalid installed Ceres escape overlay tilemaps.", error);
        }
    }

    /// <summary>Mother Brain's corpse characters.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or the selected file is invalid.</exception>
    internal RoomCharacterAtlas MotherBrainCorpse()
    {
        try
        {
            byte[] selected = ReadStockOrOverride(
                MotherBrainCorpseSheet.FileName,
                manifest.MotherBrainCorpseSha256);
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
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or the selected file is invalid.</exception>
    internal RoomCharacterAtlas MotherBrainEscapeText()
    {
        try
        {
            byte[] selected = ReadStockOrOverride(
                EscapeTextSheetFormat.FileName,
                manifest.MotherBrainEscapeTextSha256);
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
    /// <exception cref="InvalidDataException">The manifest omits a sheet, a stock file fails its hash, or a selected file is invalid.</exception>
    internal MotherBrainSpecialSpriteArtworkCatalog MotherBrainSpecialSprites()
    {
        var motherBrainSpecialSheets = new Dictionary<MotherBrainSpritePageSheet, RoomCharacterAtlas>();
        foreach (MotherBrainSpecialSpriteSheetDefinition sheet in
                 MotherBrainSpecialSpriteArtworkDefinitions.All)
        {
            if (!manifest.MotherBrainSpecialSpritesSha256.TryGetValue(
                    MotherBrainSourceDefinitions.SpecialSprites.Sources[sheet.Sheet], out string? expectedHash) ||
                string.IsNullOrWhiteSpace(expectedHash))
                throw new InvalidDataException(
                    $"Enemy tile manifest omits {sheet.FileName}.");
            try
            {
                byte[] selected = ReadStockOrOverride(sheet.FileName, expectedHash);
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

    /// <summary>Kraid's BG2 pages, heads and room background.</summary>
    /// <exception cref="InvalidDataException">A stock file fails its manifest hash, or a selected file is invalid.</exception>
    internal KraidBackgroundArtwork KraidBackground()
    {
        RoomBackgroundTilemapAtlas upperKraid = LoadKraidTilemap(
            KraidBackgroundArtworkFormat.UpperFileName, manifest.KraidUpperSha256);
        RoomBackgroundTilemapAtlas lowerKraid = LoadKraidTilemap(
            KraidBackgroundArtworkFormat.LowerFileName, manifest.KraidLowerSha256);
        var kraidHeads = new Dictionary<int, KraidHeadTilemapAtlas>();
        foreach ((int mouthStage, string sha256) in manifest.KraidHeadsSha256)
        {
            string fileName = KraidBackgroundArtworkFormat.HeadFileName(mouthStage);
            byte[] selected = ReadStockOrOverride(fileName, sha256);
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
        byte[] roomBackgroundPng = ReadStockOrOverride(
            roomBackgroundName, manifest.KraidRoomBackgroundSha256);
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

    /// <summary>Kraid's colours.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or the selected file is invalid.</exception>
    internal KraidColorCatalog KraidColors()
    {
        try
        {
            byte[] selected = ReadStockOrOverride(KraidColorFormat.FileName,
                manifest.KraidColorsSha256);
            return KraidColorFile.Read(new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Kraid color file {KraidColorFormat.FileName}: {error.Message}", error);
        }
    }

    /// <summary>The Ceres door's characters and colours.</summary>
    /// <exception cref="InvalidDataException">A stock file fails its manifest hash, or a selected file is invalid.</exception>
    internal CeresDoorSourceArtwork CeresDoor()
    {
        try
        {
            return CeresDoorVisualFiles.Load(
                ReadStockOrOverride(CeresDoorVisualFormat.TilesFileName,
                    manifest.CeresDoorTilesSha256),
                ReadStockOrOverride(CeresDoorVisualFormat.ColorsFileName,
                    manifest.CeresDoorColorsSha256));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException("Invalid installed Ceres-door visuals.", error);
        }
    }

    /// <summary>The Magdollite's palette cycle.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or the selected file is invalid.</exception>
    internal MagdollitePaletteCycle MagdollitePaletteCycle() =>
        ReadColors(MagdollitePaletteCycleFormat.FileName, manifest.MagdollitePaletteCycleSha256, "Magdollite palette cycle",
            stream => MagdollitePaletteCycleFile.Read(stream));

    /// <summary>The Work Robot's palette cycle.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or the selected file is invalid.</exception>
    internal WorkRobotPaletteCycle WorkRobotPaletteCycle() =>
        ReadColors(WorkRobotPaletteCycleFormat.FileName, manifest.WorkRobotPaletteCycleSha256, "Work Robot palette cycle",
            stream => WorkRobotPaletteCycleFile.Read(stream));

    /// <summary>Crocomire's colours.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or the selected file is invalid.</exception>
    internal CrocomireColorCatalog CrocomireColors() =>
        ReadColors(CrocomireColorFormat.FileName, manifest.CrocomireColorsSha256, "Crocomire colors",
            stream => CrocomireColorFile.Read(stream));

    /// <summary>Draygon's colours.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or the selected file is invalid.</exception>
    internal DraygonColorCatalog DraygonColors() =>
        ReadColors(DraygonColorFormat.FileName, manifest.DraygonColorsSha256, "Draygon colors",
            stream => DraygonColorFile.Read(stream));

    /// <summary>Phantoon's colours.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or the selected file is invalid.</exception>
    internal PhantoonColorCatalog PhantoonColors() =>
        ReadColors(PhantoonColorFormat.FileName, manifest.PhantoonColorsSha256, "Phantoon colors",
            stream => PhantoonColorFile.Read(stream));

    /// <summary>The Chozo statues' and glass tube's colours.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or the selected file is invalid.</exception>
    internal ChozoAndTubeColorCatalog ChozoAndTubeColors() =>
        ReadColors(ChozoAndTubeColorFormat.FileName, manifest.ChozoAndTubeColorsSha256, "Chozo/tube colors",
            stream => ChozoAndTubeColorFile.Read(stream));

    /// <summary>Spore Spawn's colours.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or the selected file is invalid.</exception>
    internal SporeSpawnColorCatalog SporeSpawnColors() =>
        ReadColors(SporeSpawnColorFormat.FileName, manifest.SporeSpawnColorsSha256, "Spore Spawn colors",
            stream => SporeSpawnColorFile.Read(stream));

    /// <summary>The Dachora's colours.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or the selected file is invalid.</exception>
    internal DachoraColorCatalog DachoraColors() =>
        ReadColors(DachoraColorFormat.FileName, manifest.DachoraColorsSha256, "Dachora colors",
            stream => DachoraColorFile.Read(stream));

    /// <summary>The Shitroid's colours.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or the selected file is invalid.</exception>
    internal ShitroidColorCatalog ShitroidColors() =>
        ReadColors(ShitroidColorFormat.FileName, manifest.ShitroidColorsSha256, "Shitroid colors",
            stream => ShitroidColorFile.Read(stream));

    /// <summary>The cutscene Baby Metroid's colours.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or the selected file is invalid.</exception>
    internal BabyMetroidCutsceneColorCatalog BabyMetroidCutsceneColors() =>
        ReadColors(BabyMetroidCutsceneColorFormat.FileName, manifest.BabyMetroidCutsceneColorsSha256, "cutscene Baby colors",
            stream => BabyMetroidCutsceneColorFile.Read(stream));

    /// <summary>Botwoon's colours.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or the selected file is invalid.</exception>
    internal BotwoonColorCatalog BotwoonColors() =>
        ReadColors(BotwoonColorFormat.FileName, manifest.BotwoonColorsSha256, "Botwoon colors",
            stream => BotwoonColorFile.Read(stream));

    /// <summary>Mother Brain's death colours.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or the selected file is invalid.</exception>
    internal MotherBrainDeathColorCatalog MotherBrainDeathColors() =>
        ReadColors(MotherBrainDeathColorFormat.FileName, manifest.MotherBrainDeathColorsSha256, "Mother Brain death colors",
            stream => MotherBrainDeathColorFile.Read(stream));

    /// <summary>The Zebetites' colours.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or the selected file is invalid.</exception>
    internal ZebetiteColorCatalog ZebetiteColors() =>
        ReadColors(ZebetiteColorFile.FileName, manifest.ZebetiteColorsSha256, "Zebetite colors",
            stream => ZebetiteColorFile.Read(stream));

    /// <summary>Norfair Ridley's colours.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or the selected file is invalid.</exception>
    internal NorfairRidleyColorCatalog NorfairRidleyColors() =>
        ReadColors(NorfairRidleyColorFormat.FileName, manifest.NorfairRidleyColorsSha256, "Norfair Ridley colors",
            stream => NorfairRidleyColorFile.Read(stream));

    /// <summary>The Tourian statues' colours.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or the selected file is invalid.</exception>
    internal TourianStatuePalettes TourianStatueColors() =>
        ReadColors(TourianStatueColorFormat.FileName, manifest.TourianStatueColorsSha256, "Tourian statue colors",
            stream => TourianStatueColorFile.Read(stream));

    /// <summary>The enemies' auxiliary colours.</summary>
    /// <exception cref="InvalidDataException">The stock file fails its manifest hash, or the selected file is invalid.</exception>
    internal EnemyAuxiliaryColorCatalog AuxiliaryColors() =>
        EnemyAuxiliaryColorFiles.Read(new MemoryStream(
            ReadStockOrOverride(EnemyAuxiliaryColorFormat.FileName, manifest.AuxiliaryColorsSha256), writable: false));

    /// <summary>
    /// Reads the colour document <paramref name="fileName"/> with <paramref name="read"/>, its stock checked against
    /// <paramref name="expectedSha256"/>; a document that does not read is reported as invalid <paramref name="colors"/>.
    /// </summary>
    private T ReadColors<T>(string fileName, string expectedSha256, string colors, Func<MemoryStream, T> read)
    {
        try
        {
            byte[] selected = ReadStockOrOverride(fileName, expectedSha256);
            return read(new MemoryStream(selected, writable: false));
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid {colors} in {overrideDirectory ?? stockDirectory}: {error.Message}",
                error);
        }
    }

    /// <summary>Reads one of Kraid's BG2 tilemaps.</summary>
    private RoomBackgroundTilemapAtlas LoadKraidTilemap(string fileName, string expectedSha256)
    {
        byte[] selected = ReadStockOrOverride(fileName, expectedSha256);
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

    /// <summary>The override of <paramref name="fileName"/> when one exists, else its stock, which is checked either way.</summary>
    private byte[] ReadStockOrOverride(string fileName, string expectedSha256)
    {
        byte[] stock = ReadVerifiedStock(fileName, expectedSha256);
        return OverridePath(fileName) is { } overridePath ? File.ReadAllBytes(overridePath) : stock;
    }

    /// <summary>The stock <paramref name="fileName"/>, checked against <paramref name="expectedSha256"/>.</summary>
    private byte[] ReadVerifiedStock(string fileName, string expectedSha256)
    {
        string stockPath = Path.Combine(stockDirectory, fileName);
        byte[] stock = File.ReadAllBytes(stockPath);
        if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock)), expectedSha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Stock enemy asset {stockPath} failed its manifest hash.");
        return stock;
    }

    /// <summary>The override of <paramref name="fileName"/>, or null when there is none.</summary>
    private string? OverridePath(string fileName)
    {
        string? overridePath = overrideDirectory is null ? null :
            Path.Combine(overrideDirectory, fileName);
        return overridePath is not null && File.Exists(overridePath) ? overridePath : null;
    }
}
