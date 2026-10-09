using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Small authored presentation data loaded through production PNG/JSON compilers.</summary>
    /// <param name="edit">Optional mutation key that changes one generated asset to test content-identity sensitivity.</param>
    /// <param name="reverse">When true, reverses ordered fixture inputs to test order-independent identity handling.</param>
    private sealed partial class EnemyIdentityFixture(string? edit = null, bool reverse = false)
    {
        /// <summary>Asset names registered while the fixture creates its generated content.</summary>
        public List<string> Edits { get; } = [];
        /// <summary>Records an asset identity for later fixture assertions.</summary>
        /// <param name="name">Stable generated asset key to record once.</param>
        private void Register(string name)
        {
            if (!Edits.Contains(name, StringComparer.Ordinal)) Edits.Add(name);
        }

        /// <summary>Serializes a fixture document using camel-case property names and the selected formatting mutation.</summary>
        /// <param name="document">Asset document serialized into the returned stream.</param>
        /// <returns>A readable stream positioned at the serialized JSON's beginning.</returns>
        public MemoryStream Json(object document) => new(JsonSerializer.SerializeToUtf8Bytes(document,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = edit == "json-indent" }));

        /// <summary>Creates a deterministic indexed PNG and registers its resource identity.</summary>
        /// <param name="name">Asset key used to select the optional content mutation.</param>
        /// <param name="bytes">Expected decoded tile byte count, used to derive image dimensions.</param>
        /// <param name="seed">Base pixel variation for distinguishing generated artwork.</param>
        /// <returns>A readable stream containing the encoded PNG.</returns>
        private MemoryStream Png(string name, int bytes, int seed = 0)
        {
            Register(name);
            int tiles = bytes / RoomCharacterAtlasFormat.BytesPerTile;
            int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tiles);
            int width = columns * 8, height = ((tiles + columns - 1) / columns) * 8;
            var pixels = new byte[width * height];
            pixels[0] = (byte)(seed ^ (edit == name ? 1 : 0));
            var png = new MemoryStream();
            IndexedPng.Write(png, width, height, pixels, Enumerable.Range(0, 16).Select(index =>
                new Rgba32((byte)(edit == "png-colors" ? 255 - index : index), 0, 0)).ToArray());
            png.Position = 0;
            return png;
        }

        /// <summary>Loads generated indexed PNG bytes through the production character-atlas decoder.</summary>
        /// <param name="name">Asset key used to generate and register the PNG.</param>
        /// <param name="bytes">Required decompressed character-data length.</param>
        /// <param name="seed">Base pixel variation for the generated artwork.</param>
        /// <returns>Decoded room character atlas.</returns>
        private RoomCharacterAtlas Characters(string name, int bytes, int seed = 0) =>
            RoomCharacterAtlas.Load(Png(name, bytes, seed), bytes);

        /// <summary>Builds a generated background tilemap cell array, optionally editing its final tile reference.</summary>
        /// <param name="name">Asset key used to register the tilemap content.</param>
        /// <param name="count">Number of cells in the authored page or map.</param>
        /// <returns>Cells with baseline attributes and the selected optional final-cell edit.</returns>
        private RoomBackgroundTilemapCell[] Cells(string name, int count)
        {
            Register(name);
            return Enumerable.Range(0, count).Select(index => new RoomBackgroundTilemapCell
            {
                TileColumn = index == count - 1 && edit == name ? 1 : 0,
                TileRow = 0, Palette = 0, Priority = false, FlipX = false, FlipY = false,
            }).ToArray();
        }

        /// <summary>Creates and production-loads a paged background tilemap with the requested byte extent.</summary>
        /// <param name="name">Asset key prefix used to register generated page contents.</param>
        /// <param name="bytes">Total byte size represented by the generated pages.</param>
        /// <returns>The decoded background tilemap atlas.</returns>
        private RoomBackgroundTilemapAtlas Map(string name, int bytes) => RoomBackgroundTilemapAtlas.Load(Json(
            new RoomBackgroundTilemapDocument
            {
                Version = RoomBackgroundTilemapFormat.Version,
                Pages = Enumerable.Range(0, bytes / RoomBackgroundTilemapFormat.BytesPerPage).Select(page =>
                    new RoomBackgroundTilemapPage { Cells = Cells(name + "-page-" + page, RoomBackgroundTilemapFormat.CellsPerPage) }).ToArray(),
            }), bytes);

        /// <summary>Generates sequential tile words and can toggle the final word for the named mutation.</summary>
        /// <param name="name">Asset key used to register and select the optional edit.</param>
        /// <param name="count">Number of words to generate.</param>
        /// <returns>The generated word array.</returns>
        private ushort[] Words(string name, int count)
        {
            Register(name);
            var words = Enumerable.Range(0, count).Select(index => (ushort)index).ToArray();
            if (edit == name) words[^1] ^= 1;
            return words;
        }

        /// <summary>Generates deterministic RGB5 entries and can alter the last color of the selected asset.</summary>
        /// <param name="name">Asset key used to register and select the optional edit.</param>
        /// <param name="count">Number of palette entries to produce.</param>
        /// <param name="seed">Starting red-channel value before cycling through the five-bit range.</param>
        /// <returns>The generated color array.</returns>
        private PaletteRgb5[] Colors(string name, int count, int seed = 0)
        {
            Register(name);
            return Enumerable.Range(0, count).Select(index => new PaletteRgb5
            {
                Red = ((seed + index) % 32) ^ (edit == name && index == count - 1 ? 1 : 0),
                Green = 0, Blue = 0,
            }).ToArray();
        }

        /// <summary>Generates named palette rows and optionally reverses their order for identity checks.</summary>
        /// <param name="name">Palette family key used for row identities and the ordering mutation.</param>
        /// <param name="count">Number of color rows.</param>
        /// <param name="colors">Number of colors in each row.</param>
        /// <returns>Generated rows in authored order, or reversed when selected by the fixture mutation.</returns>
        private PaletteRgb5[][] ColorRows(string name, int count, int colors)
        {
            Register(name + "-order");
            var rows = Enumerable.Range(0, count).Select(index => Colors(name + "-row-" + index, colors, index)).ToArray();
            if (edit == name + "-order") Array.Reverse(rows);
            return rows;
        }

        /// <summary>Creates a versioned JSON color document and passes its stream to the selected production loader.</summary>
        /// <param name="load">Catalog loader that decodes the generated document.</param>
        /// <param name="fields">Named color arrays or row arrays to include in the document.</param>
        /// <returns>The loaded color catalog.</returns>
        private T ColorCatalog<T>(Func<Stream, T> load, params (string Name, int Rows, int Colors)[] fields)
        {
            var document = new Dictionary<string, object> { ["version"] = 1 };
            foreach (var field in fields)
            {
                string name = typeof(T).Name + "." + field.Name;
                document.Add(field.Name, field.Rows == 0 ? Colors(name, field.Colors) : ColorRows(name, field.Rows, field.Colors));
            }
            return load(Json(document));
        }

        /// <summary>Builds a dictionary from ordered entries, optionally reversing enumeration order first.</summary>
        /// <param name="source">Entries in their canonical fixture order.</param>
        /// <returns>A dictionary populated in forward or reversed enumeration order.</returns>
        private Dictionary<TKey, TValue> Ordered<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> source) where TKey : notnull =>
            (reverse ? source.Reverse() : source).ToDictionary();

        /// <summary>Assembles the complete synthetic enemy-art catalog, using supplied catalogs where specified.</summary>
        /// <param name="spritemaps">Optional sprite-map catalog overriding the generated default.</param>
        /// <param name="extendedFrames">Optional extended-frame catalog overriding the generated default.</param>
        /// <param name="projectileSpritemaps">Optional projectile sprite-map catalog overriding the generated default.</param>
        /// <param name="motherBrainBodyBg2Frames">Optional Mother Brain BG2 frames overriding the generated default.</param>
        /// <returns>The production catalog assembled from deterministic fixture assets.</returns>
        public EnemyTileArtworkCatalog Build(EnemySpritemapCatalog? spritemaps = null,
            EnemyExtendedFrameCatalog? extendedFrames = null,
            EnemyProjectileSpritemapCatalog? projectileSpritemaps = null,
            MotherBrainBodyBg2FrameCatalog? motherBrainBodyBg2Frames = null)
        {
            Register("dma-source");
            Register("sheet-id");
            ushort firstId = (ushort)(edit == "sheet-id" ? 3 : 1);
            var sheets = Ordered(new[]
            {
                KeyValuePair.Create(firstId, Characters("sheet-first", RoomCharacterAtlasFormat.BytesPerTile)),
                KeyValuePair.Create((ushort)2, Characters("sheet-second", RoomCharacterAtlasFormat.BytesPerTile, 1)),
            });
            var palettes = Ordered(new[]
            {
                KeyValuePair.Create(firstId, EnemyPaletteSheet.Load(Json(new EnemyPaletteSheetDocument
                    { Version = 1, Colors = Colors("palette-first", EnemyPaletteSheet.ColorCount) }))),
                KeyValuePair.Create((ushort)2, EnemyPaletteSheet.Load(Json(new EnemyPaletteSheetDocument
                    { Version = 1, Colors = Colors("palette-second", EnemyPaletteSheet.ColorCount) }))),
            });
            var dma = Ordered(new[] { KeyValuePair.Create(firstId, edit == "dma-source" ? 3 : 1), KeyValuePair.Create((ushort)2, 2) });
            var heads = Ordered(new[]
            {
                KeyValuePair.Create((ushort)1, KraidHeadTilemapAtlas.Load(Json(new KraidHeadTilemapDocument
                {
                    Version = KraidHeadTilemapFormat.Version, Width = KraidHeadTilemapFormat.Width,
                    Height = KraidHeadTilemapFormat.Height, Cells = Cells("kraid-head-first", KraidBackgroundRomData.HeadTilemapWords),
                }))),
                KeyValuePair.Create((ushort)2, KraidHeadTilemapAtlas.Load(Json(new KraidHeadTilemapDocument
                {
                    Version = KraidHeadTilemapFormat.Version, Width = KraidHeadTilemapFormat.Width,
                    Height = KraidHeadTilemapFormat.Height, Cells = Cells("kraid-head-second", KraidBackgroundRomData.HeadTilemapWords),
                }))),
            });
            var meltFirstMap = MeltMap("melt-first-map");
            var meltSecondMap = MeltMap("melt-second-map");
            var gunship = GunshipLiftoffTransferDefinitions.Frames.ToArray().Select((definition, index) =>
                Characters("gunship-" + index, GunshipLiftoffTransferDefinitions.ByteCount, index)).ToArray();
            Register("gunship-order");
            if (edit == "gunship-order") Array.Reverse(gunship);
            var door = CeresDoorVisualCatalog.Load(Png("ceres-door-tiles", CeresDoorVisualRomData.TileByteCount), Json(new CeresDoorVisualDocument
            {
                Version = 1, Normal = Colors("ceres-door-normal", CeresDoorVisualRomData.SetupColorCount),
                Escape = Colors("ceres-door-escape", CeresDoorVisualRomData.SetupColorCount),
                Animation = ColorRows("ceres-door-animation", CeresDoorVisualRomData.AnimationRowCount, CeresDoorVisualRomData.AnimationColorCount),
                Mode7DoorFrames = Enumerable.Range(0, CeresDoorVisualRomData.Mode7FrameCount)
                    .Select(index => Words("ceres-door-mode7-" + index, CeresDoorVisualRomData.Mode7FrameByteCount).Select(word => (int)word).ToArray()).ToArray(),
            }));
            var special = new MotherBrainSpecialSpriteArtworkCatalog(Ordered(MotherBrainSpecialSpriteArtworkDefinitions.All.Select(definition =>
                KeyValuePair.Create(definition.SourceAddress, Characters(definition.FileName, definition.ByteCount)))));
            var torizo = new TorizoInstructionVramArtwork(TorizoInstructionVramArtworkDefinitions.All.ToArray().Select(definition =>
                Characters(definition.FileName, definition.ByteCount)).ToArray());
            var ceresTiles = new CeresEscapeTileArtwork(CeresEscapeTileArtworkDefinitions.All.ToArray().Select(definition =>
                Characters(definition.FileName, definition.ByteCount)).ToArray());
            var overlay = CeresEscapeOverlayTilemapCatalog.Load(Json(new CeresEscapeOverlayTilemapDocument
            {
                Version = CeresEscapeOverlayTilemapDefinitions.Version,
                Pages = Ordered(CeresEscapeOverlayTilemapDefinitions.All.ToArray().Select(page =>
                    KeyValuePair.Create(page.Name, Words("ceres-overlay-" + page.Name, page.WordCount)))),
            }));
            var auxiliary = EnemyAuxiliaryColorCatalog.Load(Json(new EnemyAuxiliaryColorDocument
            {
                Version = EnemyAuxiliaryColorFormat.Version,
                Palettes = Ordered(EnemyAuxiliaryColorDefinitions.All.ToArray().Select(definition =>
                    KeyValuePair.Create(definition.Id, ColorRows("auxiliary-" + definition.Id, definition.FrameCount, definition.ColorCount)))),
            }));

            return EnemyTileArtworkCatalog.FromArtworkForVerification(sheets, palettes,
                crocomireMelting: CrocomireMeltingArtwork.Load(Png("melt-first", CrocomireMeltingArtworkFormat.FirstByteCount),
                    Png("melt-second", CrocomireMeltingArtworkFormat.SecondByteCount), Json(meltFirstMap), Json(meltSecondMap)),
                spritemaps: spritemaps ?? Oam(), extendedFrames: extendedFrames ?? Extended(),
                kraidBackground: new KraidBackgroundArtwork(Map("kraid-upper", KraidBackgroundRomData.DecompressedTilemapBytes),
                    Map("kraid-lower", KraidBackgroundRomData.DecompressedTilemapBytes), heads,
                    Characters("kraid-background", KraidBackgroundRomData.RoomBackgroundTileBytes)),
                kraidColors: ColorCatalog(KraidColorCatalog.Load,
                    ("roomBackdrop", 0, KraidPaletteRomData.ColorCount(KraidPaletteSource.RoomBackdrop)),
                    ("initialTarget", 0, KraidPaletteRomData.ColorCount(KraidPaletteSource.InitialTarget)),
                    ("health", 0, KraidPaletteRomData.ColorCount(KraidPaletteSource.Health)),
                    ("secondary", 0, KraidPaletteRomData.ColorCount(KraidPaletteSource.Secondary)),
                    ("deathArm", 0, KraidPaletteRomData.ColorCount(KraidPaletteSource.DeathArm))),
                gunshipLiftoff: new GunshipLiftoffArtworkCatalog(gunship), ceresDoorVisual: door, dmaSources: dma,
                projectileSpritemaps: projectileSpritemaps ?? Projectiles(),
                magdollitePaletteCycle: ColorCatalog(MagdollitePaletteCycle.Load, ("frames", MagdollitePaletteRomData.FrameCount, MagdollitePaletteRomData.AnimatedColorCount)),
                workRobotPaletteCycle: ColorCatalog(WorkRobotPaletteCycle.Load, ("frames", WorkRobotPaletteTimingDefinitions.RecordCount, WorkRobotPaletteRomData.ColorCount)),
                crocomireColors: ColorCatalog(CrocomireColorCatalog.Load,
                    ("fightBody", 0, CrocomirePaletteRomData.FightBodyCount), ("initialWall", 0, CrocomirePaletteRomData.InitialWallCount),
                    ("initialProjectile", 0, CrocomirePaletteRomData.InitialProjectileCount), ("skeletonArm", 0, CrocomirePaletteRomData.SkeletonArmCount),
                    ("wallSpikes", 0, CrocomirePaletteRomData.WallSpikesCount)),
                draygonColors: ColorCatalog(DraygonColorCatalog.Load,
                    ("intro", 0, DraygonColorRomData.IntroCount), ("background", 0, DraygonColorRomData.BackgroundCount),
                    ("sprite", 0, DraygonColorRomData.SpriteCount), ("whiteFlash", 0, DraygonColorRomData.WhiteFlashCount),
                    ("healthBands", DraygonColorRomData.HealthBandCount, DraygonColorRomData.HealthBandColorCount)),
                phantoonColors: ColorCatalog(PhantoonColorCatalog.Load,
                    ("healthBands", PhantoonColorRomData.HealthBandCount, PhantoonColorRomData.HealthBandColorCount),
                    ("fadeOut", 0, PhantoonColorRomData.FadeOutCount), ("powerOn", 0, PhantoonColorRomData.PowerOnCount)),
                chozoAndTubeColors: ColorCatalog(ChozoAndTubeColorCatalog.Load,
                    ("tubeCracks", 0, ChozoAndTubeColorRomData.ColorCount), ("wreckedShip", 0, ChozoAndTubeColorRomData.ColorCount),
                    ("lowerNorfair", 0, ChozoAndTubeColorRomData.ColorCount)),
                sporeSpawnColors: ColorCatalog(SporeSpawnColorCatalog.Load,
                    ("spores", 0, SporeSpawnColorRomData.ColorsPerFrame), ("health", SporeSpawnColorRomData.HealthFrameCount, SporeSpawnColorRomData.ColorsPerFrame),
                    ("deathSprite", SporeSpawnColorRomData.DeathSpriteFrameCount, SporeSpawnColorRomData.ColorsPerFrame),
                    ("deathLevel", SporeSpawnColorRomData.DeathSceneFrameCount, SporeSpawnColorRomData.ColorsPerFrame),
                    ("deathBackground", SporeSpawnColorRomData.DeathSceneFrameCount, SporeSpawnColorRomData.ColorsPerFrame)),
                dachoraColors: ColorCatalog(DachoraColorCatalog.Load, ("normal", 0, DachoraColorRomData.ColorsPerFrame),
                    ("speed", DachoraColorRomData.AnimatedFrameCount, DachoraColorRomData.ColorsPerFrame),
                    ("shine", DachoraColorRomData.AnimatedFrameCount, DachoraColorRomData.ColorsPerFrame)),
                shitroidColors: ColorCatalog(ShitroidColorCatalog.Load,
                    ("normal", ShitroidColorRomData.NormalFrameCount, ShitroidColorRomData.NormalColorsPerFrame),
                    ("sidehopper", 0, ShitroidColorRomData.TargetColorCount), ("shitroid", 0, ShitroidColorRomData.TargetColorCount),
                    ("deadSidehopper", 0, ShitroidColorRomData.TargetColorCount)),
                babyMetroidCutsceneColors: ColorCatalog(BabyMetroidCutsceneColorCatalog.Load,
                    ("initial", 0, BabyMetroidCutsceneColorRomData.InitialColorCount),
                    ("fade", BabyMetroidCutsceneColorRomData.FadeFrameCount, BabyMetroidCutsceneColorRomData.FadeColorCount)),
                botwoonColors: ColorCatalog(BotwoonColorCatalog.Load,
                    ("health", BotwoonHealthPaletteDefinitions.PaletteCount, BotwoonHealthPaletteDefinitions.ColorsPerPalette)),
                motherBrainDeathColors: ColorCatalog(MotherBrainDeathColorCatalog.Load,
                    ("bodyFade", MotherBrainDeathRomData.BodyFadeFrameCount, MotherBrainDeathRomData.BodyColorCount),
                    ("legFade", MotherBrainDeathRomData.BodyFadeFrameCount, MotherBrainDeathRomData.BodyColorCount),
                    ("corpseFade", MotherBrainDeathRomData.CorpseFadeFrameCount, MotherBrainDeathRomData.CorpseColorCount),
                    ("explodedDoor", 0, MotherBrainDeathRomData.BodyColorCount)),
                zebetiteColors: ColorCatalog(ZebetiteColorCatalog.Load, ("frames", ZebetiteColorFormat.FrameCount, ZebetiteColorFormat.ColorsPerFrame)),
                norfairRidleyColors: ColorCatalog(NorfairRidleyColorCatalog.Load,
                    ("initial", 0, NorfairRidleyPaletteRomData.InitialColorCount),
                    ("reveal", NorfairRidleyPaletteRomData.RevealRowCount, NorfairRidleyPaletteRomData.RevealColorCount)),
                tourianStatueColors: ColorCatalog(TourianStatueColorCatalog.Load,
                    ("base", 0, TourianStatuePaletteRomData.BaseColorCount), ("statue", 0, TourianStatuePaletteRomData.StatueColorCount),
                    ("eye", TourianStatuePaletteRomData.EyeRowCount, TourianStatuePaletteRomData.EyeColorCount), ("grey", 0, TourianStatuePaletteRomData.GreyColorCount)),
                phantoonBg2Frames: PhantoonBg2FrameCatalog.Load(Bg2("phantoon-bg2", PhantoonBg2FrameDefinitions.Frames)),
                draygonBg2Frames: DraygonBg2FrameCatalog.Load(Bg2("draygon-bg2", DraygonBg2FrameDefinitions.Frames)),
                crocomireBg2Frames: CrocomireBg2FrameCatalog.Load(Bg2("crocomire-bg2", CrocomireBg2FrameDefinitions.Frames)),
                motherBrainBodyBg2Frames: motherBrainBodyBg2Frames ?? MotherBrainBodyBg2FrameCatalog.Load(
                    Bg2("mother-brain-body-bg2", MotherBrainBodyVisualDefinitions.Bg2Frames)),
                motherBrainCorpse: Characters("mother-brain-corpse", RoomCharacterAtlasFormat.BytesPerTile),
                motherBrainEscapeText: Characters("mother-brain-text", RoomCharacterAtlasFormat.BytesPerTile),
                motherBrainSpecialSprites: special,
                crocomireSkeleton: CrocomireSkeletonArtwork.Load(Png("crocomire-skeleton", CrocomireSkeletonTransferDefinitions.TotalByteCount)),
                torizoInstructionVram: torizo, ceresEscapeTiles: ceresTiles,
                ceresEscapeOverlayTilemaps: overlay, auxiliaryColors: auxiliary);
        }

        /// <summary>Creates a Crocomire melting tilemap document with generated cells and the selected mutation.</summary>
        /// <param name="name">Asset key used to register and select the optional cell edit.</param>
        /// <returns>A document with the production schema dimensions and generated tile references.</returns>
        private CrocomireMeltingTilemapDocument MeltMap(string name)
        {
            Register(name);
            return new CrocomireMeltingTilemapDocument
            {
                Version = CrocomireMeltingArtworkFormat.TilemapVersion,
                Width = CrocomireMeltingArtworkFormat.TilemapWidth, Height = CrocomireMeltingArtworkFormat.TilemapHeight,
                Cells = Enumerable.Range(0, CrocomireMeltingArtworkFormat.TilemapCellCount).Select(index => new CrocomireMeltingTilemapCell
                {
                    TileIndex = index == CrocomireMeltingArtworkFormat.TilemapCellCount - 1 && edit == name ? 1 : 0,
                    Palette = 0, Priority = false, FlipX = false, FlipY = false,
                }).ToArray(),
            };
        }
    }
}
