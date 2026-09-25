using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports the remaining Ceres map slices and Zebes reveal transfers.</summary>
internal static class CeresDestructionArtworkExtractor
{
    public static IReadOnlyDictionary<string, byte[]> Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        byte[] allCeresMaps = RomDataReader.Decompress(bus,
            CeresDestructionRomData.Assets.CeresTilemaps,
            maximumOutputBytes: CeresDestructionRomData.Vram.CompressedTilemapLimit);
        int firstDestructionByte = 2 * CeresFlightArtworkFormat.MapCellsPerView;
        if (allCeresMaps.Length < firstDestructionByte + CeresDestructionArtworkFormat.MapByteCount)
            throw new InvalidDataException("Ceres destruction lacks its three native map slices.");
        var views = new int[CeresDestructionArtworkFormat.ViewCount][];
        for (int view = 0; view < views.Length; view++)
            views[view] = allCeresMaps.AsSpan(firstDestructionByte +
                    view * CeresDestructionArtworkFormat.CellsPerView,
                    CeresDestructionArtworkFormat.CellsPerView)
                .ToArray().Select(tile => (int)tile).ToArray();
        using var mapJson = new MemoryStream();
        CeresDestructionArtworkCatalog.WriteMap(mapJson, new CeresDestructionMapDocument
        {
            Version = CeresDestructionArtworkFormat.Version,
            Width = CeresDestructionArtworkFormat.MapWidth,
            Height = CeresDestructionArtworkFormat.MapHeight,
            Views = views,
        });

        byte[] zebesMap = RomDataReader.Decompress(bus,
            CeresDestructionRomData.Assets.ZebesTilemap,
            maximumOutputBytes: CeresDestructionRomData.Vram.CompressedTilemapLimit);
        if (zebesMap.Length < CeresDestructionArtworkFormat.ZebesMapByteCount)
            throw new InvalidDataException("Zebes reveal tilemap is shorter than its native transfer.");
        byte[] zebesMapJson = RoomBackgroundTilemapExtractor.Encode(
            zebesMap.AsSpan(0, CeresDestructionArtworkFormat.ZebesMapByteCount));

        byte[] zebesCharacters = RomDataReader.Decompress(bus,
            CeresDestructionRomData.Assets.ZebesCharacters,
            maximumOutputBytes: CeresDestructionArtworkFormat.ZebesCharacterByteCount);
        if (zebesCharacters.Length != CeresDestructionArtworkFormat.ZebesCharacterByteCount)
            throw new InvalidDataException("Zebes reveal character stream has the wrong transfer length.");
        byte[] pixels = SnesGraphics.DecodePlanarTiles(zebesCharacters, 4,
            IntroCinematicArtworkFormat.TileColumns, out int width, out int height);
        using var png = new MemoryStream();
        IndexedPng.Write(png, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
        byte[] zebesPng = png.ToArray();

        var frames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
        foreach (CeresDestructionSpriteFrameDefinition definition in
            CeresDestructionSpriteDefinitions.Frames)
            frames.Add(definition.Name, IntroCinematicSpriteFrameExtractor.Extract(
                bus, definition.Pointer, definition.StockPartCount, definition.Name));
        using var spritesJson = new MemoryStream();
        CeresDestructionSpritePresentation.Write(spritesJson,
            new CeresDestructionSpriteDocument
            {
                Version = CeresDestructionSpriteFormat.Version,
                Frames = frames,
            });
        byte[] spritesFile = spritesJson.ToArray();

        CeresDestructionArtworkCatalog compiled = CeresDestructionArtworkCatalog.Load(
            new MemoryStream(mapJson.ToArray(), writable: false),
            new MemoryStream(zebesMapJson, writable: false),
            new MemoryStream(zebesPng, writable: false),
            new MemoryStream(spritesFile, writable: false));
        if (!compiled.CeresMaps.Span.SequenceEqual(allCeresMaps.AsSpan(firstDestructionByte,
                CeresDestructionArtworkFormat.MapByteCount)) ||
            !compiled.ZebesMap.Transfer.Span.SequenceEqual(zebesMap.AsSpan(0,
                CeresDestructionArtworkFormat.ZebesMapByteCount)) ||
            !compiled.ZebesCharacters.Transfer.Span.SequenceEqual(zebesCharacters))
            throw new InvalidDataException("Ceres destruction PNG/JSON export changed native bytes.");
        return new Dictionary<string, byte[]>(StringComparer.Ordinal)
        {
            [CeresDestructionArtworkFormat.CeresMapFileName] = mapJson.ToArray(),
            [CeresDestructionArtworkFormat.ZebesMapFileName] = zebesMapJson,
            [CeresDestructionArtworkFormat.ZebesCharacterFileName] = zebesPng,
            [CeresDestructionSpriteFormat.FileName] = spritesFile,
        };
    }
}
