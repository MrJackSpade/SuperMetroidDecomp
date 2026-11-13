using SuperMetroid.AssetExtraction.CartridgeTables;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Progression;

using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Builds a constructed pause presentation from retail source data and isolated fixture visuals.</summary>
internal static class PauseMenuFixtureVerification
{
    /// <summary>
    /// Translates constructed cartridge visuals into the installed pause contract without mutating shared retail catalog data:
    /// the import's read of a constructed installation, whose game catalog the pause binds (#1616).
    /// </summary>
    internal static ImportAreaMapPresentation CreateConstructedPausePresentation(
        CartridgeImportAddressSpace bus)
    {
        ImportAreaMapPresentation stock = RetailImportPresentationFixture();
        AreaMapPresentation game = stock.Game;
        var installation = RepositoryInstallation.Installation;
        byte[] Read(int address, int count) => RomDataReader.ReadFixedBank(
            CartridgeImportSource.Require(bus), address, count);
        T Document<T>(string name) => JsonSerializer.Deserialize<T>(
            File.ReadAllBytes(Path.Combine(installation.MapDirectory, name)),
            ArtworkJson.Options) ?? throw new InvalidDataException(name);
        MemoryStream Json<T>(T document) => new(JsonSerializer.SerializeToUtf8Bytes(
            document, ArtworkJson.Options));

        // The pause colours come from the constructed ROM, as the import reads them; the other screens' are stock's.
        var pauseColors = new SnesCgram();
        CartridgePaletteImporter.LoadToCgram(pauseColors, bus, MapSourceDefinitions.Artwork.PauseColors);
        MapStaticPalettes stockPalettes = stock.Palettes;
        var palettes = new MapStaticPalettes(pauseColors.Colors.ToArray(), stockPalettes.FileSelect.ToArray(),
            Enum.GetValues<AreaId>().Where(area => area != AreaId.Ceres)
                .ToDictionary(area => area, area => stockPalettes.World(area).ToArray()));
        var areas = Enum.GetValues<AreaId>().Select(game.Get).ToArray();
        areas[AreaIds.ToIndex(AreaId.Crateria)] = AreaMapImporter.Load(bus, AreaId.Crateria);
        var backdrops = PauseBackdropPresentation.FromTilemaps(
            Enum.GetValues<AreaId>().Select(_ => BackgroundTileWordBytes.Tiles(Read(0xb6e000, 0x0800))).ToArray(),
            BackgroundTileWordBytes.Tiles(Read((int)PauseMenuSourceDefinitions.PauseButtonTilemap, PauseBackdropDefinitions.ButtonCells * 2)));
        var hud = new byte[HudTileAtlasFormat.ByteCount];
        Read(MapSourceDefinitions.Artwork.HudCharacters, HudTileAtlasFormat.CharacterByteCount).CopyTo(hud, 0);

        var singlePart = new SpriteVisualPart { OffsetX = 0, OffsetY = 0,
            TileColumn = 0, TileRow = 0, Size = 8, Priority = 0, Palette = null,
            FlipX = false, FlipY = false };
        var sprites = Document<MapSpriteDocument>(MapSpriteFormat.JsonFile);
        foreach (string name in new[] { "Indicator.Frame0", "Indicator.Frame1", "Indicator.Frame2" })
            sprites.Frames[name] = [singlePart];
        using var spriteJson = Json(sprites);
        // The map sprites' page is import data no installation holds: the cartridge-derived extraction's (#1636).
        using var spritePng = File.OpenRead(Path.Combine(SuperMetroid.TestSupport.RepositoryInstallation.Verification.CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.MapDirectoryName), MapSpriteFormat.PngFile));
        var selectors = Document<PauseSelectorDocument>(PauseSelectorFormat.FileName);
        selectors.Anchors[PauseSelectorAnchors.Anchor(PauseEquipmentCategory.Suits, 2)] = new(0x90, 0x70);
        selectors.Anchors[PauseSelectorAnchors.Anchor(PauseEquipmentCategory.Suits, 3)] = new(0xa0, 0x80);
        foreach (string name in selectors.Frames.Keys.ToArray()) selectors.Frames[name] = [singlePart];
        using var selectorJson = Json(selectors);
        PauseSelectorSourcePresentation constructedSelectors = PauseSelectorSourcePresentation.Load(selectorJson);
        // The original synthetic ROM supplied zero-part reserve-strip maps. Keep
        // that fixture isolation while reserve state and digits still execute.
        var reserveTanks = Document<PauseReserveTankDocument>(PauseReserveTankFormat.FileName);
        foreach (string name in reserveTanks.Frames.Keys.ToArray()) reserveTanks.Frames[name] = [];
        using var reserveJson = Json(reserveTanks);
        PauseReserveTankSourcePresentation constructedTanks = PauseReserveTankSourcePresentation.Load(reserveJson);
        // Every part comes from the stock catalog's same-named property unless the fixture constructs it above.
        var parts = new AreaMapPresentationParts
        {
            Areas = areas,
            ContentIdentity = "constructed-pause-interaction-fixture",
            HighlightCycle = game.HighlightCycle,
            Backdrops = palettes.Backdrops(),
            Labels = game.Labels,
            Stations = game.Stations,
            Landmarks = game.Landmarks,
            SaveMarkers = game.SaveMarkers,
            Arrows = game.Arrows,
            PauseSelectors = constructedSelectors.Presentation,
            PauseReserveTanks = constructedTanks.Presentation,
            EscapeTimer = game.EscapeTimer,
            GameplayHud = game.GameplayHud,
            GameOver = game.GameOver,
            GameOptions = game.GameOptions,
            FileSelect = game.FileSelect,
            EscapeTypewriter = game.EscapeTypewriter,
            IntroNarration = game.IntroNarration,
            EndingText = game.EndingText,
            TitleBackdrop = game.TitleBackdrop,
            TitleGradient = game.TitleGradient,
            EndingSpaceBackdrops = game.EndingSpaceBackdrops,
            MotherBrainRainbowPalette = game.MotherBrainRainbowPalette,
            CeresHaze = game.CeresHaze,
            PowerBombGlowColors = game.PowerBombGlowColors,
            CeresRidleyGetawayBoostDelays = game.CeresRidleyGetawayBoostDelays,
        };
        // The screens and the HUD are pictured from the constructed ROM's characters (#1466).
        return InstalledFixtures.Record(new ImportAreaMapPresentation(new AreaMapPresentation(parts), new InstalledPresentationSources(
            stock.StaffCredits, stock.EndingTextPresentation, stock.Screens,
            stock.HighlightColours, constructedSelectors, constructedTanks, stock.GameplayHud, stock.GameOptions)
            { GameOver = stock.GameOverSource },
            new ImportPresentationCharacters(new MapCharacterArtwork(
                new MapTileCharacters(Read(MapSourceDefinitions.Artwork.MapCharacters, MapTileAtlasFormat.ByteCount)), new HudTileAtlas(hud),
                new MapTileCharacters(Read(MapSourceDefinitions.Artwork.PauseInterfaceCharacters, PauseTileAtlasFormat.ByteCount))),
                MapSpriteArtwork.Read(spriteJson, spritePng), stock.EscapeTimerTiles, stock.RoomFxAnimatedTiles, stock.WorldCharacters,
                stock.IntroFont, stock.EndingFont, stock.TitleGraphics),
            new ImportPresentationColours(stock.SamusColours, stock.TitlePalette,
                palettes, stock.CeresRidleyColors, stock.CeresRidleyFloorColors,
                stock.MotherBrainHealthPalette, stock.MotherBrainRoomColors,
                stock.RoomFxPaletteBlends, stock.RoomPaletteFx, stock.MotherBrainRainbowColours),
            new ImportPresentationDocuments(stock.FileSelect,
                new PauseScreenArtwork(backdrops, stock.Pause.Wireframes, stock.Pause.EquipmentBase, stock.Pause.EquipmentLabels,
                    stock.Pause.ReserveUi), stock.RoomFxPages, stock.Messages)));
    }
}
