using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>
/// Pictures every image the gameplay HUD can place (#1466): its top row and template, each energy tank, each numeral,
/// each item icon's cells in each shade, each AUTO cell as the indicator shows and every minimap cell the installed maps
/// can show, each its own frame named by the image. Each room tileset's BG3 sheet pictures them under its palette; the
/// pause HUD sheet pictures them under the pause palette, with the cleared cell pause fills its HUD plane with.
/// </summary>
internal static class HudImageSheet
{
    /// <summary>Cells across a sheet's shelves: the HUD's width.</summary>
    private const int ShelfCells = GameplayHudDefinitions.Width;

    /// <summary>
    /// Every image the HUD can place with <paramref name="maps"/>' cells on its minimap, in a stable order: the minimap
    /// keeps showing the last area's map until it next updates, as across an area's elevator, so every area's cells can
    /// show in any room.
    /// </summary>
    internal static IReadOnlyList<HudImage> Images(IEnumerable<IAreaMapView> maps)
    {
        var images = new List<HudImage> { HudTopRowImage.Instance, HudTemplateImage.Instance };
        images.AddRange(Enum.GetValues<HudEnergyTank>().Select(tank => new HudEnergyTankImage(tank)));
        for (int numeral = 0; numeral < GameplayHudDefinitions.Numerals; numeral++)
            images.Add(new HudEnergyDigitImage(numeral));
        for (int numeral = 0; numeral < GameplayHudDefinitions.Numerals; numeral++)
            images.Add(new HudAmmoDigitImage(numeral));
        HudIconShade?[] shades = [null, .. Enum.GetValues<HudIconShade>().Select(shade => (HudIconShade?)shade)];
        for (int item = 0; item < GameplayHudDefinitions.ItemCount; item++)
        {
            foreach (HudIconCover cover in Enum.GetValues<HudIconCover>())
            {
                // The template unshaded is the template itself, which the HUD never places over its own cells.
                foreach (HudIconShade? shade in shades.Where(shade => cover == HudIconCover.Icon || shade is not null))
                    images.Add(new HudIconImage(item, cover, shade));
            }
        }
        foreach (HudReserveIndicator indicator in Enum.GetValues<HudReserveIndicator>())
        {
            for (int cell = 0; cell < GameplayHudDefinitions.AutoReserveCellCount; cell++)
                images.Add(new HudReserveCellImage(indicator, cell));
        }
        images.Add(HudMinimapHiddenImage.Instance);
        var minimap = new HashSet<HudMinimapCellImage>();
        foreach (IAreaMapView map in Ensure.NotNull(maps))
        {
            for (int y = 0; y < AreaMapLayout.HeightInTiles; y++)
            for (int x = 0; x < AreaMapLayout.WidthInTiles; x++)
            {
                MapCell cell = map.Cell(new Position16<MapTiles>((ushort)x, (ushort)y));
                foreach (MinimapShade shade in Enum.GetValues<MinimapShade>())
                {
                    var image = new HudMinimapCellImage(cell.Shape, cell.Mirroring, shade);
                    if (minimap.Add(image))
                        images.Add(image);
                }
            }
        }
        return images;
    }

    /// <summary>Every installed area map, in area order, whose cells the minimap can show.</summary>
    internal static IEnumerable<IAreaMapView> Maps(AreaMapPresentation maps) =>
        Enum.GetValues<AreaId>().Select(Ensure.NotNull(maps).Get);

    /// <summary>
    /// Lays every one of <paramref name="hud"/>'s images out on shelves across the HUD's width, each image left of the next
    /// until a shelf has no room for it.
    /// </summary>
    internal static HudSheetLayout Layout(GameplayHudArtwork hud, IEnumerable<IAreaMapView> maps)
    {
        Ensure.NotNull(hud);
        var shelves = new List<int>();
        var placements = ImmutableArray.CreateBuilder<HudSheetPlacement>();
        int column = ShelfCells;
        foreach (HudImage image in Images(maps))
        {
            HudImageTiles tiles = hud.Tiles(image);
            if (column + tiles.Size.X > ShelfCells)
            {
                shelves.Add(0);
                column = 0;
            }
            int shelf = shelves.Count - 1;
            shelves[shelf] = Math.Max(shelves[shelf], tiles.Size.Y);
            placements.Add(new HudSheetPlacement(tiles, shelf, new Position16<TilemapCells>((ushort)column, 0)));
            column += tiles.Size.X;
        }
        return new HudSheetLayout([.. shelves], placements.ToImmutable());
    }

    /// <summary>The frame names of the pause HUD sheet: each HUD image the installed maps let the HUD place, and the cleared cell.</summary>
    internal static IEnumerable<string> PauseFrameNames(AreaMapPresentation maps) =>
        [.. Images(Maps(maps)).Select(image => image.FrameName), PauseHudDefinitions.ClearedCellFrame];

    /// <summary>
    /// Composes the pause HUD sheet: every HUD image <paramref name="maps"/>' HUD can place, painted from the HUD's
    /// characters under the pause palette, then the cleared cell pause fills its HUD plane with beneath the HUD. A tile
    /// naming a character beyond the HUD's shows nothing, as pause shows nothing there.
    /// </summary>
    internal static ComposedSpriteSheet ComposePause(AreaMapPresentation maps)
    {
        Ensure.NotNull(maps);
        ReadOnlyMemory<byte> characters = MapCharacterArtwork.Of(maps).Hud.Bytes[..HudTileAtlasFormat.CharacterByteCount];
        HudSheetLayout layout = Layout(GameplayHudArtwork.Of(maps.GameplayHud), Maps(maps));
        const int cell = RoomTileCanvas.CharacterPixels;
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(ShelfCells * cell, checked((ushort)((layout.Rows + 1) * cell))));
        RgbColor[] colours = ScreenColourImage.Encode(maps.Palettes.Pause)[..SpriteImage.SpriteColourCount];
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal);
        foreach (HudSheetPlacement placed in layout.Placements)
        {
            var corner = new Position16<TilemapCells>(placed.Cell.X, checked((ushort)(layout.ShelfTop(placed.Shelf) + placed.Cell.Y)));
            HudImageTiles image = placed.Image;
            for (int y = 0; y < image.Size.Y; y++)
            for (int x = 0; x < image.Size.X; x++)
                Paint(new Position16<TilemapCells>((ushort)(corner.X + x), (ushort)(corner.Y + y)), image.Tiles[y * image.Size.X + x]);
            frames.Add(image.Image.FrameName, TileWordSheetComposer.Frame(canvas, Pixels(corner), Pixels(image.Size), TileWordLayer.Bg3));
        }
        var cleared = new Position16<TilemapCells>(0, checked((ushort)layout.Rows));
        Paint(cleared, PauseHudPlaneDefinitions.ClearedCell);
        frames.Add(PauseHudDefinitions.ClearedCellFrame, TileWordSheetComposer.Frame(canvas, Pixels(cleared), Pixels(new Extent16<TilemapCells>(1, 1)), TileWordLayer.Bg3));
        return canvas.ToSheet(colours, frames, ScreenTileSheet.ColoursOf(TileInkDepth.FourColours));

        void Paint(Position16<TilemapCells> at, BackgroundTile tile)
        {
            int start = tile.Tile * RoomTileCanvas.TwoBppCharacterBytes;
            if (start + RoomTileCanvas.TwoBppCharacterBytes > characters.Length)
                return;
            canvas.PaintTwoBppTile(at.X * cell, at.Y * cell, tile, characters.Span.Slice(start, RoomTileCanvas.TwoBppCharacterBytes), colours);
        }
    }

    /// <summary>The pixel corner of cell <paramref name="cell"/>.</summary>
    internal static Position16<ImagePixels> Pixels(Position16<TilemapCells> cell) =>
        new(checked((ushort)(cell.X * RoomTileCanvas.CharacterPixels)), checked((ushort)(cell.Y * RoomTileCanvas.CharacterPixels)));

    /// <summary>The pixel size of <paramref name="size"/> cells.</summary>
    internal static Extent16<ImagePixels> Pixels(Extent16<TilemapCells> size) =>
        new(checked((ushort)(size.X * RoomTileCanvas.CharacterPixels)), checked((ushort)(size.Y * RoomTileCanvas.CharacterPixels)));
}
