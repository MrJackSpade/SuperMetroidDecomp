using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Bosses.Torizo;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Confirms the identified partial ordinary-enemy installation admission gap.</summary>
internal static class EnemyTileArtworkPresentationContractChecks
{
    /// <summary>Confirms empty ordinary artwork is rejected, then checks complete installed artwork and its static ownership contract.</summary>
    internal static void Run()
    {
        bool rejected = false;
        try { _ = EnemyTileSourceArtwork.FromInstalledArtwork(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() }); }
        catch (InvalidDataException) { rejected = true; }
        if (!rejected) throw new InvalidOperationException(
            "Enemy artwork confirmation failed: production construction must reject an empty ordinary installation.");
        ConfirmInstalledData();
        EnemyArtworkStaticContractChecks.Run();
    }

    /// <summary>Exercises complete identity admission, copied source aliases, and exact VRAM/CGRAM upload boundaries with constructed artwork.</summary>
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
                Enumerable.Range(0, EnemyPaletteSheet.ColorCount).Select(index => new RgbaPixel((byte)index, 0, 0)).ToArray());
            png.Position = 0;
            RoomCharacterAtlas atlas = RoomCharacterAtlasPng.Read(png, bytes);
            cached.Add(bytes, atlas);
            return atlas;
        }
        var sheets = EnemyTileSourceDefinitions.All.ToDictionary(definition => definition.DefinitionId,
            definition => Sheet(definition.ByteCount));
        using var paletteJson = new MemoryStream(EnemyPaletteSheetFile.Write(new EnemyPaletteSheetDocument {
            Version = 1, Colors = Enumerable.Range(0, EnemyPaletteSheet.ColorCount)
                .Select(_ => new PaletteRgb5 { Red = 1, Green = 0, Blue = 0 }).ToArray() }));
        EnemyPaletteSheet palette = EnemyPaletteSheetFile.Read(paletteJson);
        var palettes = EnemyTileSourceDefinitions.All.ToDictionary(definition => definition.DefinitionId, _ => palette);
        var ceres = new CeresEscapeTileArtwork(CeresEscapeTileArtworkDefinitions.All.Select(page => Sheet(page.ByteCount)).ToArray());
        using var overlayJson = new MemoryStream(CeresEscapeOverlays.Write(new CeresEscapeOverlayTilemapDocument {
            Version = CeresWarningGlyphDefinitions.Version,
            Pages = CeresEscapeOverlayTilemapDefinitions.All.ToDictionary(page => page.Name, page => new BackgroundTile[page.WordCount]) }));
        CeresEscapeOverlays overlays = CeresEscapeOverlays.Load(overlayJson);
        var torizo = new TorizoSwapArtwork(TorizoSwapArtworkDefinitions.All
            .Select(page => Sheet(page.ByteCount)).ToArray());
        EnemyTileSourceArtwork Install() => EnemyTileSourceArtwork.FromInstalledArtwork(new EnemyTileArtworkParts { Sheets = sheets, Palettes = palettes });
        EnemyTileSourceDefinition first = EnemyTileSourceDefinitions.All[0];
        EnemyTileSourceArtwork catalog = Install();
        _ = AreaSourceArtwork.FromInstalledArtwork(new AreaSourceArtwork { CeresEscapeTiles = ceres, CeresEscapeOverlayTilemaps = overlays });
        _ = BossSourceArtwork.FromInstalledArtwork(new BossSourceArtwork { TorizoSwaps = torizo });
        Reject(() => AreaSourceArtwork.FromInstalledArtwork(new AreaSourceArtwork { CeresEscapeTiles = ceres }), "required Ceres DMA extensions");
        Reject(() => BossSourceArtwork.FromInstalledArtwork(new BossSourceArtwork()), "required Torizo DMA extension");
        RoomCharacterAtlas original = sheets[first.DefinitionId];
        sheets.Remove(first.DefinitionId); sheets.Add(0, original);
        Reject(() => Install(), "same-count substituted identity");
        sheets.Remove(0);
        sheets[first.DefinitionId] = Sheet(first.ByteCount == 32 ? 64 : 32);
        Reject(() => Install(), "incorrect native sheet length");
        sheets[first.DefinitionId] = original;

        // Mutation of the caller-owned dictionaries cannot invalidate an admitted catalog.
        sheets.Clear(); palettes.Clear();
        Require(catalog.TilesOf(first.DefinitionId).Length == first.ByteCount, "copied installed sheet");
        ReadOnlyMemory<byte> bytes;
        Require(ceres.TryResolve(0, 1, 1, out bytes) && bytes.Length == 1,
            "bounded Ceres tile slice");
        CeresEscapeOverlayTilemapDefinition overlay = CeresEscapeOverlayTilemapDefinitions.Emergency;
        Require(overlays.TryResolve(overlay, out ReadOnlyMemory<BackgroundTile> tiles) &&
            tiles.Length == overlay.WordCount, "exact Ceres overlay source");
        var vram = new SnesVram();
        vram.LoadBytes(0, Enumerable.Repeat((byte)0xa5, first.ByteCount + 2).ToArray());
        catalog.LoadTo(first.DefinitionId, first.ByteCount, new VramRegion(vram, PpuMemoryLayout.Vram), 1);
        Require(VramBytes.At(vram, 0) == 0xa5 && VramBytes.At(vram, first.ByteCount + 1) == 0xa5 &&
            Enumerable.Range(1, first.ByteCount).All(offset => VramBytes.At(vram, offset) == 0), "exact tile upload and unchanged neighbors");
        var cgram = new ScreenPalette();
        ColourRun backgrounds = cgram.BackgroundSets.All;
        catalog.LoadPaletteTo(first.DefinitionId, backgrounds.Slice(1, 16));
        Require(backgrounds[0] == RgbColor.Black && backgrounds[1] == new RgbColor(1, 0, 0) && backgrounds[16] == new RgbColor(1, 0, 0) && backgrounds[17] == RgbColor.Black,
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
