using SuperMetroid.AssetExtraction.CartridgeTables;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Progression;

using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Hud;
namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Builds a constructed pause presentation from retail source data and isolated fixture visuals.</summary>
internal static class PauseMenuFixtureVerification
{
    /// <summary>Translates constructed cartridge visuals into the installed pause contract without mutating shared retail catalog data.</summary>
    internal static AreaMapPresentation CreateConstructedPausePresentation(
        CartridgeImportAddressSpace bus)
    {
        var stock = RetailPresentationFixture();
        var installation = RepositoryInstallation.Installation;
        byte[] Read(int address, int count) => RomDataReader.ReadFixedBank(
            CartridgeImportSource.Require(bus), address, count);
        T Document<T>(string name) => JsonSerializer.Deserialize<T>(
            File.ReadAllBytes(Path.Combine(installation.MapDirectory, name)),
            ArtworkJson.Options) ?? throw new InvalidDataException(name);
        MemoryStream Json<T>(T document) => new(JsonSerializer.SerializeToUtf8Bytes(
            document, ArtworkJson.Options));

        var pauseColors = new RgbColor[SnesCgram.ColorCount];
        byte[] paletteBytes = Read(0xb6f000, CgramPalettes.ByteCount);
        for (int i = 0; i < pauseColors.Length; i++)
            pauseColors[i] = RgbColor.FromWordIgnoringTopBit(LittleEndianBytes.Word(paletteBytes[i * 2], paletteBytes[i * 2 + 1]));
        var palettes = new MapStaticPalettes(ScreenColourImage.Decode(pauseColors), stock.Palettes.FileSelect,
            Enum.GetValues<AreaId>().Where(area => area != AreaId.Ceres)
                .ToDictionary(area => area, stock.Palettes.World));
        var areas = Enum.GetValues<AreaId>().Select(stock.Get).ToArray();
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
        using var spritePng = File.OpenRead(Path.Combine(installation.MapDirectory, MapSpriteFormat.PngFile));
        var selectors = Document<PauseSelectorDocument>(PauseSelectorDefinitions.FileName);
        selectors.Anchors[PauseSelectorAnchors.Anchor(PauseEquipmentCategory.Suits, 2)] = new(0x90, 0x70);
        selectors.Anchors[PauseSelectorAnchors.Anchor(PauseEquipmentCategory.Suits, 3)] = new(0xa0, 0x80);
        foreach (string name in selectors.Frames.Keys.ToArray()) selectors.Frames[name] = [singlePart];
        using var selectorJson = Json(selectors);
        // The original synthetic ROM supplied zero-part reserve-strip maps. Keep
        // that fixture isolation while reserve state and digits still execute.
        var reserveTanks = Document<PauseReserveTankDocument>(PauseReserveTankDefinitions.FileName);
        foreach (string name in reserveTanks.Frames.Keys.ToArray()) reserveTanks.Frames[name] = [];
        using var reserveJson = Json(reserveTanks);
        // Every part comes from the stock catalog's same-named property unless the fixture constructs it above.
        var parts = new AreaMapPresentationParts
        {
            Areas = areas,
            HiddenCells = stock.HiddenCells,
            ContentIdentity = "constructed-pause-interaction-fixture",
            HighlightCycle = stock.HighlightCycle,
            Palettes = palettes,
            Labels = stock.Labels,
            Stations = stock.Stations,
            Landmarks = stock.Landmarks,
            SaveMarkers = stock.SaveMarkers,
            Arrows = stock.Arrows,
            WorldArtwork = stock.WorldArtwork,
            Sprites = MapSpriteArtwork.Read(spriteJson, spritePng),
            PauseSelectors = PauseSelectorSourcePresentation.Load(selectorJson),
            PauseReserveTanks = PauseReserveTankSourcePresentation.Load(reserveJson),
            PauseReserveArrow = stock.PauseReserveArrow,
            EscapeTimer = stock.EscapeTimer,
            EscapeTimerTiles = stock.EscapeTimerTiles,
            GameplayHud = stock.GameplayHud,
            GameOver = stock.GameOver,
            GameOptions = stock.GameOptions,
            FileSelect = stock.FileSelect,
            EscapeTypewriter = stock.EscapeTypewriter,
            IntroNarration = stock.IntroNarration,
            IntroFont = stock.IntroFont,
            EndingText = stock.EndingText,
            EndingFont = stock.EndingFont,
            TitleGraphics = stock.TitleGraphics,
            TitlePalette = stock.TitlePalette,
            TitleGradient = stock.TitleGradient,
            RoomPaletteFx = stock.RoomPaletteFx,
            MotherBrainHealthPalette = stock.MotherBrainHealthPalette,
            MotherBrainRainbowPalette = stock.MotherBrainRainbowPalette,
            MotherBrainRoomColors = stock.MotherBrainRoomColors,
            RoomFxAnimatedTiles = stock.RoomFxAnimatedTiles,
            RoomFxPaletteBlends = stock.RoomFxPaletteBlends,
            PowerBombFixedColors = stock.PowerBombFixedColors,
            SamusVisorColors = stock.SamusVisorColors,
            SamusHurtColors = stock.SamusHurtColors,
            SamusSuitColors = stock.SamusSuitColors,
            SamusFullBodyCycleColors = stock.SamusFullBodyCycleColors,
            CrystalFlashColors = stock.CrystalFlashColors,
            SamusChargeColors = stock.SamusChargeColors,
            CeresRidleyColors = stock.CeresRidleyColors,
            CeresRidleyFloorColors = stock.CeresRidleyFloorColors,
            CeresRidleyGetawayBoostDelays = stock.CeresRidleyGetawayBoostDelays,
            SamusHyperBeamColors = stock.SamusHyperBeamColors,
        };
        // The screens and the HUD are pictured from the constructed ROM's characters (#1466).
        return new InstalledAreaMapPresentation(parts, new InstalledPresentationSources(new MapCharacterArtwork(
            new MapTileCharacters(Read(MapSourceDefinitions.Artwork.MapCharacters, MapTileAtlasFormat.ByteCount)), new HudTileAtlas(hud),
            new MapTileCharacters(Read(MapSourceDefinitions.Artwork.PauseInterfaceCharacters, PauseTileAtlasFormat.ByteCount))),
            GameplayMessageArtwork.Of(stock), InstalledAreaMapPresentation.RoomFxPagesOf(stock), InstalledAreaMapPresentation.StaffCreditsOf(stock),
            InstalledAreaMapPresentation.EndingTextOf(stock), InstalledAreaMapPresentation.ScreensOf(stock),
            new PauseScreenArtwork(backdrops, InstalledAreaMapPresentation.PauseOf(stock).Wireframes,
                InstalledAreaMapPresentation.PauseOf(stock).EquipmentBase, InstalledAreaMapPresentation.PauseOf(stock).EquipmentLabels,
                InstalledAreaMapPresentation.PauseOf(stock).ReserveUi)));
    }
}
