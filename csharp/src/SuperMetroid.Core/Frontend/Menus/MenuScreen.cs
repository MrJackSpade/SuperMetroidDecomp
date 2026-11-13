using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>
/// The shared menu screen loaded for file select, options, game over and the
/// file-select maps (#1358): its backdrop and its sprite entries; its pages are installed
/// pictures it places (#1467), each in the colours the import pre-coloured it in (#1512). It publishes these; the capture
/// composes them, so no menu composes a screen of its own.
/// </summary>
internal sealed class MenuScreen
{
    /// <summary>Installs the screen's backdrop and the sheets its backgrounds draw from.</summary>
    /// <param name="backdrop">The solid colour the screen shows where nothing is drawn (#1512).</param>
    /// <param name="sheets">The installed sheets the screen's pictures come from.</param>
    public MenuScreen(RgbColor backdrop, InstalledSpriteSheets? sheets)
    {
        Objects = new ScreenSprites();
        Backdrop = backdrop;
        BindScreenTiles(sheets);
    }

    /// <summary>The installed sheets the screen's pictures come from (#1467).</summary>
    [AllowNull]
    private InstalledSpriteSheets Sheets { get => field ?? throw new InvalidOperationException("The menu's pictures are bound when the game is bound to its installed content."); set; }

    /// <summary>Rebinds the sheets the screen's pictures come from (#1467).</summary>
    public void BindScreenTiles(InstalledSpriteSheets? sheets) =>
        Sheets = sheets ?? throw new InvalidOperationException("Menus require the installed sprite sheets.");

    /// <summary>
    /// A page showing the menus' pictures named <paramref name="frames"/>, each placed at the page's corner in drawing order,
    /// a later one over an earlier one (#1467): each lands where the game's page shows it, and the page shows nothing
    /// beyond its pictures (#1555).
    /// </summary>
    /// <exception cref="InvalidDataException">The installed menu pictures hold no such frame.</exception>
    internal PicturedPlane PagePictures(params ReadOnlySpan<string> frames)
    {
        SpriteSheet pictures = Sheets.Get(SpriteSheetId.MenuPictures);
        var placed = ImmutableArray.CreateBuilder<PlacedPlanePicture>(frames.Length);
        foreach (string frame in frames)
            placed.Add(Place(pictures.Frame(frame), default));
        return new PicturedPlane(placed.MoveToImmutable());
    }

    /// <summary>
    /// A page showing the menus' pictures named in <paramref name="pictures"/>, each with its origin on its page pixel, in
    /// drawing order (#1467): a field's picture lands where the game shows that field.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed menu pictures hold no such frame.</exception>
    internal PicturedPlane PlacedPictures(IEnumerable<PlacedFrame> pictures)
    {
        SpriteSheet sheet = Sheets.Get(SpriteSheetId.MenuPictures);
        return new PicturedPlane([.. Ensure.NotNull(pictures).Select(placed => Place(sheet.Frame(placed.Frame), placed.Origin))]);
    }

    /// <summary>
    /// <paramref name="picture"/> with its origin on page pixel <paramref name="origin"/>, in the standard colours the import
    /// pre-coloured it in under the menu's colours (#1512).
    /// </summary>
    private static PlacedPlanePicture Place(SpriteSheetFrame picture, Position16<PlanePixels> origin) =>
        PlacedPlanePicture.WithOriginAt(picture, origin, PictureShading.Standard);

    /// <summary>The solid colour the screen shows where nothing is drawn (#1512): file select's, or the world map's selected area's.</summary>
    public RgbColor Backdrop { get; set; }

    /// <summary>The menu's sprite entries over the installed menu sprite page.</summary>
    public ScreenSprites Objects { get; }

    /// <summary>What the screen paints with: its backdrop (#1512); its sprites are drawn by its sprite layer (#1514).</summary>
    internal FramePaint Paint() => new(Backdrop);

    /// <summary>The screen's sprite entries as one layer of sprites over its pages, each in the shading it names (#1512, #1514).</summary>
    internal SpritesLayer Sprites() => new(Objects.Draws);
}
