using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// The pause screen <c>GameState_13</c> sets up (#1358): the installed artwork and pictures it draws, each in the colours
/// the import pre-coloured it in (#1512), its backdrop and its sprite entries. It publishes these; the capture composes
/// them, so pause owns no display memory.
/// </summary>
internal sealed class PauseScreen
{
    /// <summary>Installs the map, pause interface, HUD and menu sprite artwork, and the sheets its backgrounds draw from.</summary>
    public PauseScreen(AreaMapPresentation artwork, InstalledSpriteSheets? sheets)
    {
        Artwork = Ensure.NotNull(artwork);
        Backdrop = artwork.Backdrops.Pause;
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
        return new PicturedPlane(MenuScreen.PageSize, placed.ToImmutable());
    }

    /// <summary>
    /// <paramref name="picture"/> placed with its origin on the frame's corner, where the game's page shows it, in
    /// <paramref name="shading"/> (#1512).
    /// </summary>
    private static PlacedPlanePicture AtCorner(SpriteSheetFrame picture, PictureShading shading) =>
        PlacedPlanePicture.AtCell(picture, default, MenuScreen.PageSize, shading);

    /// <summary><paramref name="picture"/> placed on the frame's corner in the standard colours the import pre-coloured it in under the pause colours (#1512).</summary>
    private static PlacedPlanePicture AtCorner(SpriteSheetFrame picture) => AtCorner(picture, PictureShading.Standard);

    /// <summary>
    /// The equipment page <paramref name="page"/> shows (#1467): the template's picture and, beside it, the picture of each
    /// label, the overrun tail, the wireframe, the reserve display and the arrow in the state the page shows it in, the
    /// arrow in <paramref name="arrowShading"/> (#1512). A picture the installed pictures hold as showing nothing, such as
    /// a blank label whose cells the page's data leaves clear, is not placed (#1513).
    /// </summary>
    /// <exception cref="InvalidDataException">The installed pause pictures hold no such frame.</exception>
    internal PicturedPlane EquipmentPage(ShownEquipmentPage page, PictureShading arrowShading)
    {
        SpriteSheet pictures = Pictures;
        string arrow = PauseEquipmentPictures.ReserveArrowFrame(page.Arrow);
        return new PicturedPlane(MenuScreen.PageSize, [.. PauseEquipmentPictures.Frames(page).Where(frame => !pictures.ShowsNothing(frame))
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
    /// The gameplay HUD pause keeps resident (#1351, #1466): pause never replaces the HUD gameplay showed, and mirrors the
    /// rows gameplay draws while it opens and closes.
    /// </summary>
    public ShownHud Hud { get; private set; } = ShownHud.Nothing;

    /// <summary>The retained HUD's pictures last resolved, while neither the HUD nor the sheets have changed.</summary>
    private PicturedHud? pictured;

    /// <summary>Keeps <paramref name="gameplay"/>, the HUD gameplay displayed at its last frame, resident.</summary>
    internal void RetainHud(ShownHud gameplay)
    {
        Hud = Ensure.NotNull(gameplay);
        pictured = null;
    }

    /// <summary>Mirrors the rows <paramref name="gameplay"/> draws, keeping the static top row pause retained.</summary>
    internal void MirrorAcceptedHud(ShownHud gameplay)
    {
        Hud = Hud.WithDrawnRowsOf(gameplay);
        pictured = null;
    }

    /// <summary>
    /// The HUD pause shows (#1466): the retained HUD's images under the pause palette, drawn on the HUD's own layer. The
    /// cleared cells <c>QueueClearingOfFxTilemap</c> leaves beneath and around the HUD show nothing, so pause draws none.
    /// </summary>
    internal PicturedHud RetainedHudPictures() => pictured ??= HudPictures.Pictured(Hud, PictureShading.Standard);

    /// <summary>The installed artwork the screen draws.</summary>
    [AllowNull]
    public AreaMapPresentation Artwork
    {
        get => field ?? throw new InvalidOperationException("The pause screen's artwork is bound when the game is bound to its installed content.");
        private set;
    }

    /// <summary>The solid colour the pause screen shows where nothing is drawn (#1512).</summary>
    public RgbColor Backdrop { get; private set; }

    /// <summary>The pause sprite entries over the installed menu OBJ page.</summary>
    public ScreenSprites Objects { get; }

    /// <summary>Rebinds the installed artwork and the backdrop it names, keeping the sprite entries.</summary>
    public void BindArtwork(AreaMapPresentation artwork)
    {
        Artwork = Ensure.NotNull(artwork);
        Backdrop = artwork.Backdrops.Pause;
    }

    /// <summary>What the screen paints with: its backdrop and sprite entries, each sprite in the shading it names (#1512).</summary>
    internal FramePaint Paint() => new(Backdrop, Objects.Draws);
}
