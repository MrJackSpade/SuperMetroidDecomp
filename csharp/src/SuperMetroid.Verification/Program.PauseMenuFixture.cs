using SuperMetroid.Core.Frontend;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using static RetailPresentationFixtureVerification;

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
        T Construct<T>(params object[] arguments) => (T)typeof(T)
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single()
            .Invoke(arguments);
        T Document<T>(string name) => JsonSerializer.Deserialize<T>(
            File.ReadAllBytes(Path.Combine(installation.MapDirectory, name)),
            MapPresentationFormat.JsonOptions) ?? throw new InvalidDataException(name);
        MemoryStream Json<T>(T document) => new(JsonSerializer.SerializeToUtf8Bytes(
            document, MapPresentationFormat.JsonOptions));

        var pauseColors = new Bgr555[SnesCgram.ColorCount];
        byte[] paletteBytes = Read(0xb6f000, CgramPalettes.ByteCount);
        for (int i = 0; i < pauseColors.Length; i++)
            pauseColors[i] = Bgr555.FromCgramPortWord((ushort)(SnesLittleEndian.Word(paletteBytes[i * 2], paletteBytes[i * 2 + 1])));
        var palettes = Construct<MapStaticPalettes>(pauseColors, stock.Palettes.FileSelect.ToArray(),
            Enum.GetValues<AreaId>().Where(area => area != AreaId.Ceres)
                .ToDictionary(area => area, area => stock.Palettes.World(area).ToArray()));
        var areas = Enum.GetValues<AreaId>().Select(stock.Get).ToArray();
        areas[AreaIds.ToIndex(AreaId.Crateria)] = AreaMapImporter.Load(bus, AreaId.Crateria);
        var backdrops = PauseBackdropPresentation.FromTilemaps(
            Enum.GetValues<AreaId>().Select(_ => Read(0xb6e000, 0x0800)).ToArray(),
            Read(PauseBackdropDefinitions.ButtonSource, PauseBackdropDefinitions.ButtonCells * 2));
        var hud = new byte[HudTileAtlasFormat.TransferByteCount];
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
        var overrides = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["areas"] = areas,
            ["contentIdentity"] = "constructed-pause-interaction-fixture",
            ["tiles"] = Construct<MapTileAtlas>(Read(MapTileAtlasFormat.SourceAddress, MapTileAtlasFormat.ByteCount)),
            ["pauseTiles"] = Construct<MapTileAtlas>(Read(PauseTileAtlasFormat.SourceAddress, PauseTileAtlasFormat.ByteCount)),
            ["hudTiles"] = Construct<HudTileAtlas>(hud),
            ["palettes"] = palettes,
            ["pauseBackdrops"] = backdrops,
            ["sprites"] = MapSpriteCatalog.Load(spriteJson, spritePng),
            ["pauseSelectors"] = PauseSelectorPresentation.Load(selectorJson),
            ["pauseReserveTanks"] = PauseReserveTankPresentation.Load(reserveJson),
        };
        // Every part comes from the stock catalog's same-named property unless overridden above.
        var parts = (AreaMapPresentationParts)RuntimeHelpers.GetUninitializedObject(typeof(AreaMapPresentationParts));
        foreach (PropertyInfo part in typeof(AreaMapPresentationParts).GetProperties())
            part.SetValue(parts, overrides.TryGetValue(char.ToLowerInvariant(part.Name[0]) + part.Name[1..], out var value) ? value :
                typeof(AreaMapPresentationCatalog).GetProperty(part.Name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)!.GetValue(stock));
        return (AreaMapPresentationCatalog)PrivateState.Construct(typeof(AreaMapPresentationCatalog), parts);
    }
}
