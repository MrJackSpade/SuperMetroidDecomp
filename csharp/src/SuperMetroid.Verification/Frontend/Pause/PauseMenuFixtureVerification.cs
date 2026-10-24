using SuperMetroid.AssetExtraction.CartridgeTables;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Builds a constructed pause presentation from retail source data and isolated fixture visuals.</summary>
internal static class PauseMenuFixtureVerification
{
    /// <summary>Translates constructed cartridge visuals into the installed pause contract without mutating shared retail catalog data.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the constructed pause presentation source bytes")]
    internal static AreaMapPresentationCatalog CreateConstructedPausePresentation(
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

        var pauseColors = new Bgr555[SnesCgram.ColorCount];
        byte[] paletteBytes = Read(0xb6f000, CgramPalettes.ByteCount);
        for (int i = 0; i < pauseColors.Length; i++)
            pauseColors[i] = Bgr555.FromCgramPortWord(SnesLittleEndian.Word(paletteBytes[i * 2], paletteBytes[i * 2 + 1]));
        var palettes = new MapStaticPalettes(pauseColors, stock.Palettes.FileSelect.ToArray(),
            Enum.GetValues<AreaId>().Where(area => area != AreaId.Ceres)
                .ToDictionary(area => area, area => stock.Palettes.World(area).ToArray()));
        var areas = Enum.GetValues<AreaId>().Select(stock.Get).ToArray();
        areas[AreaIds.ToIndex(AreaId.Crateria)] = AreaMapImporter.Load(bus, AreaId.Crateria);
        var backdrops = PauseBackdropPresentation.FromTilemaps(
            Enum.GetValues<AreaId>().Select(_ => Read(0xb6e000, 0x0800)).ToArray(),
            Read(PauseMenuCartridgeTables.PauseButtonTilemap, PauseBackdropDefinitions.ButtonCells * 2));
        var hud = new byte[HudTileAtlasFormat.ByteCount];
        Read(HudTileAtlasFormat.SourceAddress, HudTileAtlasFormat.CharacterByteCount).CopyTo(hud, 0);

        var singlePart = new SpriteVisualPart { OffsetX = 0, OffsetY = 0,
            TileColumn = 0, TileRow = 0, Size = 8, Priority = 0, Palette = null,
            FlipX = false, FlipY = false };
        var sprites = Document<MapSpriteDocument>(MapSpriteFormat.JsonFile);
        foreach (string name in new[] { "Indicator.Frame0", "Indicator.Frame1", "Indicator.Frame2" })
            sprites.Frames[name] = [singlePart];
        using var spriteJson = Json(sprites);
        using var spritePng = File.OpenRead(Path.Combine(installation.MapDirectory, MapSpriteFormat.PngFile));
        var selectors = Document<PauseSelectorDocument>(PauseSelectorDefinitions.FileName);
        selectors.Anchors[PauseSelectorDefinitions.Anchor(PauseEquipmentCategory.Suits, 2)] = new(0x90, 0x70);
        selectors.Anchors[PauseSelectorDefinitions.Anchor(PauseEquipmentCategory.Suits, 3)] = new(0xa0, 0x80);
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
            ContentIdentity = "constructed-pause-interaction-fixture",
            Tiles = new MapTileAtlas(Read(MapTileAtlasFormat.SourceAddress, MapTileAtlasFormat.ByteCount)),
            HudTiles = new HudTileAtlas(hud),
            HighlightCycle = stock.HighlightCycle,
            Palettes = palettes,
            Labels = stock.Labels,
            Stations = stock.Stations,
            Landmarks = stock.Landmarks,
            SaveMarkers = stock.SaveMarkers,
            Arrows = stock.Arrows,
            Screens = stock.Screens,
            WorldArtwork = stock.WorldArtwork,
            Sprites = MapSpriteCatalog.Load(spriteJson, spritePng),
            PauseTiles = new MapTileAtlas(Read(PauseTileAtlasFormat.SourceAddress, PauseTileAtlasFormat.ByteCount)),
            PauseBackdrops = backdrops,
            PauseWireframes = stock.PauseWireframes,
            PauseSelectors = PauseSelectorPresentation.Load(selectorJson),
            PauseReserveTanks = PauseReserveTankPresentation.Load(reserveJson),
            PauseReserveUi = stock.PauseReserveUi,
            PauseEquipmentBase = stock.PauseEquipmentBase,
            PauseEquipmentLabels = stock.PauseEquipmentLabels,
            EscapeTimer = stock.EscapeTimer,
            EscapeTimerTiles = stock.EscapeTimerTiles,
            GameplayHud = stock.GameplayHud,
            GameOver = stock.GameOver,
            GameOptions = stock.GameOptions,
            FileSelect = stock.FileSelect,
            GameplayMessageTitles = stock.GameplayMessageTitles,
            GameplayMessagePanels = stock.GameplayMessagePanels,
            GameplayMessageNotices = stock.GameplayMessageNotices,
            EscapeTypewriter = stock.EscapeTypewriter,
            IntroNarration = stock.IntroNarration,
            IntroFont = stock.IntroFont,
            EndingText = stock.EndingText,
            EndingFont = stock.EndingFont,
            StaffCredits = stock.StaffCredits,
            TitleGraphics = stock.TitleGraphics,
            TitlePalette = stock.TitlePalette,
            TitleGradient = stock.TitleGradient,
            RoomPaletteFx = stock.RoomPaletteFx,
            MotherBrainHealthPalette = stock.MotherBrainHealthPalette,
            MotherBrainRainbowPalette = stock.MotherBrainRainbowPalette,
            MotherBrainRoomColors = stock.MotherBrainRoomColors,
            RoomFxAnimatedTiles = stock.RoomFxAnimatedTiles,
            RoomFxLayer3Tilemaps = stock.RoomFxLayer3Tilemaps,
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
            SamusHyperBeamColors = stock.SamusHyperBeamColors,
        };
        return new AreaMapPresentationCatalog(parts);
    }
}
