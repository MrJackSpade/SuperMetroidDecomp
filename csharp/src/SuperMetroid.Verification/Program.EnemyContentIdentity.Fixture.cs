using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Small authored presentation data loaded through production PNG/JSON compilers.</summary>
    private sealed partial class EnemyIdentityFixture(string? edit = null, bool reverse = false)
    {
        public List<string> Edits { get; } = [];
        private void Register(string name)
        {
            if (!Edits.Contains(name, StringComparer.Ordinal)) Edits.Add(name);
        }

        public MemoryStream Json(object document) => new(JsonSerializer.SerializeToUtf8Bytes(document,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = edit == "json-indent" }));

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

        private RoomCharacterAtlas Characters(string name, int bytes, int seed = 0) =>
            RoomCharacterAtlas.Load(Png(name, bytes, seed), bytes);

        private RoomBackgroundTilemapCell[] Cells(string name, int count)
        {
            Register(name);
            return Enumerable.Range(0, count).Select(index => new RoomBackgroundTilemapCell
            {
                TileColumn = index == count - 1 && edit == name ? 1 : 0,
                TileRow = 0, Palette = 0, Priority = false, FlipX = false, FlipY = false,
            }).ToArray();
        }

        private RoomBackgroundTilemapAtlas Map(string name, int bytes) => RoomBackgroundTilemapAtlas.Load(Json(
            new RoomBackgroundTilemapDocument
            {
                Version = RoomBackgroundTilemapFormat.Version,
                Pages = Enumerable.Range(0, bytes / RoomBackgroundTilemapFormat.BytesPerPage).Select(page =>
                    new RoomBackgroundTilemapPage { Cells = Cells(name + "-page-" + page, RoomBackgroundTilemapFormat.CellsPerPage) }).ToArray(),
            }), bytes);

        private ushort[] Words(string name, int count)
        {
            Register(name);
            var words = Enumerable.Range(0, count).Select(index => (ushort)index).ToArray();
            if (edit == name) words[^1] ^= 1;
            return words;
        }

        private PaletteRgb5[] Colors(string name, int count, int seed = 0)
        {
            Register(name);
            return Enumerable.Range(0, count).Select(index => new PaletteRgb5
            {
                Red = ((seed + index) % 32) ^ (edit == name && index == count - 1 ? 1 : 0),
                Green = 0, Blue = 0,
            }).ToArray();
        }

        private PaletteRgb5[][] ColorRows(string name, int count, int colors)
        {
            Register(name + "-order");
            var rows = Enumerable.Range(0, count).Select(index => Colors(name + "-row-" + index, colors, index)).ToArray();
            if (edit == name + "-order") Array.Reverse(rows);
            return rows;
        }

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

        private Dictionary<TKey, TValue> Ordered<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> source) where TKey : notnull =>
            (reverse ? source.Reverse() : source).ToDictionary();

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
