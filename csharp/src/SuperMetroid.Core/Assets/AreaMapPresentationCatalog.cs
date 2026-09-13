using System.Security.Cryptography;
using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable map content snapshot. Overrides never alter stock provenance or exploration rules.</summary>
public sealed class AreaMapPresentationCatalog : IVramAssetProvider
{
    private readonly IAreaMapView[] areas;
    private AreaMapPresentationCatalog(IAreaMapView[] areas, string contentIdentity, MapTileAtlas tiles, HudTileAtlas hudTiles, MapPaletteCycle highlightCycle, MapStaticPalettes palettes, WorldMapLabelLayout labels, MapStationLayout stations, MapLandmarkLayout landmarks, MapSaveMarkerLayout saveMarkers, MapArrowPresentation arrows, MapScreenPresentation screens, WorldMapArtwork worldArtwork, MapSpriteCatalog sprites, MapTileAtlas pauseTiles)
    {
        this.areas = areas;
        ContentIdentity = contentIdentity;
        Tiles = tiles;
        HudTiles = hudTiles;
        HighlightCycle = highlightCycle;
        Palettes = palettes;
        Labels = labels;
        Stations = stations;
        Landmarks = landmarks;
        SaveMarkers = saveMarkers;
        Arrows = arrows;
        Screens = screens;
        WorldArtwork = worldArtwork;
        Sprites = sprites;
        PauseTiles = pauseTiles;
    }

    public string ContentIdentity { get; }
    public MapTileAtlas Tiles { get; }
    public HudTileAtlas HudTiles { get; }
    public MapPaletteCycle HighlightCycle { get; }
    public MapStaticPalettes Palettes { get; }
    public WorldMapLabelLayout Labels { get; }
    public MapStationLayout Stations { get; }
    public MapLandmarkLayout Landmarks { get; }
    public MapSaveMarkerLayout SaveMarkers { get; }
    public MapArrowPresentation Arrows { get; }
    public MapScreenPresentation Screens { get; }
    public WorldMapArtwork WorldArtwork { get; }
    public MapSpriteCatalog Sprites { get; }
    public MapTileAtlas PauseTiles { get; }
    public ReadOnlyMemory<byte> Resolve(VramAssetId asset) => asset == VramAssetId.StandardHudTiles
        ? HudTiles.Transfer : throw new InvalidDataException($"Map catalog cannot resolve VRAM asset {asset}.");
    public IAreaMapView Get(AreaId area) => areas[AreaIds.ToIndex(area)];

    /// <summary>Reads all areas atomically into a new catalog; an invalid override is never replaced with stock.</summary>
    public static AreaMapPresentationCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        var stock = ReadVerifiedStock(stockDirectory);
        var areas = new IAreaMapView[AreaIds.RetailCount];
        using var identity = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            using var baselineStream = new MemoryStream(stock.Maps[area], writable: false);
            var baseline = AreaMapPresentationAsset.Load(baselineStream, new AreaMapDecodeRules(area));
            IAreaMapView definition = new AreaMapStockRules(baseline, stock.StationCells[area]);
            // The identity covers baseline rule content too: equal override bytes do
            // not imply equal exploration semantics across different stock installs.
            AppendFramed(stock.Maps[area]);
            byte[] stationPlane = new byte[AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles];
            foreach (int cell in stock.StationCells[area]) stationPlane[cell] = 1;
            AppendFramed(stationPlane);
            string? replacement = overrideDirectory is null ? null : Path.Combine(overrideDirectory, AreaMapCatalogFormat.FileName(area));
            bool hasReplacement = replacement is not null && File.Exists(replacement);
            byte[] selected = hasReplacement ? File.ReadAllBytes(replacement!) : stock.Maps[area];
            try
            {
                using var stream = new MemoryStream(selected, writable: false);
                areas[AreaIds.ToIndex(area)] = AreaMapPresentationAsset.Load(stream, definition);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid {area} map presentation ({(hasReplacement ? replacement : stockDirectory)}): {error.Message}", error);
            }
            // Hash framed contents in fixed area order: identity changes after a valid
            // replacement is reloaded, without rewriting original extraction provenance.
            AppendFramed(selected);
        }
        string? atlasOverride = overrideDirectory is null ? null : Path.Combine(overrideDirectory, MapTileAtlasFormat.FileName);
        byte[] atlasBytes = atlasOverride is not null && File.Exists(atlasOverride) ? File.ReadAllBytes(atlasOverride) : stock.Atlas;
        MapTileAtlas tiles;
        try { tiles = MapTileAtlas.Load(new MemoryStream(atlasBytes, writable: false)); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid map tile atlas ({atlasOverride ?? stockDirectory}): {error.Message}", error); }
        AppendFramed(atlasBytes);
        string? hudOverride = overrideDirectory is null ? null : Path.Combine(overrideDirectory, HudTileAtlasFormat.FileName);
        byte[] hudBytes = hudOverride is not null && File.Exists(hudOverride) ? File.ReadAllBytes(hudOverride) : stock.HudAtlas;
        HudTileAtlas hudTiles;
        try { hudTiles = HudTileAtlas.Load(new MemoryStream(hudBytes, writable: false)); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid HUD tile atlas ({hudOverride ?? stockDirectory}): {error.Message}", error); }
        AppendFramed(hudBytes);
        string? cycleOverride = overrideDirectory is null ? null : Path.Combine(overrideDirectory, MapPaletteCycleFormat.FileName);
        byte[] cycleBytes = cycleOverride is not null && File.Exists(cycleOverride) ? File.ReadAllBytes(cycleOverride) : stock.HighlightCycle;
        MapPaletteCycle cycle;
        try { cycle = MapPaletteCycle.Load(new MemoryStream(cycleBytes, writable: false)); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid map highlight cycle ({cycleOverride ?? stockDirectory}): {error.Message}", error); }
        AppendFramed(cycleBytes);
        string? paletteOverride = overrideDirectory is null ? null : Path.Combine(overrideDirectory, MapStaticPalettesFormat.FileName);
        byte[] paletteBytes = paletteOverride is not null && File.Exists(paletteOverride) ? File.ReadAllBytes(paletteOverride) : stock.Palettes;
        MapStaticPalettes palettes;
        try { palettes = MapStaticPalettes.Load(new MemoryStream(paletteBytes, writable: false)); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid map palettes ({paletteOverride ?? stockDirectory}): {error.Message}", error); }
        AppendFramed(paletteBytes);
        string? labelOverride = overrideDirectory is null ? null : Path.Combine(overrideDirectory, WorldMapLabelFormat.FileName);
        byte[] labelBytes = labelOverride is not null && File.Exists(labelOverride) ? File.ReadAllBytes(labelOverride) : stock.Labels;
        WorldMapLabelLayout labels;
        try { labels = WorldMapLabelLayout.Load(new MemoryStream(labelBytes, writable: false)); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid world-map labels ({labelOverride ?? stockDirectory}): {error.Message}", error); }
        AppendFramed(labelBytes);
        string? stationOverride = overrideDirectory is null ? null : Path.Combine(overrideDirectory, MapStationLayoutFormat.FileName);
        byte[] stationBytes = stationOverride is not null && File.Exists(stationOverride) ? File.ReadAllBytes(stationOverride) : stock.Stations;
        MapStationLayout stations;
        try { stations = MapStationLayout.Load(new MemoryStream(stationBytes, writable: false)); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid map station labels ({stationOverride ?? stockDirectory}): {error.Message}", error); }
        AppendFramed(stationBytes);
        string? landmarkOverride = overrideDirectory is null ? null : Path.Combine(overrideDirectory, MapLandmarkFormat.FileName);
        byte[] landmarkBytes = landmarkOverride is not null && File.Exists(landmarkOverride) ? File.ReadAllBytes(landmarkOverride) : stock.Landmarks;
        MapLandmarkLayout landmarks;
        try { landmarks = MapLandmarkLayout.Load(new MemoryStream(landmarkBytes, writable: false)); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid map landmarks ({landmarkOverride ?? stockDirectory}): {error.Message}", error); }
        AppendFramed(landmarkBytes);
        string? saveMarkerOverride = overrideDirectory is null ? null : Path.Combine(overrideDirectory, MapSaveMarkerFormat.FileName);
        byte[] saveMarkerBytes = saveMarkerOverride is not null && File.Exists(saveMarkerOverride) ? File.ReadAllBytes(saveMarkerOverride) : stock.SaveMarkers;
        MapSaveMarkerLayout saveMarkers;
        try { saveMarkers = MapSaveMarkerLayout.Load(new MemoryStream(saveMarkerBytes, writable: false)); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid save markers ({saveMarkerOverride ?? stockDirectory}): {error.Message}", error); }
        AppendFramed(saveMarkerBytes);
        string? arrowOverride = overrideDirectory is null ? null : Path.Combine(overrideDirectory, MapArrowFormat.FileName);
        byte[] arrowBytes = arrowOverride is not null && File.Exists(arrowOverride) ? File.ReadAllBytes(arrowOverride) : stock.Arrows;
        MapArrowPresentation arrows;
        try { arrows = MapArrowPresentation.Load(new MemoryStream(arrowBytes, writable: false)); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid map arrows ({arrowOverride ?? stockDirectory}): {error.Message}", error); }
        AppendFramed(arrowBytes);
        byte[] screenBytes = Select(MapScreenDefinitions.FileName, stock.Screens);
        byte[] worldFrontBytes = Select(WorldMapArtworkFormat.ForegroundFile, stock.WorldFront);
        byte[] worldBackBytes = Select(WorldMapArtworkFormat.BackgroundFile, stock.WorldBack);
        MapScreenPresentation screens;
        WorldMapArtwork artwork;
        try { screens = MapScreenPresentation.Load(new MemoryStream(screenBytes, writable: false)); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid map screens ({overrideDirectory ?? stockDirectory}/{MapScreenDefinitions.FileName}): {error.Message}", error); }
        try { artwork = WorldMapArtwork.Load(new MemoryStream(worldFrontBytes, writable: false), new MemoryStream(worldBackBytes, writable: false)); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid world-map PNG artwork in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        byte[] spriteJson = Select(MapSpriteFormat.JsonFile, stock.SpriteJson), spritePng = Select(MapSpriteFormat.PngFile, stock.SpritePng);
        MapSpriteCatalog sprites;
        try { sprites = MapSpriteCatalog.Load(new MemoryStream(spriteJson), new MemoryStream(spritePng)); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid map sprites in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        MapTileAtlas pauseTiles;
        try { pauseTiles = MapTileAtlas.Load(new MemoryStream(Select(PauseTileAtlasFormat.FileName, stock.PauseTiles))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid pause artwork in {overrideDirectory ?? stockDirectory}/{PauseTileAtlasFormat.FileName}: {error.Message}", error); }
        return new(areas, Convert.ToHexString(identity.GetHashAndReset()), tiles, hudTiles, cycle, palettes, labels, stations, landmarks, saveMarkers, arrows, screens, artwork, sprites, pauseTiles);

        byte[] Select(string name, byte[] baseline)
        {
            string? path = overrideDirectory is null ? null : Path.Combine(overrideDirectory, name);
            byte[] bytes = path is not null && File.Exists(path) ? File.ReadAllBytes(path) : baseline;
            AppendFramed(bytes);
            return bytes;
        }

        void AppendFramed(byte[] bytes)
        {
            Span<byte> length = stackalloc byte[sizeof(int)];
            BinaryPrimitives.WriteInt32LittleEndian(length, bytes.Length);
            identity.AppendData(length);
            identity.AppendData(bytes);
        }
    }

    /// <summary>Installer integrity check; never repairs files or touches the override directory.</summary>
    public static void ValidateStock(string directory) => _ = ReadVerifiedStock(directory);

    private static (Dictionary<AreaId, byte[]> Maps, Dictionary<AreaId, HashSet<int>> StationCells, byte[] Atlas, byte[] HudAtlas, byte[] HighlightCycle, byte[] Palettes, byte[] Labels, byte[] Stations, byte[] Landmarks, byte[] SaveMarkers, byte[] Arrows, byte[] Screens, byte[] WorldFront, byte[] WorldBack, byte[] SpriteJson, byte[] SpritePng, byte[] PauseTiles) ReadVerifiedStock(string directory)
    {
        AreaMapCatalogManifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<AreaMapCatalogManifest>(
                File.ReadAllBytes(Path.Combine(directory, AreaMapCatalogFormat.ManifestFile)), MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Map catalog manifest is null.");
        }
        catch (JsonException error) { throw new InvalidDataException($"Invalid map catalog manifest in {directory}.", error); }
        if (manifest.Version != AreaMapCatalogFormat.Version || manifest.Sha256 is null || manifest.Sha256.Count != AreaIds.RetailCount + 16)
            throw new InvalidDataException("Map catalog manifest must contain the supported version, seven maps and all sixteen shared presentation resource hashes.");
        var result = new Dictionary<AreaId, byte[]>();
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            string file = AreaMapCatalogFormat.FileName(area);
            result.Add(area, ReadChecked(file));
        }
        Dictionary<string, int[]> masks;
        try
        {
            masks = JsonSerializer.Deserialize<Dictionary<string, int[]>>(ReadChecked(AreaMapCatalogFormat.StationRevealFile), MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Station reveal content is null.");
        }
        catch (JsonException error) { throw new InvalidDataException("Invalid station reveal content.", error); }
        if (masks.Count != AreaIds.RetailCount) throw new InvalidDataException("Station reveal content requires all seven areas.");
        var stationCells = new Dictionary<AreaId, HashSet<int>>();
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            if (!masks.TryGetValue(area.ToString(), out int[]? cells) || cells is null ||
                cells.Any(cell => (uint)cell >= AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles) ||
                cells.Distinct().Count() != cells.Length)
                throw new InvalidDataException($"Invalid or duplicate station reveal cells for {area}.");
            stationCells.Add(area, cells.ToHashSet());
        }
        byte[] atlas = ReadChecked(MapTileAtlasFormat.FileName);
        _ = MapTileAtlas.Load(new MemoryStream(atlas, writable: false));
        byte[] hudAtlas = ReadChecked(HudTileAtlasFormat.FileName);
        _ = HudTileAtlas.Load(new MemoryStream(hudAtlas, writable: false));
        byte[] highlightCycle = ReadChecked(MapPaletteCycleFormat.FileName);
        _ = MapPaletteCycle.Load(new MemoryStream(highlightCycle, writable: false));
        byte[] palettes = ReadChecked(MapStaticPalettesFormat.FileName);
        _ = MapStaticPalettes.Load(new MemoryStream(palettes, writable: false));
        byte[] labels = ReadChecked(WorldMapLabelFormat.FileName);
        _ = WorldMapLabelLayout.Load(new MemoryStream(labels, writable: false));
        byte[] stations = ReadChecked(MapStationLayoutFormat.FileName);
        _ = MapStationLayout.Load(new MemoryStream(stations, writable: false));
        byte[] landmarks = ReadChecked(MapLandmarkFormat.FileName);
        _ = MapLandmarkLayout.Load(new MemoryStream(landmarks, writable: false));
        byte[] saveMarkers = ReadChecked(MapSaveMarkerFormat.FileName);
        _ = MapSaveMarkerLayout.Load(new MemoryStream(saveMarkers, writable: false));
        byte[] arrows = ReadChecked(MapArrowFormat.FileName);
        _ = MapArrowPresentation.Load(new MemoryStream(arrows, writable: false));
        byte[] screens = ReadChecked(MapScreenDefinitions.FileName);
        _ = MapScreenPresentation.Load(new MemoryStream(screens, writable: false));
        byte[] front = ReadChecked(WorldMapArtworkFormat.ForegroundFile), back = ReadChecked(WorldMapArtworkFormat.BackgroundFile);
        _ = WorldMapArtwork.Load(new MemoryStream(front, writable: false), new MemoryStream(back, writable: false));
        byte[] spriteJson = ReadChecked(MapSpriteFormat.JsonFile), spritePng = ReadChecked(MapSpriteFormat.PngFile);
        _ = MapSpriteCatalog.Load(new MemoryStream(spriteJson), new MemoryStream(spritePng));
        byte[] pauseTiles = ReadChecked(PauseTileAtlasFormat.FileName);
        _ = MapTileAtlas.Load(new MemoryStream(pauseTiles));
        return (result, stationCells, atlas, hudAtlas, highlightCycle, palettes, labels, stations, landmarks, saveMarkers, arrows, screens, front, back, spriteJson, spritePng, pauseTiles);

        byte[] ReadChecked(string file)
        {
            if (!manifest.Sha256.TryGetValue(file, out string? expected))
                throw new InvalidDataException($"Map catalog manifest is missing {file}.");
            byte[] bytes = File.ReadAllBytes(Path.Combine(directory, file));
            if (!string.Equals(expected, Convert.ToHexString(SHA256.HashData(bytes)), StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock map {file} failed its SHA-256 check. Put edits in the overrides directory, not stock content.");
            return bytes;
        }
    }
}

/// <summary>Stock content provenance, separate from a catalog's selected replacement identity.</summary>
public sealed record AreaMapCatalogManifest
{
    public required int Version { get; init; }
    public required string SourceCartridgeSha256 { get; init; }
    public required Dictionary<string, string> Sha256 { get; init; }
}

public static class AreaMapCatalogFormat
{
    public const int Version = 14;
    /// <summary>Bundled authored reveal mask: logical row-major cell indexes, not SRAM offsets or editable engine code.</summary>
    public const string StationRevealFile = "station-reveal.json";
    public const string ManifestFile = "manifest.json";
    public static string FileName(AreaId area)
    {
        _ = AreaIds.ToIndex(area);
        return area.ToString().ToLowerInvariant() + ".json";
    }
}
