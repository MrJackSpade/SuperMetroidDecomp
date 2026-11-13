using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// The pause screen the pause setup builds (#1358): the installed artwork and pictures it draws, each in the colours
/// the import pre-coloured it in (#1512), its backdrop and its sprite entries. It publishes these; the capture composes
/// them, so pause composes no screen of its own.
/// </summary>
internal sealed class PauseScreen
{
    /// <summary>Installs the map, pause interface, HUD and menu sprite artwork, and the sheets its backgrounds draw from.</summary>
    public PauseScreen(AreaMapPresentation artwork, InstalledSpriteSheets? sheets)
    {
        Backdrop = Ensure.NotNull(artwork).Backdrops.Pause;
        Objects = new ScreenSprites();
        BindScreenTiles(sheets);
    }

    /// <summary>The installed sheets the screen's background tiles come from (#1392).</summary>
    [AllowNull]
    private InstalledSpriteSheets Sheets { get => field ?? throw new InvalidOperationException("The pause screen's tiles are bound when the game is bound to its installed content."); set; }

    /// <summary>Rebinds the sheets the screen's background tiles come from (#1392).</summary>
    public void BindScreenTiles(InstalledSpriteSheets? sheets)
    {
        Sheets = sheets ?? throw new InvalidOperationException("Pause requires the installed sprite sheets.");
        HudPictures = null;
        pictured = null;
    }

    /// <summary>The pause screen's pictures (#1467): the area maps' cells, each area's backdrop, the button rows and labels.</summary>
    internal SpriteSheet Pictures => Sheets.Get(SpriteSheetId.PausePictures);

    /// <summary>
    /// The frame <paramref name="area"/>'s pause screen shows under <paramref name="highlight"/> (#1467): the area's
    /// backdrop, the button rows over its bottom rows and each label in the shade the highlight shows it in; a picture the
    /// installed pictures hold as showing nothing is not placed (#1513).
    /// </summary>
    /// <exception cref="InvalidDataException">The installed pause pictures hold no such frame.</exception>
    internal PicturedPlane Frame(AreaId area, PauseButtonHighlight highlight)
    {
        SpriteSheet pictures = Pictures;
        var frames = new List<string> { PauseFramePictures.BackdropFrame(area), PauseFramePictures.ButtonRowsFrame };
        foreach (PauseButtonLabel label in Enum.GetValues<PauseButtonLabel>())
            frames.Add(PauseFramePictures.ButtonFrame(label, PauseFramePictures.ShadeOf(highlight, label)));
        var placed = ImmutableArray.CreateBuilder<PlacedPlanePicture>();
        foreach (string frame in frames)
        {
            if (!pictures.ShowsNothing(frame))
                placed.Add(AtCorner(pictures.Frame(frame)));
        }
        return new PicturedPlane(placed.ToImmutable());
    }

    /// <summary>
    /// <paramref name="picture"/> placed where the pause screen shows it, in <paramref name="shading"/> (#1512): with its
    /// origin on the page's corner, which lies <see cref="PageRowsHiddenAboveScreen"/> above the screen's top, so the
    /// screen shows each page from the row below its first (#1555).
    /// </summary>
    private static PlacedPlanePicture AtCorner(SpriteSheetFrame picture, PictureShading shading)
    {
        PlacedPlanePicture atCorner = PlacedPlanePicture.WithOriginAt(picture, default, shading);
        return atCorner with { Corner = atCorner.Corner.WithY(atCorner.Corner.Y - PageRowsHiddenAboveScreen) };
    }

    /// <summary>
    /// The pixel rows of each pause page, frame and equipment page alike, that the screen hides above its top (#1705):
    /// the screen shows each page from the row below its first, while the map's marker and icons are placed from the
    /// page's first row. The pause screen keeps this one-row offset between its pages and its sprites, as gameplay keeps
    /// <see cref="ScreenGeometry.LayerRowsHiddenAboveScreen"/>, so every marker shows where the original shows it.
    /// </summary>
    internal const int PageRowsHiddenAboveScreen = 1;

    /// <summary><paramref name="picture"/> placed on the frame's corner in the standard colours the import pre-coloured it in under the pause colours (#1512).</summary>
    private static PlacedPlanePicture AtCorner(SpriteSheetFrame picture) => AtCorner(picture, PictureShading.Standard);

    /// <summary>
    /// The equipment page <paramref name="page"/> shows (#1467): the template's picture and, beside it, the picture of each
    /// label, the wireframe, the reserve display and the arrow in the state the page shows it in, the
    /// arrow in <paramref name="arrowShading"/> (#1512). A picture the installed pictures hold as showing nothing, such as
    /// a blank label the page's data leaves clear, is not placed (#1513).
    /// </summary>
    /// <exception cref="InvalidDataException">The installed pause pictures hold no such frame.</exception>
    internal PicturedPlane EquipmentPage(ShownEquipmentPage page, PictureShading arrowShading)
    {
        SpriteSheet pictures = Pictures;
        string arrow = PauseEquipmentPictures.ReserveArrowFrame(page.Arrow);
        return new PicturedPlane([.. PauseEquipmentPictures.Frames(page).Where(frame => !pictures.ShowsNothing(frame))
            .Select(frame => AtCorner(pictures.Frame(frame), frame == arrow ? arrowShading : PictureShading.Standard))]);
    }

    /// <summary>
    /// The HUD's images under the pause palette (#1466): a lookup of the bound sheet's frames, remade from the sheet when
    /// absent, not game state.
    /// </summary>
    [field: NonSerialized]
    [AllowNull]
    private HudPictures HudPictures { get => field ??= new HudPictures(Sheets.Get(SpriteSheetId.PauseHud)); set; }

    /// <summary>
    /// The gameplay HUD pause shows (#1351, #1466, #1626): the HUD gameplay displayed at its last accepted frame, whole.
    /// </summary>
    public ShownHud Hud { get; private set; } = ShownHud.Nothing;

    /// <summary>The shown HUD's pictures last resolved, while neither the HUD nor the sheets have changed.</summary>
    private PicturedHud? pictured;

    /// <summary>Shows <paramref name="gameplay"/>, the HUD gameplay displayed at its last accepted frame.</summary>
    internal void ShowHud(ShownHud gameplay)
    {
        Hud = Ensure.NotNull(gameplay);
        pictured = null;
    }

    /// <summary>
    /// The HUD pause shows (#1466): the shown HUD's images under the pause palette, drawn on the HUD's own layer. The
    /// cleared cells the pause leaves beneath and around the HUD show nothing, so pause draws none.
    /// </summary>
    internal PicturedHud ShownHudPictures() => pictured ??= HudPictures.Pictured(Hud, PictureShading.Standard);

    /// <summary>The solid colour the pause screen shows where nothing is drawn (#1512).</summary>
    public RgbColor Backdrop { get; private set; }

    /// <summary>The pause sprite entries over the installed menu sprite page.</summary>
    public ScreenSprites Objects { get; }

    /// <summary>Rebinds the backdrop the installed artwork names, keeping the sprite entries.</summary>
    public void BindArtwork(AreaMapPresentation artwork) => Backdrop = Ensure.NotNull(artwork).Backdrops.Pause;

    /// <summary>What the screen paints with: its backdrop (#1512); its sprites are drawn by its sprite layer (#1514).</summary>
    internal FramePaint Paint() => new(Backdrop);

    /// <summary>The screen's sprite entries as one layer of sprites, each in the shading it names (#1512, #1514).</summary>
    internal SpritesLayer Sprites() => new(Objects.Draws);
}
