using SuperMetroid.AssetExtraction.Artwork;
using System.Collections.Immutable;
using SuperMetroid.Core.World.Artwork;
using SuperMetroid.Core.Areas;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Bosses.Ridley;
using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Verification.Enemies;

/// <summary>Creates controlled installed-enemy artwork variants for content identity checks.</summary>
internal static class EnemyContentIdentityFixtureVerification
{
    /// <summary>JSON options: Camel-case property names, indented output.</summary>
    private static readonly JsonSerializerOptions CamelCaseIndentedJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
    /// <summary>JSON options: Camel-case property names.</summary>
    private static readonly JsonSerializerOptions CamelCaseJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>Small authored presentation data loaded through production PNG/JSON compilers.</summary>
    internal sealed class EnemyIdentityFixture(string? edit = null, bool reverse = false)
    {
        /// <summary>Names every artwork component for which this fixture can produce a distinct edit.</summary>
        public List<string> Edits { get; } = [];
        /// <summary>Records a supported edit once while retaining fixture construction order.</summary>
        private void Register(string name)
        {
            if (!Edits.Contains(name, StringComparer.Ordinal)) Edits.Add(name);
        }

        /// <summary>Serializes fixture documents through the same camel-case JSON shape as installed artwork.</summary>
        public MemoryStream Json(object document) => new(JsonSerializer.SerializeToUtf8Bytes(document,
            (edit == "json-indent" ? CamelCaseIndentedJson : CamelCaseJson)));

        /// <summary>Writes indexed tile pixels with an optional first-pixel edit or cosmetic palette change.</summary>
        private MemoryStream Png(string name, int bytes, int seed = 0)
        {
            Register(name);
            int tiles = bytes / RoomCharacterAtlasFormat.BytesPerTile;
            int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tiles);
            int width = columns * 8, height = ((tiles + columns - 1) / columns) * 8;
            var pixels = new byte[width * height];
            pixels[0] = (byte)(seed ^ (edit == name ? 1 : 0));
            var png = new MemoryStream();
            IndexedPng.Write(png, new(checked((ushort)width), checked((ushort)height)), pixels, Enumerable.Range(0, 16).Select(index =>
                new Rgba32((byte)(edit == "png-colors" ? 255 - index : index), 0, 0)).ToArray());
            png.Position = 0;
            return png;
        }

        /// <summary>Compiles fixture PNG pixels into a production room-character atlas.</summary>
        private RoomCharacterAtlas Characters(string name, int bytes, int seed = 0) =>
            RoomCharacterAtlasPng.Read(Png(name, bytes, seed), bytes);

        /// <summary>Changes the final tilemap cell only when its named edit is selected.</summary>
        private RoomBackgroundTilemapCell[] Cells(string name, int count)
        {
            Register(name);
            return Enumerable.Range(0, count).Select(index => new RoomBackgroundTilemapCell
            {
                TileColumn = index == count - 1 && edit == name ? 1 : 0,
                TileRow = 0, Palette = 0, Priority = false, FlipX = false, FlipY = false,
            }).ToArray();
        }

        /// <summary>Builds the requested number of production background pages from editable cells.</summary>
        private RoomBackgroundTilemapAtlas Map(string name, int bytes) => RoomBackgroundTilemapAtlas.Load(Json(
            new RoomBackgroundTilemapDocument
            {
                Version = RoomBackgroundTilemapFormat.Version,
                Pages = Enumerable.Range(0, bytes / RoomBackgroundTilemapFormat.BytesPerPage).Select(page =>
                    new RoomBackgroundTilemapPage { Cells = Cells(name + "-page-" + page, RoomBackgroundTilemapFormat.CellsPerPage) }).ToArray(),
            }), bytes);

        /// <summary>Builds a word sequence whose final element can carry a controlled edit.</summary>
        private ushort[] Words(string name, int count)
        {
            Register(name);
            var words = Enumerable.Range(0, count).Select(index => (ushort)index).ToArray();
            if (edit == name) words[^1] ^= 1;
            return words;
        }

        /// <summary>Builds five-bit palette colors with an optional final red-channel mutation.</summary>
        private PaletteRgb5[] Colors(string name, int count, int seed = 0)
        {
            Register(name);
            return Enumerable.Range(0, count).Select(index => new PaletteRgb5
            {
                Red = ((seed + index) % 32) ^ (edit == name && index == count - 1 ? 1 : 0),
                Green = 0, Blue = 0,
            }).ToArray();
        }

        /// <summary>Builds distinguishable palette rows and can reverse their authored order.</summary>
        private PaletteRgb5[][] ColorRows(string name, int count, int colors)
        {
            Register(name + "-order");
            var rows = Enumerable.Range(0, count).Select(index => Colors(name + "-row-" + index, colors, index)).ToArray();
            if (edit == name + "-order") Array.Reverse(rows);
            return rows;
        }

        /// <summary>Loads a production color catalog from one or more scalar or animated palette fields.</summary>
        private T ColorCatalog<T>(Func<Stream, T> load, params EnemyContentIdentityFixtureField[] fields)
        {
            var document = new Dictionary<string, object> { ["version"] = 1 };
            foreach (var field in fields)
            {
                string name = typeof(T).Name + "." + field.Name;
                document.Add(field.Name, field.Rows == 0 ? Colors(name, field.Colors) : ColorRows(name, field.Rows, field.Colors));
            }
            return load(Json(document));
        }

        /// <summary>Reverses dictionary insertion order to verify identity hashing uses canonical keys.</summary>
        private Dictionary<TKey, TValue> Ordered<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> source) where TKey : notnull =>
            (reverse ? source.Reverse() : source).ToDictionary();

        /// <summary>Assembles every installed enemy presentation domain with one optional controlled edit.</summary>
        public InstalledEnemyArtwork Build(EnemySpritemapCatalog? spritemaps = null,
            EnemyExtendedFrameCatalog? extendedFrames = null,
            EnemyProjectileSpritemapCatalog? projectileSpritemaps = null,
            MotherBrainBodyBg2FrameCatalog? motherBrainBodyBg2Frames = null)
        {
            Register("sheet-id");
            EnemyDefinitionId firstId = (EnemyDefinitionId)(edit == "sheet-id" ? 3 : 1);
            var sheets = Ordered(new[]
            {
                KeyValuePair.Create(firstId, Characters("sheet-first", RoomCharacterAtlasFormat.BytesPerTile)),
                KeyValuePair.Create((EnemyDefinitionId)2, Characters("sheet-second", RoomCharacterAtlasFormat.BytesPerTile, 1)),
            });
            var palettes = Ordered(new[]
            {
                KeyValuePair.Create(firstId, EnemyPaletteSheetFile.Read(Json(new EnemyPaletteSheetDocument
                    { Version = 1, Colors = Colors("palette-first", EnemyPaletteSheet.ColorCount) }))),
                KeyValuePair.Create((EnemyDefinitionId)2, EnemyPaletteSheetFile.Read(Json(new EnemyPaletteSheetDocument
                    { Version = 1, Colors = Colors("palette-second", EnemyPaletteSheet.ColorCount) }))),
            });
            var heads = Ordered(new[]
            {
                KeyValuePair.Create(0, KraidHeadTilemapAtlas.Load(Json(new KraidHeadTilemapDocument
                {
                    Version = KraidHeadTilemapFormat.Version, Width = KraidHeadTilemapFormat.Width,
                    Height = KraidHeadTilemapFormat.Height, Cells = Cells("kraid-head-first", KraidBackgroundRomData.HeadTilemapWords),
                }))),
                KeyValuePair.Create(1, KraidHeadTilemapAtlas.Load(Json(new KraidHeadTilemapDocument
                {
                    Version = KraidHeadTilemapFormat.Version, Width = KraidHeadTilemapFormat.Width,
                    Height = KraidHeadTilemapFormat.Height, Cells = Cells("kraid-head-second", KraidBackgroundRomData.HeadTilemapWords),
                }))),
            });
            var meltFirstMap = MeltMap("melt-first-map");
            var meltSecondMap = MeltMap("melt-second-map");
            var gunship = GunshipLiftoffTransferDefinitions.Frames.Select((definition, index) =>
                Characters("gunship-" + index, GunshipLiftoffTransferDefinitions.ByteCount, index)).ToArray();
            Register("gunship-order");
            if (edit == "gunship-order") Array.Reverse(gunship);
            var door = CeresDoorVisualFiles.Read(Png("ceres-door-tiles", CeresDoorVisualRomData.TileByteCount), Json(new CeresDoorVisualDocument
            {
                Version = 1, Normal = Colors("ceres-door-normal", CeresDoorVisualRomData.SetupColorCount),
                Escape = Colors("ceres-door-escape", CeresDoorVisualRomData.SetupColorCount),
                Animation = ColorRows("ceres-door-animation", CeresDoorVisualRomData.AnimationRowCount, CeresDoorVisualRomData.AnimationColorCount),
                Mode7DoorFrames = Enumerable.Range(0, CeresDoorVisualRomData.Mode7FrameCount)
                    .Select(index => Words("ceres-door-mode7-" + index, CeresDoorVisualRomData.Mode7FrameByteCount).Select(word => (int)word).ToArray()).ToArray(),
            }));
            var special = new MotherBrainSpecialSpriteArtworkCatalog(Ordered(MotherBrainSpecialSpriteArtworkDefinitions.All.Select(definition =>
                KeyValuePair.Create(definition.Sheet, Characters(definition.FileName, definition.ByteCount)))));
            var torizo = new TorizoSwapArtwork(TorizoSwapArtworkDefinitions.All.Select(definition =>
                Characters(definition.FileName, definition.ByteCount)).ToArray());
            var ceresTiles = new CeresEscapeTileArtwork(CeresEscapeTileArtworkDefinitions.All.Select(definition =>
                Characters(definition.FileName, definition.ByteCount)).ToArray());
            var overlay = CeresEscapeOverlayTilemapCatalog.Load(Json(new CeresEscapeOverlayTilemapDocument
            {
                Version = CeresEscapeOverlayTilemapDefinitions.Version,
                Pages = Ordered(CeresEscapeOverlayTilemapDefinitions.All.Select(page =>
                    KeyValuePair.Create(page.Name, Words("ceres-overlay-" + page.Name, page.WordCount)))),
            }));
            var auxiliary = EnemyAuxiliaryColorFiles.Read(Json(new EnemyAuxiliaryColorDocument
            {
                Version = EnemyAuxiliaryColorFormat.Version,
                Palettes = Ordered(EnemyAuxiliaryColorDefinitions.All.Select(definition =>
                    KeyValuePair.Create(definition.Id, ColorRows("auxiliary-" + definition.Id, definition.FrameCount, definition.ColorCount)))),
            }));

            return new InstalledEnemyArtwork(
                EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = sheets, Palettes = palettes, Spritemaps = spritemaps ?? Oam(), ExtendedFrames = extendedFrames ?? Extended(), GunshipLiftoff = new GunshipLiftoffArtworkCatalog(gunship), ProjectileSpritemaps = projectileSpritemaps ?? Projectiles(), MagdollitePaletteCycle = ColorCatalog(MagdollitePaletteCycleFile.Read, new EnemyContentIdentityFixtureField("frames", MagdollitePaletteDefinitions.FrameCount, MagdollitePaletteDefinitions.AnimatedColorCount)), WorkRobotPaletteCycle = ColorCatalog(WorkRobotPaletteCycleFile.Read, new EnemyContentIdentityFixtureField("frames", WorkRobotPaletteTimingDefinitions.RecordCount, WorkRobotPaletteDefinitions.ColorCount)), ChozoAndTubeColors = ColorCatalog(ChozoAndTubeColorFile.Read,
                    new EnemyContentIdentityFixtureField("tubeCracks", 0, ChozoAndTubeColorDefinitions.ColorCount), new EnemyContentIdentityFixtureField("wreckedShip", 0, ChozoAndTubeColorDefinitions.ColorCount),
                    new EnemyContentIdentityFixtureField("lowerNorfair", 0, ChozoAndTubeColorDefinitions.ColorCount)), DachoraColors = ColorCatalog(DachoraColorFile.Read, new EnemyContentIdentityFixtureField("normal", 0, DachoraColorDefinitions.ColorsPerFrame),
                    new EnemyContentIdentityFixtureField("speed", DachoraColorDefinitions.AnimatedFrameCount, DachoraColorDefinitions.ColorsPerFrame),
                    new EnemyContentIdentityFixtureField("shine", DachoraColorDefinitions.AnimatedFrameCount, DachoraColorDefinitions.ColorsPerFrame)), AuxiliaryColors = auxiliary }),
                new BossArtworkCatalog { CrocomireMelting = CrocomireMeltingArtwork.Load(RoomCharacterAtlasPng.Read(Png("melt-first", CrocomireMeltingArtworkFormat.FirstByteCount), CrocomireMeltingArtworkFormat.FirstByteCount),
                    RoomCharacterAtlasPng.Read(Png("melt-second", CrocomireMeltingArtworkFormat.SecondByteCount), CrocomireMeltingArtworkFormat.SecondByteCount), Json(meltFirstMap), Json(meltSecondMap)), KraidBackground = new KraidBackgroundArtwork(Map("kraid-upper", KraidBackgroundRomData.DecompressedTilemapBytes),
                    Map("kraid-lower", KraidBackgroundRomData.DecompressedTilemapBytes), heads,
                    Characters("kraid-background", KraidBackgroundRomData.RoomBackgroundTileBytes)), KraidColors = ColorCatalog(KraidColorFile.Read,
                    new EnemyContentIdentityFixtureField("roomBackdrop", 0, KraidPaletteRomData.ColorCount(KraidPaletteSource.RoomBackdrop)),
                    new EnemyContentIdentityFixtureField("initialTarget", 0, KraidPaletteRomData.ColorCount(KraidPaletteSource.InitialTarget)),
                    new EnemyContentIdentityFixtureField("health", 0, KraidPaletteRomData.ColorCount(KraidPaletteSource.Health)),
                    new EnemyContentIdentityFixtureField("secondary", 0, KraidPaletteRomData.ColorCount(KraidPaletteSource.Secondary)),
                    new EnemyContentIdentityFixtureField("deathArm", 0, KraidPaletteRomData.ColorCount(KraidPaletteSource.DeathArm))), CrocomireColors = ColorCatalog(CrocomireColorFile.Read,
                    new EnemyContentIdentityFixtureField("fightBody", 0, CrocomirePaletteRomData.FightBodyCount), new EnemyContentIdentityFixtureField("initialWall", 0, CrocomirePaletteRomData.InitialWallCount),
                    new EnemyContentIdentityFixtureField("initialProjectile", 0, CrocomirePaletteRomData.InitialProjectileCount), new EnemyContentIdentityFixtureField("skeletonArm", 0, CrocomirePaletteRomData.SkeletonArmCount),
                    new EnemyContentIdentityFixtureField("wallSpikes", 0, CrocomirePaletteRomData.WallSpikesCount)), DraygonColors = ColorCatalog(DraygonColorFile.Read,
                    new EnemyContentIdentityFixtureField("intro", 0, DraygonColorRomData.IntroCount), new EnemyContentIdentityFixtureField("background", 0, DraygonColorRomData.BackgroundCount),
                    new EnemyContentIdentityFixtureField("sprite", 0, DraygonColorRomData.SpriteCount), new EnemyContentIdentityFixtureField("whiteFlash", 0, DraygonColorRomData.WhiteFlashCount),
                    new EnemyContentIdentityFixtureField("healthBands", DraygonColorRomData.HealthBandCount, DraygonColorRomData.HealthBandColorCount)), PhantoonColors = ColorCatalog(PhantoonColorFile.Read,
                    new EnemyContentIdentityFixtureField("healthBands", PhantoonColorRomData.HealthBandCount, PhantoonColorRomData.HealthBandColorCount),
                    new EnemyContentIdentityFixtureField("fadeOut", 0, PhantoonColorRomData.FadeOutCount), new EnemyContentIdentityFixtureField("powerOn", 0, PhantoonColorRomData.PowerOnCount)), SporeSpawnColors = ColorCatalog(SporeSpawnColorFile.Read,
                    new EnemyContentIdentityFixtureField("spores", 0, SporeSpawnColorDefinitions.ColorsPerFrame), new EnemyContentIdentityFixtureField("health", SporeSpawnColorDefinitions.HealthFrameCount, SporeSpawnColorDefinitions.ColorsPerFrame),
                    new EnemyContentIdentityFixtureField("deathSprite", SporeSpawnColorDefinitions.DeathSpriteFrameCount, SporeSpawnColorDefinitions.ColorsPerFrame),
                    new EnemyContentIdentityFixtureField("deathLevel", SporeSpawnColorDefinitions.DeathSceneFrameCount, SporeSpawnColorDefinitions.ColorsPerFrame),
                    new EnemyContentIdentityFixtureField("deathBackground", SporeSpawnColorDefinitions.DeathSceneFrameCount, SporeSpawnColorDefinitions.ColorsPerFrame)), ShitroidColors = ColorCatalog(ShitroidColorFile.Read,
                    new EnemyContentIdentityFixtureField("normal", ShitroidColorDefinitions.NormalFrameCount, ShitroidColorDefinitions.NormalColorsPerFrame),
                    new EnemyContentIdentityFixtureField("sidehopper", 0, ShitroidColorDefinitions.TargetColorCount), new EnemyContentIdentityFixtureField("shitroid", 0, ShitroidColorDefinitions.TargetColorCount),
                    new EnemyContentIdentityFixtureField("deadSidehopper", 0, ShitroidColorDefinitions.TargetColorCount)), BabyMetroidCutsceneColors = ColorCatalog(BabyMetroidCutsceneColorFile.Read,
                    new EnemyContentIdentityFixtureField("initial", 0, BabyMetroidCutsceneColorDefinitions.InitialColorCount),
                    new EnemyContentIdentityFixtureField("fade", BabyMetroidCutsceneColorDefinitions.FadeFrameCount, BabyMetroidCutsceneColorDefinitions.FadeColorCount)), BotwoonColors = ColorCatalog(BotwoonColorFile.Read,
                    new EnemyContentIdentityFixtureField("health", BotwoonHealthPaletteDefinitions.PaletteCount, BotwoonHealthPaletteDefinitions.ColorsPerPalette)), MotherBrainDeathColors = ColorCatalog(MotherBrainDeathColorFile.Read,
                    new EnemyContentIdentityFixtureField("bodyFade", MotherBrainDeathDefinitions.BodyFadeFrameCount, MotherBrainDeathDefinitions.BodyColorCount),
                    new EnemyContentIdentityFixtureField("legFade", MotherBrainDeathDefinitions.BodyFadeFrameCount, MotherBrainDeathDefinitions.BodyColorCount),
                    new EnemyContentIdentityFixtureField("corpseFade", MotherBrainDeathDefinitions.CorpseFadeFrameCount, MotherBrainDeathDefinitions.CorpseColorCount),
                    new EnemyContentIdentityFixtureField("explodedDoor", 0, MotherBrainDeathDefinitions.BodyColorCount)), ZebetiteColors = ColorCatalog(ZebetiteColorFile.Read, new EnemyContentIdentityFixtureField("frames", ZebetiteColorFormat.FrameCount, ZebetiteColorFormat.ColorsPerFrame)), NorfairRidleyColors = ColorCatalog(NorfairRidleyColorFile.Read,
                    new EnemyContentIdentityFixtureField("initial", 0, NorfairRidleyPaletteRomData.InitialColorCount),
                    new EnemyContentIdentityFixtureField("reveal", NorfairRidleyPaletteRomData.RevealRowCount, NorfairRidleyPaletteRomData.RevealColorCount)), PhantoonBg2Frames = PhantoonBg2FrameCatalog.Load(Bg2("phantoon-bg2", PhantoonBg2FrameDefinitions.Frames)), DraygonBg2Frames = DraygonBg2FrameCatalog.Load(Bg2("draygon-bg2", DraygonBg2FrameDefinitions.Frames)), CrocomireBg2Frames = CrocomireBg2FrameCatalog.Load(Bg2("crocomire-bg2", CrocomireBg2FrameDefinitions.Frames)), MotherBrainBodyBg2Frames = motherBrainBodyBg2Frames ?? MotherBrainBodyBg2FrameCatalog.Load(
                    Bg2("mother-brain-body-bg2", MotherBrainBodyVisualDefinitions.Bg2Frames)), MotherBrainCorpse = Characters("mother-brain-corpse", RoomCharacterAtlasFormat.BytesPerTile), MotherBrainEscapeText = Characters("mother-brain-text", RoomCharacterAtlasFormat.BytesPerTile), MotherBrainSpecialSprites = special, CrocomireSkeleton = CrocomireSkeletonArtwork.Load(RoomCharacterAtlasPng.Read(Png("crocomire-skeleton", CrocomireSkeletonTransferDefinitions.TotalByteCount), CrocomireSkeletonTransferDefinitions.TotalByteCount)), TorizoSwaps = torizo },
                new AreaArtworkCatalog { CeresDoorVisual = door, TourianStatueColors = ColorCatalog(TourianStatueColorFile.Read,
                    new EnemyContentIdentityFixtureField("base", 0, TourianStatuePaletteRomData.BaseColorCount), new EnemyContentIdentityFixtureField("statue", 0, TourianStatuePaletteRomData.StatueColorCount),
                    new EnemyContentIdentityFixtureField("eye", TourianStatuePaletteRomData.EyeRowCount, TourianStatuePaletteRomData.EyeColorCount), new EnemyContentIdentityFixtureField("grey", 0, TourianStatuePaletteRomData.GreyColorCount)), CeresEscapeTiles = ceresTiles, CeresEscapeOverlayTilemaps = overlay });
        }

        /// <summary>Changes the final Crocomire melt tile only for the selected map edit.</summary>
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

        /// <summary>Builds two OAM parts while varying geometry, attributes, ordering, or count.</summary>
        private SpriteVisualPart[] Parts(string family, bool mutate)
        {
            if (mutate)
                foreach (string component in new[] { "x", "y", "size", "tile-column", "tile-row", "palette", "priority", "flip-x", "flip-y", "order", "count" })
                    Register(family + "-" + component);
            var first = new SpriteVisualPart
            {
                OffsetX = 0, OffsetY = 0, Size = 8, TileColumn = 0, TileRow = 0,
                Palette = 1, Priority = 1, FlipX = false, FlipY = false,
            };
            var second = first with { OffsetX = 1, TileColumn = 1, Palette = 2 };
            string? change = mutate && edit?.StartsWith(family + "-", StringComparison.Ordinal) == true
                ? edit[(family.Length + 1)..] : null;
            second = change switch
            {
                "x" => second with { OffsetX = 2 }, "y" => second with { OffsetY = 1 },
                "size" => second with { Size = 16 }, "tile-column" => second with { TileColumn = 2 },
                "tile-row" => second with { TileRow = 1 }, "palette" => second with { Palette = 3 },
                "priority" => second with { Priority = 2 }, "flip-x" => second with { FlipX = true },
                "flip-y" => second with { FlipY = true }, _ => second,
            };
            return change switch { "order" => [second, first], "count" => [first], _ => [first, second] };
        }

        /// <summary>Builds spritemap frames and physical-to-display bindings in optional reverse key order.</summary>
        public EnemySpritemapDocument OamDocument()
        {
            var definitions = EnemySpritemapDefinitions.Frames.ToArray();
            Register("oam-binding");
            return new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.Current,
                Frames = Ordered(definitions.Select((frame, index) =>
                    KeyValuePair.Create(frame.Name, Parts("oam", index == definitions.Length - 1)))),
                DisplayFrames = Ordered(definitions.Select((frame, index) => KeyValuePair.Create(frame.Name,
                    index == 0 && edit == "oam-binding"
                        ? definitions.First(other => other.Bank == frame.Bank && other.Name != frame.Name).Name : frame.Name))),
            };
        }
        /// <summary>Compiles fixture OAM data through the installed spritemap loader.</summary>
        public EnemySpritemapCatalog Oam() => EnemySpritemapDocuments.Load(Json(OamDocument()));

        /// <summary>Builds extended sprite components with independently editable offsets and frame bindings.</summary>
        public EnemyExtendedFrameDocument ExtendedDocument()
        {
            var definitions = EnemyExtendedFrameDefinitions.Frames.ToArray();
            foreach (string component in new[] { "x", "y", "order", "count", "binding" }) Register("extended-" + component);
            var frames = definitions.Select((frame, index) =>
            {
                bool last = index == definitions.Length - 1;
                var first = new EnemyExtendedVisualComponent { OffsetX = 0, OffsetY = 0, Parts = Parts("extended-parts", false) };
                var second = new EnemyExtendedVisualComponent
                {
                    OffsetX = last && edit == "extended-x" ? 2 : 1,
                    OffsetY = last && edit == "extended-y" ? 1 : 0,
                    Parts = Parts("extended-parts", last),
                };
                EnemyExtendedVisualComponent[] components = last && edit == "extended-order" ? [second, first]
                    : last && edit == "extended-count" ? [first] : [first, second];
                return KeyValuePair.Create(frame.Name, components);
            });
            return new EnemyExtendedFrameDocument
            {
                Version = (int)EnemyExtendedFrameSchema.Current, Frames = Ordered(frames),
                DisplayFrames = Ordered(definitions.Select((frame, index) =>
                    KeyValuePair.Create(frame.Name, index == 0 && edit == "extended-binding" ? definitions[1].Name : frame.Name))),
            };
        }
        /// <summary>Compiles extended frame fixtures through the installed catalog loader.</summary>
        public EnemyExtendedFrameCatalog Extended() => EnemyExtendedFrameDocuments.Load(Json(ExtendedDocument()));

        /// <summary>Builds ordinary and instruction-program projectile OAM frames.</summary>
        public EnemyProjectileSpritemapDocument ProjectileDocument()
        {
            var frames = EnemyProjectileSpritemapDefinitions.Frames.ToArray();
            var programs = EnemyProjectilePresentationFrameSets.All.ToArray();
            return new EnemyProjectileSpritemapDocument
            {
                Version = (int)EnemyProjectileSpritemapVersion.Current,
                Frames = Ordered(frames.Select((frame, index) =>
                    KeyValuePair.Create(frame.Name, Parts("projectile", index == frames.Length - 1)))),
                ProgramFrames = Ordered(programs.Select((frame, index) =>
                    KeyValuePair.Create(frame.Name, Parts("projectile-program", index == programs.Length - 1)))),
            };
        }
        /// <summary>Compiles both projectile frame namespaces into one installed catalog.</summary>
        public EnemyProjectileSpritemapCatalog Projectiles() => EnemyProjectileSpritemapCatalog.Load(Json(ProjectileDocument()));

        /// <summary>Builds BG2 frame writes whose tiles, placement, order, count, or run boundaries can vary.</summary>
        private MemoryStream Bg2(string family, ImmutableArray<EnemySpritemapDefinition> definitions)
        {
            foreach (string component in new[] { "tiles", "x", "y", "order", "count", "run-boundary" }) Register(family + "-" + component);
            var frames = definitions.SelectToArray((frame, index) =>
            {
                bool first = index == 0;
                var a = new EnemyBg2WriteDocument { X = 0, Y = 0, Tiles = [1, 2] };
                var b = new EnemyBg2WriteDocument
                {
                    X = first && edit == family + "-x" ? 2 : 1,
                    Y = first && edit == family + "-y" ? 2 : 1,
                    Tiles = first && edit == family + "-tiles" ? [3, 5] : [3, 4],
                };
                EnemyBg2WriteDocument[] writes = first && edit == family + "-order" ? [b, a]
                    : first && edit == family + "-count" ? [a]
                    // Same ordered tile payload, different command boundaries and destinations.
                    : first && edit == family + "-run-boundary" ? [a with { Tiles = [1] }, a with { X = 1, Tiles = [2] }, b]
                    : [a, b];
                return KeyValuePair.Create(frame.Name, writes);
            });
            return Json(new EnemyBg2FrameDocument { Version = 1, Frames = Ordered(frames) });
        }
    }
}
