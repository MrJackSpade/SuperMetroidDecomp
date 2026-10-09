using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the identified partial ordinary-enemy installation admission gap.</summary>
internal static class EnemyTileArtworkPresentationContractChecks
{
    /// <summary>Confirms enemy tile artwork construction and installed-data presentation contracts.</summary>
    internal static void Run()
    {
        bool rejected = false;
        try { _ = EnemyTileArtworkCatalog.FromInstalledArtwork(new Dictionary<ushort, RoomCharacterAtlas>(),
            new Dictionary<ushort, EnemyPaletteSheet>()); }
        catch (InvalidDataException) { rejected = true; }
        if (!rejected) throw new InvalidOperationException(
            "Enemy artwork confirmation failed: production construction must reject an empty ordinary installation.");
        ConfirmInstalledData();
        EnemyArtworkStaticContractChecks.Run();
    }

    /// <summary>Confirms installed enemy tile sheets retain exact compiled identities and transfer sizes.</summary>
    private static void ConfirmInstalledData()
    {
        var cached = new Dictionary<int, RoomCharacterAtlas>();
        RoomCharacterAtlas Sheet(int bytes)
        {
            if (cached.TryGetValue(bytes, out RoomCharacterAtlas? existing)) return existing;
            int tiles = RoomCharacterAtlasFormat.ValidateTileCount(bytes);
            int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tiles), rows = (tiles + columns - 1) / columns;
            using var png = new MemoryStream();
            IndexedPng.Write(png, columns * 8, rows * 8, new byte[columns * rows * 64],
                Enumerable.Range(0, EnemyPaletteSheet.ColorCount).Select(index => new Rgba32((byte)index, 0, 0)).ToArray());
            png.Position = 0;
            RoomCharacterAtlas atlas = RoomCharacterAtlas.Load(png, bytes);
            cached.Add(bytes, atlas);
            return atlas;
        }
        var sheets = EnemyTileSourceDefinitions.All.ToDictionary(definition => definition.DefinitionPointer,
            definition => Sheet(definition.ByteCount));
        using var paletteJson = new MemoryStream(EnemyPaletteSheet.Write(new EnemyPaletteSheetDocument {
            Version = 1, Colors = Enumerable.Range(0, EnemyPaletteSheet.ColorCount)
                .Select(_ => new PaletteRgb5 { Red = 1, Green = 0, Blue = 0 }).ToArray() }));
        EnemyPaletteSheet palette = EnemyPaletteSheet.Load(paletteJson);
        var palettes = EnemyTileSourceDefinitions.All.ToDictionary(definition => definition.DefinitionPointer, _ => palette);
        var sources = EnemyTileSourceDefinitions.All.ToDictionary(definition => definition.DefinitionPointer, definition => definition.SourceAddress);
        var ceres = new CeresEscapeTileArtwork(CeresEscapeTileArtworkDefinitions.All.ToArray().Select(page => Sheet(page.ByteCount)).ToArray());
        using var overlayJson = new MemoryStream(CeresEscapeOverlayTilemapCatalog.Write(new CeresEscapeOverlayTilemapDocument {
            Version = CeresEscapeOverlayTilemapDefinitions.Version,
            Pages = CeresEscapeOverlayTilemapDefinitions.All.ToArray().ToDictionary(page => page.Name, page => new ushort[page.WordCount]) }));
        CeresEscapeOverlayTilemapCatalog overlays = CeresEscapeOverlayTilemapCatalog.Load(overlayJson);
        var torizo = new TorizoInstructionVramArtwork(TorizoInstructionVramArtworkDefinitions.All.ToArray()
            .Select(page => Sheet(page.ByteCount)).ToArray());
        EnemyTileArtworkCatalog Install() => EnemyTileArtworkCatalog.FromInstalledArtwork(sheets, palettes,
            dmaSources: sources, ceresEscapeTiles: ceres, ceresEscapeOverlayTilemaps: overlays,
            torizoInstructionVram: torizo);
        EnemyTileSourceDefinition first = EnemyTileSourceDefinitions.All[0];
        EnemyTileArtworkCatalog catalog = Install();
        Reject(() => EnemyTileArtworkCatalog.FromInstalledArtwork(sheets, palettes), "required Ceres DMA extensions");
        Reject(() => EnemyTileArtworkCatalog.FromInstalledArtwork(sheets, palettes,
            ceresEscapeTiles: ceres, ceresEscapeOverlayTilemaps: overlays), "required Torizo DMA extension");
        RoomCharacterAtlas original = sheets[first.DefinitionPointer];
        sheets.Remove(first.DefinitionPointer); sheets.Add(0, original);
        Reject(() => Install(), "same-count substituted identity");
        sheets.Remove(0); sheets.Add(first.DefinitionPointer, null!);
        Reject(() => Install(), "null required sheet");
        sheets[first.DefinitionPointer] = Sheet(first.ByteCount == 32 ? 64 : 32);
        Reject(() => Install(), "incorrect native sheet length");
        sheets[first.DefinitionPointer] = original;
        palettes[first.DefinitionPointer] = null!;
        Reject(() => Install(), "null required palette");
        palettes[first.DefinitionPointer] = palette;
        sources[first.DefinitionPointer]++;
        Reject(() => Install(), "incorrect native DMA alias");

        // Mutation of the caller-owned dictionaries cannot invalidate an admitted catalog.
        sheets.Clear(); palettes.Clear(); sources.Clear();
        Require(catalog.TryResolve(first.SourceAddress, first.ByteCount, out ReadOnlyMemory<byte> bytes) &&
            bytes.Length == first.ByteCount, "copied native DMA alias");
        Require(!catalog.TryResolve(0, int.MinValue, out _), "unowned source remains a false query");
        CeresEscapeTileSheetDefinition warning = CeresEscapeTileArtworkDefinitions.WarningText;
        Require(catalog.TryResolve(warning.SourceAddress + 1, 1, out bytes) && bytes.Length == 1,
            "bounded Ceres tile slice");
        CeresEscapeOverlayTilemapDefinition overlay = CeresEscapeOverlayTilemapDefinitions.Emergency;
        Require(catalog.TryResolve(overlay.SourceAddress, overlay.WordCount * sizeof(ushort), out bytes) &&
            bytes.Length == overlay.WordCount * sizeof(ushort), "exact Ceres overlay source");
        var vram = new SnesVram();
        vram.LoadBytes(0, Enumerable.Repeat((byte)0xa5, first.ByteCount + 2).ToArray());
        catalog.LoadTo(first.DefinitionPointer, first.ByteCount, vram, 1);
        Require(vram.ReadByte(0) == 0xa5 && vram.ReadByte(first.ByteCount + 1) == 0xa5 &&
            vram.Bytes.Slice(1, first.ByteCount).IndexOfAnyExcept((byte)0) < 0, "exact tile upload and unchanged neighbors");
        var cgram = new SnesCgram();
        catalog.LoadPaletteTo(first.DefinitionPointer, cgram, 1);
        Require(cgram.Colors[0] == 0 && cgram.Colors[1] == 1 && cgram.Colors[16] == 1 && cgram.Colors[17] == 0,
            "exact palette upload and unchanged neighbors");
    }

    /// <summary>Requires an enemy tile artwork construction action to reject invalid data.</summary>
    private static void Reject(Action action, string reason)
    {
        try { action(); }
        catch (InvalidDataException) { return; }
        throw new InvalidOperationException("Enemy artwork confirmation did not reject " + reason);
    }

    /// <summary>Throws when an enemy tile artwork expectation is not satisfied.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Enemy artwork confirmation failed: " + reason);
    }
}
