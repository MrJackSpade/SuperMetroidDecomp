using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Writes stock maps into fresh installation staging; never receives the user override directory.</summary>
public static class MapPresentationExtractor
{
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(directory);
        var hashes = new Dictionary<string, string>();
        var stationCells = new Dictionary<string, int[]>();
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var buffer = new MemoryStream();
            var map = AreaMapRomData.Load(bus, area);
            AreaMapPresentationAsset.Write(buffer, map);
            stationCells.Add(area.ToString(), Enumerable.Range(0, AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles)
                .Where(i => map.IsRevealedByMapStation(i % AreaMapLayout.WidthInTiles, i / AreaMapLayout.WidthInTiles)).ToArray());
            byte[] bytes = buffer.ToArray();
            string name = AreaMapCatalogFormat.FileName(area);
            // Exclusive creation prevents this stock importer from overwriting edited
            // files if a caller accidentally supplies an existing content directory.
            using (var file = new FileStream(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write))
                file.Write(bytes);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(bytes)));
        }
        byte[] revealBytes = JsonSerializer.SerializeToUtf8Bytes(stationCells, new JsonSerializerOptions { WriteIndented = true });
        using (var file = new FileStream(Path.Combine(directory, AreaMapCatalogFormat.StationRevealFile), FileMode.CreateNew, FileAccess.Write))
            file.Write(revealBytes);
        hashes.Add(AreaMapCatalogFormat.StationRevealFile, Convert.ToHexString(SHA256.HashData(revealBytes)));
        byte[] tilePixels = SnesGraphics.DecodePlanarTiles(
            RomDataReader.ReadFixedBank(bus, MapTileAtlasFormat.SourceAddress, MapTileAtlasFormat.ByteCount),
            4, MapTileAtlasFormat.TileColumns, out int atlasWidth, out int atlasHeight);
        using var atlas = new MemoryStream();
        // Palette indexes are tile content; runtime CGRAM supplies the selected map
        // palette. This neutral preview palette is not an imported gameplay palette.
        IndexedPng.Write(atlas, atlasWidth, atlasHeight, tilePixels, SnesGraphics.DiagnosticPalette(MapTileAtlasFormat.ColorCount));
        byte[] atlasBytes = atlas.ToArray();
        using (var file = new FileStream(Path.Combine(directory, MapTileAtlasFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(atlasBytes);
        hashes.Add(MapTileAtlasFormat.FileName, Convert.ToHexString(SHA256.HashData(atlasBytes)));
        byte[] pausePixels = SnesGraphics.DecodePlanarTiles(
            RomDataReader.ReadFixedBank(bus, PauseTileAtlasFormat.SourceAddress, PauseTileAtlasFormat.ByteCount),
            4, MapTileAtlasFormat.TileColumns, out int pauseWidth, out int pauseHeight);
        using var pauseAtlas = new MemoryStream();
        IndexedPng.Write(pauseAtlas, pauseWidth, pauseHeight, pausePixels, SnesGraphics.DiagnosticPalette(MapTileAtlasFormat.ColorCount));
        byte[] pauseBytes = pauseAtlas.ToArray();
        using (var file = new FileStream(Path.Combine(directory, PauseTileAtlasFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(pauseBytes);
        hashes.Add(PauseTileAtlasFormat.FileName, Convert.ToHexString(SHA256.HashData(pauseBytes)));
        byte[] hudPixels = SnesGraphics.DecodePlanarTiles(
            RomDataReader.ReadFixedBank(bus, HudTileAtlasFormat.SourceAddress, HudTileAtlasFormat.CharacterByteCount),
            2, MapTileAtlasFormat.TileColumns, out int hudWidth, out int hudHeight);
        using var hudAtlas = new MemoryStream();
        IndexedPng.Write(hudAtlas, hudWidth, hudHeight, hudPixels, SnesGraphics.DiagnosticPalette(4));
        byte[] hudBytes = hudAtlas.ToArray();
        using (var file = new FileStream(Path.Combine(directory, HudTileAtlasFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(hudBytes);
        hashes.Add(HudTileAtlasFormat.FileName, Convert.ToHexString(SHA256.HashData(hudBytes)));
        byte[] cycleBytes = MapPaletteCycleExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, MapPaletteCycleFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(cycleBytes);
        hashes.Add(MapPaletteCycleFormat.FileName, Convert.ToHexString(SHA256.HashData(cycleBytes)));
        byte[] paletteBytes = MapStaticPalettesExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, MapStaticPalettesFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(paletteBytes);
        hashes.Add(MapStaticPalettesFormat.FileName, Convert.ToHexString(SHA256.HashData(paletteBytes)));
        var labelPoints = new Dictionary<string, MapLabelPoint>();
        for (int area = 0; area < SuperMetroid.Core.Frontend.FileSelectMapRomData.AreaCount; area++)
        {
            int address = SuperMetroid.Core.Frontend.FileSelectMapRomData.LabelPositions + area * 4;
            labelPoints.Add(((AreaId)area).ToString(), new(RomDataReader.ReadWordFixedBank(bus, address),
                RomDataReader.ReadWordFixedBank(bus, address + 2)));
        }
        using var labels = new MemoryStream();
        WorldMapLabelLayout.Write(labels, new() { Version = WorldMapLabelFormat.Version, Areas = labelPoints });
        byte[] labelBytes = labels.ToArray();
        using (var file = new FileStream(Path.Combine(directory, WorldMapLabelFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(labelBytes);
        hashes.Add(WorldMapLabelFormat.FileName, Convert.ToHexString(SHA256.HashData(labelBytes)));
        byte[] stationLabelBytes = MapStationLayoutExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, MapStationLayoutFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(stationLabelBytes);
        hashes.Add(MapStationLayoutFormat.FileName, Convert.ToHexString(SHA256.HashData(stationLabelBytes)));
        byte[] landmarkBytes = MapLandmarkExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, MapLandmarkFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(landmarkBytes);
        hashes.Add(MapLandmarkFormat.FileName, Convert.ToHexString(SHA256.HashData(landmarkBytes)));
        byte[] saveMarkerBytes = MapSaveMarkerExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, MapSaveMarkerFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(saveMarkerBytes);
        hashes.Add(MapSaveMarkerFormat.FileName, Convert.ToHexString(SHA256.HashData(saveMarkerBytes)));
        byte[] arrowBytes = MapArrowExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, MapArrowFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(arrowBytes);
        hashes.Add(MapArrowFormat.FileName, Convert.ToHexString(SHA256.HashData(arrowBytes)));
        foreach (var resource in MapScreenExtractor.Extract(bus))
        {
            using var file = new FileStream(Path.Combine(directory, resource.Key), FileMode.CreateNew, FileAccess.Write);
            file.Write(resource.Value);
            hashes.Add(resource.Key, Convert.ToHexString(SHA256.HashData(resource.Value)));
        }
        foreach (var resource in MapSpriteExtractor.Extract(bus))
        {
            using var file = new FileStream(Path.Combine(directory, resource.Key), FileMode.CreateNew, FileAccess.Write);
            file.Write(resource.Value);
            hashes.Add(resource.Key, Convert.ToHexString(SHA256.HashData(resource.Value)));
        }
        byte[] backdropBytes = PauseBackdropExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, PauseBackdropDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(backdropBytes);
        hashes.Add(PauseBackdropDefinitions.FileName, Convert.ToHexString(SHA256.HashData(backdropBytes)));
        byte[] wireframeBytes = PauseWireframeExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, PauseWireframeDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(wireframeBytes);
        hashes.Add(PauseWireframeDefinitions.FileName, Convert.ToHexString(SHA256.HashData(wireframeBytes)));
        byte[] selectorBytes = PauseSelectorExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, PauseSelectorDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(selectorBytes);
        hashes.Add(PauseSelectorDefinitions.FileName, Convert.ToHexString(SHA256.HashData(selectorBytes)));
        byte[] reserveTankBytes = PauseReserveTankExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, PauseReserveTankDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(reserveTankBytes);
        hashes.Add(PauseReserveTankDefinitions.FileName, Convert.ToHexString(SHA256.HashData(reserveTankBytes)));
        byte[] reserveUiBytes = PauseReserveUiExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, PauseReserveUiDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(reserveUiBytes);
        hashes.Add(PauseReserveUiDefinitions.FileName, Convert.ToHexString(SHA256.HashData(reserveUiBytes)));
        byte[] equipmentBaseBytes = PauseEquipmentBaseExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, PauseEquipmentBaseDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(equipmentBaseBytes);
        hashes.Add(PauseEquipmentBaseDefinitions.FileName, Convert.ToHexString(SHA256.HashData(equipmentBaseBytes)));
        byte[] equipmentLabelBytes = PauseEquipmentLabelExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, PauseEquipmentLabelDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(equipmentLabelBytes);
        hashes.Add(PauseEquipmentLabelDefinitions.FileName, Convert.ToHexString(SHA256.HashData(equipmentLabelBytes)));
        byte[] escapeTimerBytes = EscapeTimerPresentationExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, EscapeTimerPresentationDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(escapeTimerBytes);
        hashes.Add(EscapeTimerPresentationDefinitions.FileName, Convert.ToHexString(SHA256.HashData(escapeTimerBytes)));
        byte[] escapeTimerTileBytes = EscapeTimerTileAtlasExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, EscapeTimerTileAtlasFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(escapeTimerTileBytes);
        hashes.Add(EscapeTimerTileAtlasFormat.FileName, Convert.ToHexString(SHA256.HashData(escapeTimerTileBytes)));
        byte[] gameplayHudBytes = GameplayHudPresentationExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, GameplayHudDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(gameplayHudBytes);
        hashes.Add(GameplayHudDefinitions.FileName, Convert.ToHexString(SHA256.HashData(gameplayHudBytes)));
        byte[] gameOverBytes = GameOverPresentationExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, GameOverPresentationDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(gameOverBytes);
        hashes.Add(GameOverPresentationDefinitions.FileName, Convert.ToHexString(SHA256.HashData(gameOverBytes)));
        byte[] gameOptionsBytes = GameOptionsPresentationExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, GameOptionsPresentationDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(gameOptionsBytes);
        hashes.Add(GameOptionsPresentationDefinitions.FileName, Convert.ToHexString(SHA256.HashData(gameOptionsBytes)));
        byte[] fileSelectBytes = FileSelectPresentationExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, FileSelectPresentationDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(fileSelectBytes);
        hashes.Add(FileSelectPresentationDefinitions.FileName, Convert.ToHexString(SHA256.HashData(fileSelectBytes)));
        byte[] gameplayMessageTitleBytes = GameplayMessageTitleExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, GameplayMessageTitleDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(gameplayMessageTitleBytes);
        hashes.Add(GameplayMessageTitleDefinitions.FileName,
            Convert.ToHexString(SHA256.HashData(gameplayMessageTitleBytes)));
        byte[] gameplayMessagePanelBytes = GameplayMessagePanelExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, GameplayMessagePanelDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(gameplayMessagePanelBytes);
        hashes.Add(GameplayMessagePanelDefinitions.FileName,
            Convert.ToHexString(SHA256.HashData(gameplayMessagePanelBytes)));
        byte[] gameplayMessageNoticeBytes = GameplayMessageNoticeExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, GameplayMessageNoticeDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(gameplayMessageNoticeBytes);
        hashes.Add(GameplayMessageNoticeDefinitions.FileName,
            Convert.ToHexString(SHA256.HashData(gameplayMessageNoticeBytes)));
        byte[] escapeTypewriterBytes = EscapeTypewriterExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, EscapeTypewriterDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(escapeTypewriterBytes);
        hashes.Add(EscapeTypewriterDefinitions.FileName,
            Convert.ToHexString(SHA256.HashData(escapeTypewriterBytes)));
        byte[] introNarrationBytes = IntroNarrationExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, IntroNarrationDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(introNarrationBytes);
        hashes.Add(IntroNarrationDefinitions.FileName,
            Convert.ToHexString(SHA256.HashData(introNarrationBytes)));
        byte[] introFontBytes = IntroFontAtlasExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, IntroFontAtlasFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(introFontBytes);
        hashes.Add(IntroFontAtlasFormat.FileName,
            Convert.ToHexString(SHA256.HashData(introFontBytes)));
        byte[] endingTextBytes = EndingTextExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, EndingTextDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(endingTextBytes);
        hashes.Add(EndingTextDefinitions.FileName,
            Convert.ToHexString(SHA256.HashData(endingTextBytes)));
        byte[] endingFontBytes = EndingFontAtlasExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, EndingFontAtlasFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(endingFontBytes);
        hashes.Add(EndingFontAtlasFormat.FileName,
            Convert.ToHexString(SHA256.HashData(endingFontBytes)));
        byte[] creditsBytes = CreditsPresentationExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, CreditsPresentationDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(creditsBytes);
        hashes.Add(CreditsPresentationDefinitions.FileName,
            Convert.ToHexString(SHA256.HashData(creditsBytes)));
        IReadOnlyDictionary<string, byte[]> titleGraphics = TitleGraphicsExtractor.Extract(bus);
        foreach (string name in new[]
                 {
                     TitleGraphicsFormat.Mode7TilesFile,
                     TitleGraphicsFormat.Mode7MapFile,
                     TitleGraphicsFormat.ObjectTilesFile,
                     TitleGraphicsFormat.BabyTilesFile,
                 })
        {
            byte[] bytes = titleGraphics[name];
            using (var file = new FileStream(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write))
                file.Write(bytes);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(bytes)));
        }
        byte[] titlePaletteBytes = TitlePaletteExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, TitlePaletteFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(titlePaletteBytes);
        hashes.Add(TitlePaletteFormat.FileName,
            Convert.ToHexString(SHA256.HashData(titlePaletteBytes)));
        byte[] titleGradientBytes = TitleGradientExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, TitleGradientFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(titleGradientBytes);
        hashes.Add(TitleGradientFormat.FileName,
            Convert.ToHexString(SHA256.HashData(titleGradientBytes)));
        byte[] roomPaletteFxBytes = RoomPaletteFxPresentationExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, RoomPaletteFxPresentationFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(roomPaletteFxBytes);
        hashes.Add(RoomPaletteFxPresentationFormat.FileName,
            Convert.ToHexString(SHA256.HashData(roomPaletteFxBytes)));
        byte[] motherBrainHealthBytes = MotherBrainHealthPaletteExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, MotherBrainHealthPaletteFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(motherBrainHealthBytes);
        hashes.Add(MotherBrainHealthPaletteFormat.FileName,
            Convert.ToHexString(SHA256.HashData(motherBrainHealthBytes)));
        byte[] motherBrainRainbowBytes = MotherBrainRainbowPaletteExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, MotherBrainRainbowPaletteFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(motherBrainRainbowBytes);
        hashes.Add(MotherBrainRainbowPaletteFormat.FileName,
            Convert.ToHexString(SHA256.HashData(motherBrainRainbowBytes)));
        byte[] roomFxAnimatedTileBytes = RoomFxAnimatedTileAtlasExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, RoomFxAnimatedTileAtlasFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(roomFxAnimatedTileBytes);
        hashes.Add(RoomFxAnimatedTileAtlasFormat.FileName,
            Convert.ToHexString(SHA256.HashData(roomFxAnimatedTileBytes)));
        byte[] roomFxLayer3TilemapBytes = RoomFxLayer3TilemapExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, RoomFxLayer3TilemapFormat.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(roomFxLayer3TilemapBytes);
        hashes.Add(RoomFxLayer3TilemapFormat.FileName,
            Convert.ToHexString(SHA256.HashData(roomFxLayer3TilemapBytes)));
        byte[] roomFxPaletteBlendBytes = RoomFxPaletteBlendExtractor.Extract(bus);
        using (var file = new FileStream(Path.Combine(directory, RoomFxPaletteBlendDefinitions.FileName), FileMode.CreateNew, FileAccess.Write))
            file.Write(roomFxPaletteBlendBytes);
        hashes.Add(RoomFxPaletteBlendDefinitions.FileName,
            Convert.ToHexString(SHA256.HashData(roomFxPaletteBlendBytes)));
        using var manifest = new FileStream(Path.Combine(directory, AreaMapCatalogFormat.ManifestFile), FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest, new AreaMapCatalogManifest
        {
            Version = AreaMapCatalogFormat.Version, SourceCartridgeSha256 = sourceCartridgeSha256, Sha256 = hashes
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });
    }
}
