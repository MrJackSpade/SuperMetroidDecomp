using SuperMetroid.Core.Areas;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Confirms the identified partial ordinary-enemy installation admission gap.</summary>
internal static class EnemyTileArtworkPresentationContractChecks
{
    /// <summary>Confirms empty ordinary artwork is rejected, then checks complete installed artwork and its static ownership contract.</summary>
    internal static void Run()
    {
        bool rejected = false;
        try { _ = EnemyTileArtworkCatalog.FromInstalledArtwork(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() }); }
        catch (InvalidDataException) { rejected = true; }
        if (!rejected) throw new InvalidOperationException(
            "Enemy artwork confirmation failed: production construction must reject an empty ordinary installation.");
        ConfirmInstalledData();
        EnemyArtworkStaticContractChecks.Run();
    }

    /// <summary>Exercises complete identity admission, copied source aliases, and exact VRAM/CGRAM upload boundaries with constructed artwork.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
    private static void ConfirmInstalledData()
    {
        var cached = new Dictionary<int, RoomCharacterAtlas>();
        RoomCharacterAtlas Sheet(int bytes)
        {
            if (cached.TryGetValue(bytes, out RoomCharacterAtlas? existing)) return existing;
            int tiles = RoomCharacterAtlasFormat.ValidateTileCount(bytes);
            int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tiles), rows = (tiles + columns - 1) / columns;
            using var png = new MemoryStream();
            IndexedPng.Write(png, new(checked((ushort)(columns * 8)), checked((ushort)(rows * 8))), new byte[columns * rows * 64],
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
        var ceres = new CeresEscapeTileArtwork(CeresEscapeTileArtworkDefinitions.All.Select(page => Sheet(page.ByteCount)).ToArray());
        using var overlayJson = new MemoryStream(CeresEscapeOverlayTilemapCatalog.Write(new CeresEscapeOverlayTilemapDocument {
            Version = CeresEscapeOverlayTilemapDefinitions.Version,
            Pages = CeresEscapeOverlayTilemapDefinitions.All.ToDictionary(page => page.Name, page => new ushort[page.WordCount]) }));
        CeresEscapeOverlayTilemapCatalog overlays = CeresEscapeOverlayTilemapCatalog.Load(overlayJson);
        var torizo = new TorizoInstructionVramArtwork(TorizoInstructionVramArtworkDefinitions.All
            .Select(page => Sheet(page.ByteCount)).ToArray());
        EnemyTileArtworkCatalog Install() => EnemyTileArtworkCatalog.FromInstalledArtwork(new EnemyTileArtworkParts { Sheets = sheets, Palettes = palettes, DmaSources = sources });
        EnemyTileSourceDefinition first = EnemyTileSourceDefinitions.All[0];
        EnemyTileArtworkCatalog catalog = Install();
        _ = AreaArtworkCatalog.FromInstalledArtwork(new AreaArtworkCatalog { CeresEscapeTiles = ceres, CeresEscapeOverlayTilemaps = overlays });
        _ = BossArtworkCatalog.FromInstalledArtwork(new BossArtworkCatalog { TorizoInstructionVram = torizo });
        Reject(() => AreaArtworkCatalog.FromInstalledArtwork(new AreaArtworkCatalog { CeresEscapeTiles = ceres }), "required Ceres DMA extensions");
        Reject(() => BossArtworkCatalog.FromInstalledArtwork(new BossArtworkCatalog()), "required Torizo DMA extension");
        RoomCharacterAtlas original = sheets[first.DefinitionPointer];
        sheets.Remove(first.DefinitionPointer); sheets.Add(0, original);
        Reject(() => Install(), "same-count substituted identity");
        sheets.Remove(0);
        sheets[first.DefinitionPointer] = Sheet(first.ByteCount == 32 ? 64 : 32);
        Reject(() => Install(), "incorrect native sheet length");
        sheets[first.DefinitionPointer] = original;
        sources[first.DefinitionPointer]++;
        Reject(() => Install(), "incorrect native DMA alias");

        // Mutation of the caller-owned dictionaries cannot invalidate an admitted catalog.
        sheets.Clear(); palettes.Clear(); sources.Clear();
        Require(catalog.TryResolve(first.SourceAddress, first.ByteCount, out ReadOnlyMemory<byte> bytes) &&
            bytes.Length == first.ByteCount, "copied native DMA alias");
        Require(!catalog.TryResolve(0, int.MinValue, out _), "unowned source remains a false query");
        TileSheetDefinition warning = CeresEscapeTileArtworkDefinitions.WarningText;
        Require(ceres.TryResolve(warning.SourceAddress + 1, 1, out bytes) && bytes.Length == 1,
            "bounded Ceres tile slice");
        CeresEscapeOverlayTilemapDefinition overlay = CeresEscapeOverlayTilemapDefinitions.Emergency;
        Require(overlays.TryResolve(overlay.SourceAddress, overlay.WordCount * sizeof(ushort), out bytes) &&
            bytes.Length == overlay.WordCount * sizeof(ushort), "exact Ceres overlay source");
        var vram = new SnesVram();
        vram.LoadBytes(0, Enumerable.Repeat((byte)0xa5, first.ByteCount + 2).ToArray());
        catalog.LoadTo(first.DefinitionPointer, first.ByteCount, new VramStore(vram).All, 1);
        Require(vram.ReadByte(0) == 0xa5 && vram.ReadByte(first.ByteCount + 1) == 0xa5 &&
            vram.Bytes.Slice(1, first.ByteCount).IndexOfAnyExcept((byte)0) < 0, "exact tile upload and unchanged neighbors");
        var cgram = new SnesCgram();
        catalog.LoadPaletteTo(first.DefinitionPointer, new CgramPalettes(cgram).All.Slice(1, 16));
        Require(cgram.Colors[0] == Bgr555.Black && cgram.Colors[1] == new Bgr555(1, 0, 0) && cgram.Colors[16] == new Bgr555(1, 0, 0) && cgram.Colors[17] == Bgr555.Black,
            "exact palette upload and unchanged neighbors");
    }

    /// <summary>Requires an invalid installation to fail with an asset-data error.</summary>
    private static void Reject(Action action, string reason)
    {
        try { action(); }
        catch (InvalidDataException) { return; }
        throw new InvalidOperationException("Enemy artwork confirmation did not reject " + reason);
    }

    /// <summary>Reports a failed positive artwork contract check with its specific predicate.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Enemy artwork confirmation failed: " + reason);
    }
}
