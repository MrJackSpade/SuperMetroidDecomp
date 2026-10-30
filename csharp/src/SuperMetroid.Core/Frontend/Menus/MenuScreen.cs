using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Rendering;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>
/// The shared menu screen <c>LoadInitialMenuTiles</c> ($81:8DDB) sets up for file select, options, game over and the
/// file-select maps (#1358): the installed artwork it draws, its palette and its sprite entries; its pages are installed
/// pictures it places (#1467). It publishes these; the capture composes them, so no menu owns PPU memory.
/// </summary>
internal sealed class MenuScreen
{
    /// <summary>Installs the shared world and palette artwork, and the sheets its backgrounds draw from.</summary>
    public MenuScreen(MapStaticPalettes? mapPalettes, WorldMapArtwork? worldArtwork, InstalledSpriteSheets? sheets)
    {
        WorldArtwork = RequireWorldArtwork(worldArtwork);
        Objects = new ScreenSprites();
        BindMapPalettes(mapPalettes);
        BindScreenTiles(sheets);
    }

    /// <summary>The installed sheets the screen's pictures come from (#1467).</summary>
    [field: NonSerialized]
    [AllowNull]
    private InstalledSpriteSheets Sheets { get => field ?? throw new InvalidOperationException("The menu's pictures are rebound after debugger restoration."); set; }

    /// <summary>Rebinds the sheets the screen's pictures come from (#1467).</summary>
    public void BindScreenTiles(InstalledSpriteSheets? sheets) =>
        Sheets = sheets ?? throw new InvalidOperationException("Menus require the installed sprite sheets.");

    /// <summary>The size of a menu page in cells: one screen's tilemap, which a scrolled page repeats across.</summary>
    internal static Extent16<TilemapCells> PageSize { get; } =
        new(ScreenGeometry.TilemapScreenWidthInTiles, ScreenGeometry.TilemapScreenWidthInTiles);

    /// <summary>
    /// A page showing the menus' pictures named <paramref name="frames"/>, each placed at the page's corner in drawing order,
    /// a later one over an earlier one (#1467): each lands where the cartridge's page shows it.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed menu pictures hold no such frame.</exception>
    internal PicturedPlane PagePictures(params ReadOnlySpan<string> frames)
    {
        SpriteSheet pictures = Sheets.Get(SpriteSheetId.MenuPictures);
        var placed = ImmutableArray.CreateBuilder<PlacedPlanePicture>(frames.Length);
        foreach (string frame in frames)
            placed.Add(Place(pictures.Frame(frame), default));
        return new PicturedPlane(PageSize, placed.MoveToImmutable());
    }

    /// <summary>
    /// A page showing the menus' pictures named in <paramref name="pictures"/>, each placed at its page cell in drawing order
    /// (#1467): a field's picture lands where the cartridge writes that field.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed menu pictures hold no such frame.</exception>
    internal PicturedPlane CellPictures(IEnumerable<PlacedFrame> pictures)
    {
        SpriteSheet sheet = Sheets.Get(SpriteSheetId.MenuPictures);
        return new PicturedPlane(PageSize, [.. Ensure.NotNull(pictures).Select(placed => Place(sheet.Frame(placed.Frame), placed.Cell))]);
    }

    /// <summary>
    /// <paramref name="picture"/> placed at page cell <paramref name="cell"/>: the point its frame names as its origin lands on
    /// the cell's corner. A picture reaching past the page's top or left edge continues from the opposite edge.
    /// </summary>
    private static PlacedPlanePicture Place(SpriteSheetFrame picture, Position16<TilemapCells> cell)
    {
        int page = PageSize.X * ScreenGeometry.BackgroundTileSizePixels;
        int left = cell.X * ScreenGeometry.BackgroundTileSizePixels - picture.Origin.X;
        int top = cell.Y * ScreenGeometry.BackgroundTileSizePixels - picture.Origin.Y;
        return new(picture, new((ushort)(((left % page) + page) % page), (ushort)(((top % page) + page) % page)));
    }

    /// <summary>The installed world-map characters.</summary>
    [field: NonSerialized]
    [AllowNull]
    public WorldMapArtwork WorldArtwork { get => field ?? throw new InvalidOperationException("The menu's world-map characters are rebound after debugger restoration."); private set; }


    /// <summary>The menu's palette.</summary>
    public ScreenPalette Palette { get; } = new();

    /// <summary>The menu's sprite entries over the installed menu OBJ page.</summary>
    public ScreenSprites Objects { get; }

    /// <summary>Rebinds the world characters, keeping tilemaps and ongoing palette state.</summary>
    public void BindWorldArtwork(WorldMapArtwork? worldArtwork)
    {
        WorldArtwork = RequireWorldArtwork(worldArtwork);
    }


    /// <summary>Loads the file-select palette image.</summary>
    public void BindMapPalettes(MapStaticPalettes? mapPalettes)
    {
        MapStaticPalettes palettes = mapPalettes ?? throw new InvalidOperationException("Menus require installed palettes.");
        palettes.FileSelect.LoadTo(Palette);
    }

    /// <summary>The installed world-map characters, which a menu cannot draw without.</summary>
    private static WorldMapArtwork RequireWorldArtwork(WorldMapArtwork? worldArtwork) =>
        worldArtwork ?? throw new InvalidOperationException("Menus require installed world-map artwork.");


    /// <summary>What the screen paints with: its palette and sprite entries.</summary>
    internal FramePaint Paint() => new(Palette.Shown(), Objects.Draws);
}
