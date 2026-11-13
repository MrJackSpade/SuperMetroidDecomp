using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Progression;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;
using SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Options.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Bosses.Ridley;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Pause;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Writes stock maps into fresh installation staging.</summary>
public static class MapPresentationExtractor
{
    /// <summary>JSON options: Camel-case property names, indented output.</summary>
    private static readonly JsonSerializerOptions CamelCaseIndentedJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
    /// <summary>JSON options: Indented output.</summary>
    private static readonly JsonSerializerOptions IndentedJson = new() { WriteIndented = true };

    /// <summary>Creates the complete stock map family in fresh staging, including area maps, reveal masks, character atlases, palettes, labels, markers, sprites, screens, and a provenance/hash manifest.</summary>
    /// <param name="bus">Supported-cartridge address space supplying every native map presentation source.</param>
    /// <param name="directory">Destination stock directory; generated files use create-new semantics. The character and tile pages (#1636) and the colour documents only the import reads (#1637) go to its import-data directory (<see cref="MapPresentationFiles.ImportDataFiles"/>).</param>
    /// <param name="sourceCartridgeSha256">Source identity recorded in the completed map manifest.</param>
    /// <param name="cancellationToken">Cancellation checked before importing each area; later presentation groups are not interrupted by this token.</param>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(directory);
        string importData = SuperMetroid.AssetExtraction.Installation.ImportDataDirectory.Of(directory);
        Directory.CreateDirectory(importData);
        // A page or a colour document only the import reads goes to the import data, which the installer deletes before it
        // publishes; every other document is installed.
        string PathOf(string name) => Path.Combine(MapPresentationFiles.ImportDataFiles.Contains(name) ? importData : directory, name);
        var hashes = new Dictionary<string, string>();
        var stationCells = new Dictionary<string, int[]>();
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var buffer = new MemoryStream();
            var map = AreaMapImporter.Load(bus, area);
            AreaMapPresentationFile.Write(buffer, map);
            stationCells.Add(area.ToString(), Enumerable.Range(0, AreaMapSize.WidthInTiles * AreaMapSize.HeightInTiles)
                .Where(i => map.IsRevealedByMapStation(new(checked((ushort)(i % AreaMapSize.WidthInTiles)), checked((ushort)(i / AreaMapSize.WidthInTiles))))).ToArray());
            byte[] bytes = buffer.ToArray();
            string name = AreaMapCatalogFormat.FileName(area);
            // Exclusive creation prevents this stock importer from overwriting edited
            // files if a caller accidentally supplies an existing content directory.
            using (var file = new FileStream(PathOf(name), FileMode.CreateNew, FileAccess.Write))
                file.Write(bytes);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(bytes)));
        }
        byte[] revealBytes = JsonSerializer.SerializeToUtf8Bytes(stationCells, IndentedJson);
        using (var file = new FileStream(PathOf(AreaMapCatalogFormat.StationRevealFile), FileMode.CreateNew, FileAccess.Write))
            file.Write(revealBytes);
        hashes.Add(AreaMapCatalogFormat.StationRevealFile, Convert.ToHexString(SHA256.HashData(revealBytes)));
        byte[] tilePixels = SnesGraphics.DecodePlanarTiles(
            RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), MapSourceDefinitions.Artwork.MapCharacters, MapTileAtlasFormat.ByteCount),
            4, MapTileAtlasFormat.TileColumns, out Extent16<ImagePixels> atlasSize);
        using var atlas = new MemoryStream();
        // Palette indexes are tile content; runtime CGRAM supplies the selected map
        // palette. This neutral preview palette is not an imported gameplay palette.
        IndexedPng.Write(atlas, atlasSize, tilePixels, SnesGraphics.DiagnosticPalette(MapTileAtlasFormat.ColorCount));
        byte[] atlasBytes = atlas.ToArray();
        using (var file = new FileStream(PathOf(MapTileAtlasFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(atlasBytes);
        hashes.Add(MapTileAtlasFormat.FileName, Convert.ToHexString(SHA256.HashData(atlasBytes)));
        byte[] pausePixels = SnesGraphics.DecodePlanarTiles(
            RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), MapSourceDefinitions.Artwork.PauseInterfaceCharacters, PauseTileAtlasFormat.ByteCount),
            4, MapTileAtlasFormat.TileColumns, out Extent16<ImagePixels> pauseSize);
        using var pauseAtlas = new MemoryStream();
        IndexedPng.Write(pauseAtlas, pauseSize, pausePixels, SnesGraphics.DiagnosticPalette(MapTileAtlasFormat.ColorCount));
        byte[] pauseBytes = pauseAtlas.ToArray();
        using (var file = new FileStream(PathOf(PauseTileAtlasFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(pauseBytes);
        hashes.Add(PauseTileAtlasFormat.FileName, Convert.ToHexString(SHA256.HashData(pauseBytes)));
        byte[] hudPixels = SnesGraphics.DecodePlanarTiles(
            RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), MapSourceDefinitions.Artwork.HudCharacters, HudTileAtlasFormat.CharacterByteCount),
            2, MapTileAtlasFormat.TileColumns, out Extent16<ImagePixels> hudSize);
        using var hudAtlas = new MemoryStream();
        IndexedPng.Write(hudAtlas, hudSize, hudPixels, SnesGraphics.DiagnosticPalette(4));
        byte[] hudBytes = hudAtlas.ToArray();
        using (var file = new FileStream(PathOf(HudTileAtlasFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(hudBytes);
        hashes.Add(HudTileAtlasFormat.FileName, Convert.ToHexString(SHA256.HashData(hudBytes)));
        byte[] cycleBytes = MapPaletteCycleExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(MapPaletteCycleFile.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(cycleBytes);
        hashes.Add(MapPaletteCycleFile.FileName, Convert.ToHexString(SHA256.HashData(cycleBytes)));
        byte[] paletteBytes = MapStaticPalettesExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(MapStaticPalettesFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(paletteBytes);
        hashes.Add(MapStaticPalettesFormat.FileName, Convert.ToHexString(SHA256.HashData(paletteBytes)));
        var labelPoints = new Dictionary<string, Position16<SpritePixels>>();
        foreach (AreaId area in AreaIds.Zebes)
        {
            int address = MapSourceDefinitions.FileSelectMap.LabelPositions + AreaNumberCodec.Encode(area) * 4;
            labelPoints.Add(area.ToString(), new(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address),
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address + 2)));
        }
        using var labels = new MemoryStream();
        WorldMapLabelFile.Write(labels, new() { Version = WorldMapLabelFormat.Version, Areas = labelPoints });
        byte[] labelBytes = labels.ToArray();
        using (var file = new FileStream(PathOf(WorldMapLabelFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(labelBytes);
        hashes.Add(WorldMapLabelFormat.FileName, Convert.ToHexString(SHA256.HashData(labelBytes)));
        byte[] stationLabelBytes = MapStationLayoutExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(MapStationLayoutFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(stationLabelBytes);
        hashes.Add(MapStationLayoutFormat.FileName, Convert.ToHexString(SHA256.HashData(stationLabelBytes)));
        byte[] landmarkBytes = MapLandmarkExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(MapLandmarkFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(landmarkBytes);
        hashes.Add(MapLandmarkFormat.FileName, Convert.ToHexString(SHA256.HashData(landmarkBytes)));
        byte[] saveMarkerBytes = MapSaveMarkerExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(MapSaveMarkerFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(saveMarkerBytes);
        hashes.Add(MapSaveMarkerFormat.FileName, Convert.ToHexString(SHA256.HashData(saveMarkerBytes)));
        byte[] arrowBytes = MapArrowExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(MapArrowFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(arrowBytes);
        hashes.Add(MapArrowFormat.FileName, Convert.ToHexString(SHA256.HashData(arrowBytes)));
        foreach (var resource in MapScreenExtractor.Extract(bus))
        {
            using var file = new FileStream(PathOf(resource.Key), FileMode.CreateNew, FileAccess.Write);
            file.Write(resource.Value);
            hashes.Add(resource.Key, Convert.ToHexString(SHA256.HashData(resource.Value)));
        }
        foreach (var resource in MapSpriteExtractor.Extract(bus))
        {
            using var file = new FileStream(PathOf(resource.Key), FileMode.CreateNew, FileAccess.Write);
            file.Write(resource.Value);
            hashes.Add(resource.Key, Convert.ToHexString(SHA256.HashData(resource.Value)));
        }
        byte[] backdropBytes = PauseBackdropExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(PauseBackdropDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(backdropBytes);
        hashes.Add(PauseBackdropDefinitions.FileName, Convert.ToHexString(SHA256.HashData(backdropBytes)));
        byte[] wireframeBytes = PauseWireframeExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(PauseWireframeDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(wireframeBytes);
        hashes.Add(PauseWireframeDefinitions.FileName, Convert.ToHexString(SHA256.HashData(wireframeBytes)));
        byte[] selectorBytes = PauseSelectorExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(PauseSelectorFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(selectorBytes);
        hashes.Add(PauseSelectorFormat.FileName, Convert.ToHexString(SHA256.HashData(selectorBytes)));
        byte[] reserveTankBytes = PauseReserveTankExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(PauseReserveTankFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(reserveTankBytes);
        hashes.Add(PauseReserveTankFormat.FileName, Convert.ToHexString(SHA256.HashData(reserveTankBytes)));
        byte[] reserveUiBytes = PauseReserveUiExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(PauseReserveUiDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(reserveUiBytes);
        hashes.Add(PauseReserveUiDefinitions.FileName, Convert.ToHexString(SHA256.HashData(reserveUiBytes)));
        byte[] equipmentBaseBytes = PauseEquipmentBaseExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(PauseEquipmentBaseDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(equipmentBaseBytes);
        hashes.Add(PauseEquipmentBaseDefinitions.FileName, Convert.ToHexString(SHA256.HashData(equipmentBaseBytes)));
        byte[] equipmentLabelBytes = PauseEquipmentLabelExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(PauseEquipmentLabelArtworkDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(equipmentLabelBytes);
        hashes.Add(PauseEquipmentLabelArtworkDefinitions.FileName, Convert.ToHexString(SHA256.HashData(equipmentLabelBytes)));
        byte[] escapeTimerBytes = EscapeTimerPresentationExtractor.Extract();
        using (var file = new FileStream(PathOf(EscapeTimerLayoutDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(escapeTimerBytes);
        hashes.Add(EscapeTimerLayoutDefinitions.FileName, Convert.ToHexString(SHA256.HashData(escapeTimerBytes)));
        byte[] escapeTimerTileBytes = EscapeTimerTileAtlasExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(EscapeTimerTileAtlasFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(escapeTimerTileBytes);
        hashes.Add(EscapeTimerTileAtlasFormat.FileName, Convert.ToHexString(SHA256.HashData(escapeTimerTileBytes)));
        byte[] gameplayHudBytes = GameplayHudPresentationExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(GameplayHudDocumentDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(gameplayHudBytes);
        hashes.Add(GameplayHudDocumentDefinitions.FileName, Convert.ToHexString(SHA256.HashData(gameplayHudBytes)));
        byte[] gameOverBytes = GameOverPresentationExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(GameOverPresentationFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(gameOverBytes);
        hashes.Add(GameOverPresentationFormat.FileName, Convert.ToHexString(SHA256.HashData(gameOverBytes)));
        byte[] gameOptionsBytes = GameOptionsPresentationExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(GameOptionsPresentationFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(gameOptionsBytes);
        hashes.Add(GameOptionsPresentationFormat.FileName, Convert.ToHexString(SHA256.HashData(gameOptionsBytes)));
        byte[] fileSelectBytes = FileSelectPresentationExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(FileSelectPresentationFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(fileSelectBytes);
        hashes.Add(FileSelectPresentationFormat.FileName, Convert.ToHexString(SHA256.HashData(fileSelectBytes)));
        byte[] gameplayMessageTitleBytes = GameplayMessageTitleExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(GameplayMessageTitleDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(gameplayMessageTitleBytes);
        hashes.Add(GameplayMessageTitleDefinitions.FileName,
            Convert.ToHexString(SHA256.HashData(gameplayMessageTitleBytes)));
        byte[] gameplayMessagePanelBytes = GameplayMessagePanelExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(GameplayMessagePanelDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(gameplayMessagePanelBytes);
        hashes.Add(GameplayMessagePanelDefinitions.FileName,
            Convert.ToHexString(SHA256.HashData(gameplayMessagePanelBytes)));
        byte[] gameplayMessageNoticeBytes = GameplayMessageNoticeExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(GameplayMessageNoticeDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(gameplayMessageNoticeBytes);
        hashes.Add(GameplayMessageNoticeDefinitions.FileName,
            Convert.ToHexString(SHA256.HashData(gameplayMessageNoticeBytes)));
        byte[] escapeTypewriterBytes = EscapeTypewriterExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(EscapeTypewriterDocumentDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(escapeTypewriterBytes);
        hashes.Add(EscapeTypewriterDocumentDefinitions.FileName,
            Convert.ToHexString(SHA256.HashData(escapeTypewriterBytes)));
        byte[] introNarrationBytes = IntroNarrationExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(IntroNarrationFile.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(introNarrationBytes);
        hashes.Add(IntroNarrationFile.FileName,
            Convert.ToHexString(SHA256.HashData(introNarrationBytes)));
        byte[] introFontBytes = IntroFontAtlasExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(IntroFontCharacters.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(introFontBytes);
        hashes.Add(IntroFontCharacters.FileName,
            Convert.ToHexString(SHA256.HashData(introFontBytes)));
        byte[] endingTextBytes = EndingTextExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(EndingTextDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(endingTextBytes);
        hashes.Add(EndingTextDefinitions.FileName,
            Convert.ToHexString(SHA256.HashData(endingTextBytes)));
        byte[] endingFontBytes = EndingFontAtlasExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(EndingFontAtlasFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(endingFontBytes);
        hashes.Add(EndingFontAtlasFormat.FileName,
            Convert.ToHexString(SHA256.HashData(endingFontBytes)));
        byte[] creditsBytes = CreditsPresentationExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(CreditsPresentationDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(creditsBytes);
        hashes.Add(CreditsPresentationDefinitions.FileName,
            Convert.ToHexString(SHA256.HashData(creditsBytes)));
        IReadOnlyDictionary<string, byte[]> titleGraphics = TitleGraphicsExtractor.Extract(bus);
        foreach (string name in new[]
                 {
                     TitleGraphicsFormat.FloorTilesFile,
                     TitleGraphicsFormat.FloorMapFile,
                     TitleGraphicsFormat.ObjectTilesFile,
                     TitleGraphicsFormat.BabyTilesFile,
                 })
        {
            byte[] bytes = titleGraphics[name];
            using (var file = new FileStream(PathOf(name), FileMode.CreateNew, FileAccess.Write))
                file.Write(bytes);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(bytes)));
        }
        byte[] titlePaletteBytes = TitlePaletteExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(TitlePaletteFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(titlePaletteBytes);
        hashes.Add(TitlePaletteFormat.FileName,
            Convert.ToHexString(SHA256.HashData(titlePaletteBytes)));
        // The scene colours the game shows are taken once from the screens' own colours (#1544): the game reads them and no
        // palette document.
        byte[] sceneColourBytes = SceneColourFiles.Extract(
            TitlePaletteFile.Read(new MemoryStream(titlePaletteBytes, writable: false)),
            MapStaticPalettesFile.Read(new MemoryStream(paletteBytes, writable: false)));
        using (var file = new FileStream(PathOf(SceneColourFiles.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(sceneColourBytes);
        hashes.Add(SceneColourFiles.FileName,
            Convert.ToHexString(SHA256.HashData(sceneColourBytes)));
        byte[] titleGradientBytes = TitleGradientExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(TitleGradientFile.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(titleGradientBytes);
        hashes.Add(TitleGradientFile.FileName,
            Convert.ToHexString(SHA256.HashData(titleGradientBytes)));
        byte[] roomPaletteFxBytes = RoomPaletteFxPresentationExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(RoomPaletteFxPresentationFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(roomPaletteFxBytes);
        hashes.Add(RoomPaletteFxPresentationFormat.FileName,
            Convert.ToHexString(SHA256.HashData(roomPaletteFxBytes)));
        byte[] motherBrainHealthBytes = MotherBrainHealthPaletteExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(MotherBrainHealthPaletteFile.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(motherBrainHealthBytes);
        hashes.Add(MotherBrainHealthPaletteFile.FileName,
            Convert.ToHexString(SHA256.HashData(motherBrainHealthBytes)));
        byte[] motherBrainRainbowBytes = MotherBrainRainbowPaletteExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(MotherBrainRainbowPaletteFile.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(motherBrainRainbowBytes);
        hashes.Add(MotherBrainRainbowPaletteFile.FileName,
            Convert.ToHexString(SHA256.HashData(motherBrainRainbowBytes)));
        byte[] motherBrainBeamBytes = MotherBrainBeamColourExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(MotherBrainBeamColourFile.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(motherBrainBeamBytes);
        hashes.Add(MotherBrainBeamColourFile.FileName,
            Convert.ToHexString(SHA256.HashData(motherBrainBeamBytes)));
        byte[] motherBrainRoomColorBytes = MotherBrainRoomColorExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(MotherBrainRoomColorFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(motherBrainRoomColorBytes);
        hashes.Add(MotherBrainRoomColorFormat.FileName,
            Convert.ToHexString(SHA256.HashData(motherBrainRoomColorBytes)));
        byte[] roomFxAnimatedTileBytes = RoomFxAnimatedTileAtlasExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(RoomFxAnimatedTileAtlasFile.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(roomFxAnimatedTileBytes);
        hashes.Add(RoomFxAnimatedTileAtlasFile.FileName,
            Convert.ToHexString(SHA256.HashData(roomFxAnimatedTileBytes)));
        byte[] roomFxLayer3TilemapBytes = RoomFxLayer3TilemapExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(RoomFxLayer3TilemapFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(roomFxLayer3TilemapBytes);
        hashes.Add(RoomFxLayer3TilemapFormat.FileName,
            Convert.ToHexString(SHA256.HashData(roomFxLayer3TilemapBytes)));
        byte[] roomFxPaletteBlendBytes = RoomFxPaletteBlendExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(RoomFxPaletteBlendDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(roomFxPaletteBlendBytes);
        hashes.Add(RoomFxPaletteBlendDefinitions.FileName,
            Convert.ToHexString(SHA256.HashData(roomFxPaletteBlendBytes)));
        byte[] powerBombGlowColorBytes = PowerBombGlowColorExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(PowerBombGlowColorFile.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(powerBombGlowColorBytes);
        hashes.Add(PowerBombGlowColorFile.FileName,
            Convert.ToHexString(SHA256.HashData(powerBombGlowColorBytes)));
        byte[] samusVisorColorBytes = SamusVisorColorExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(SamusVisorColorFile.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(samusVisorColorBytes);
        hashes.Add(SamusVisorColorFile.FileName,
            Convert.ToHexString(SHA256.HashData(samusVisorColorBytes)));
        byte[] samusHurtColorBytes = SamusHurtColorExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(SamusHurtColorFile.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(samusHurtColorBytes);
        hashes.Add(SamusHurtColorFile.FileName,
            Convert.ToHexString(SHA256.HashData(samusHurtColorBytes)));
        byte[] samusSuitColorBytes = SamusSuitColorExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(SamusSuitColorFile.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(samusSuitColorBytes);
        hashes.Add(SamusSuitColorFile.FileName,
            Convert.ToHexString(SHA256.HashData(samusSuitColorBytes)));
        byte[] samusFullBodyCycleBytes = SamusFullBodyCycleColorExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(SamusFullBodyCycleColorFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(samusFullBodyCycleBytes);
        hashes.Add(SamusFullBodyCycleColorFormat.FileName,
            Convert.ToHexString(SHA256.HashData(samusFullBodyCycleBytes)));
        byte[] crystalFlashColorBytes = CrystalFlashColorExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(CrystalFlashColorFile.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(crystalFlashColorBytes);
        hashes.Add(CrystalFlashColorFile.FileName,
            Convert.ToHexString(SHA256.HashData(crystalFlashColorBytes)));
        byte[] samusChargeColorBytes = SamusChargeColorExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(SamusChargeColorFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(samusChargeColorBytes);
        hashes.Add(SamusChargeColorFormat.FileName,
            Convert.ToHexString(SHA256.HashData(samusChargeColorBytes)));
        byte[] ceresRidleyColorBytes = CeresRidleyColorExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(CeresRidleyColorFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(ceresRidleyColorBytes);
        hashes.Add(CeresRidleyColorFormat.FileName,
            Convert.ToHexString(SHA256.HashData(ceresRidleyColorBytes)));
        byte[] ceresRidleyMode7ColorBytes = CeresRidleyMode7ColorExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(CeresRidleyFloorColorFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(ceresRidleyMode7ColorBytes);
        hashes.Add(CeresRidleyFloorColorFormat.FileName,
            Convert.ToHexString(SHA256.HashData(ceresRidleyMode7ColorBytes)));
        byte[] ceresRidleyGetawayBoostDelayBytes = CeresRidleyGetawayBoostDelayFiles.Extract(bus);
        using (var file = new FileStream(PathOf(CeresRidleyGetawayBoostDelayFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(ceresRidleyGetawayBoostDelayBytes);
        hashes.Add(CeresRidleyGetawayBoostDelayFormat.FileName,
            Convert.ToHexString(SHA256.HashData(ceresRidleyGetawayBoostDelayBytes)));
        byte[] samusHyperBeamColorBytes = SamusHyperBeamColorExtractor.Extract(bus);
        using (var file = new FileStream(PathOf(SamusHyperBeamColorFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(samusHyperBeamColorBytes);
        hashes.Add(SamusHyperBeamColorFormat.FileName,
            Convert.ToHexString(SHA256.HashData(samusHyperBeamColorBytes)));
        using var manifest = new FileStream(PathOf(AreaMapCatalogFormat.ManifestFile), FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest, new AreaMapCatalogManifest
        {
            Version = AreaMapCatalogFormat.Version, SourceCartridgeSha256 = sourceCartridgeSha256, Sha256 = hashes
        }, CamelCaseIndentedJson);
    }
}
