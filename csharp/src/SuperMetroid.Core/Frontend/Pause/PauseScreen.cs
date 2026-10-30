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
/// The pause screen <c>GameState_13</c> sets up (#1358): the installed artwork it draws, its own tilemaps and palette,
/// and its sprite entries. It publishes these; the capture composes them, so pause owns no PPU memory.
/// </summary>
internal sealed class PauseScreen
{
    /// <summary>Installs the map, pause interface, HUD and menu sprite artwork, and the sheets its backgrounds draw from.</summary>
    public PauseScreen(AreaMapPresentation artwork, InstalledSpriteSheets? sheets)
    {
        Artwork = Ensure.NotNull(artwork);
        Objects = new ScreenSprites();
        BindScreenTiles(sheets);
    }

    /// <summary>The installed sheets the screen's background tiles come from (#1392).</summary>
    [field: NonSerialized]
    [AllowNull]
    private InstalledSpriteSheets Sheets { get => field ?? throw new InvalidOperationException("The pause screen's tiles are rebound after debugger restoration."); set; }

    /// <summary>Rebinds the sheets the screen's background tiles come from (#1392).</summary>
    public void BindScreenTiles(InstalledSpriteSheets? sheets)
    {
        Sheets = sheets ?? throw new InvalidOperationException("Pause requires the installed sprite sheets.");
        PlaneTiles = null;
        HudPictures = null;
        pictured = null;
    }

    /// <summary>The pause screen's pictures (#1467): the area maps' cells, each area's backdrop, the button rows and labels.</summary>
    internal SpriteSheet Pictures => Sheets.Get(SpriteSheetId.PausePictures);

    /// <summary>
    /// The frame <paramref name="area"/>'s pause screen shows under <paramref name="highlight"/> (#1467): the area's
    /// backdrop, the button rows over its bottom rows and each label in the shade the highlight shows it in.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed pause pictures hold no such frame.</exception>
    internal PicturedPlane Frame(AreaId area, PauseButtonHighlight highlight)
    {
        SpriteSheet pictures = Pictures;
        PauseButtonLabel[] labels = Enum.GetValues<PauseButtonLabel>();
        var placed = ImmutableArray.CreateBuilder<PlacedPlanePicture>();
        placed.Add(AtCorner(pictures.Frame(PauseFramePictures.BackdropFrame(area))));
        placed.Add(AtCorner(pictures.Frame(PauseFramePictures.ButtonRowsFrame)));
        foreach (PauseButtonLabel label in labels)
            placed.Add(AtCorner(pictures.Frame(PauseFramePictures.ButtonFrame(label, PauseFramePictures.ShadeOf(highlight, label)))));
        return new PicturedPlane(MenuScreen.PageSize, placed.ToImmutable());
    }

    /// <summary><paramref name="picture"/> placed with its origin on the frame's corner, where the cartridge's page shows it.</summary>
    private static PlacedPlanePicture AtCorner(SpriteSheetFrame picture) =>
        PlacedPlanePicture.AtCell(picture, default, MenuScreen.PageSize);

    /// <summary>What the equipment plane shows under the pause palette (#1392).</summary>
    [field: NonSerialized]
    [AllowNull]
    internal ScreenLayerPictures PlaneTiles { get => field ??= PauseScreenTiles.Planes(Sheets); private set; }

    /// <summary>The HUD's images under the pause palette (#1466).</summary>
    [field: NonSerialized]
    [AllowNull]
    private HudPictures HudPictures { get => field ??= new HudPictures(Sheets.Get(SpriteSheetId.PauseHud)); set; }

    /// <summary>
    /// The gameplay HUD pause keeps resident (#1351, #1466): pause never replaces the HUD gameplay showed, and mirrors the
    /// rows gameplay draws while it opens and closes.
    /// </summary>
    public ShownHud Hud { get; private set; } = ShownHud.Nothing;

    /// <summary>The retained HUD's pictures last resolved, while neither the HUD nor the sheets have changed.</summary>
    [NonSerialized] private PicturedHud? pictured;

    /// <summary>Keeps <paramref name="gameplay"/>, the HUD gameplay displayed at its last NMI, resident.</summary>
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
    internal PicturedHud RetainedHudPictures() => pictured ??= HudPictures.Pictured(Hud);

    /// <summary>The installed artwork the screen draws.</summary>
    [field: NonSerialized]
    [AllowNull]
    public AreaMapPresentation Artwork
    {
        get => field ?? throw new InvalidOperationException("The pause screen's artwork is rebound after debugger restoration.");
        private set;
    }

    /// <summary>The pause tilemaps.</summary>
    public PauseTilemaps Tilemaps { get; } = new();

    /// <summary>The pause palette, with its live highlight and reserve-arrow colours.</summary>
    public ScreenPalette Palette { get; } = new();

    /// <summary>The pause sprite entries over the installed menu OBJ page.</summary>
    public ScreenSprites Objects { get; }

    /// <summary>Rebinds the installed artwork, keeping tilemaps, palette and sprite entries.</summary>
    public void BindArtwork(AreaMapPresentation artwork)
    {
        Artwork = Ensure.NotNull(artwork);
    }

    /// <summary>What the screen paints with: its palette and sprite entries.</summary>
    internal FramePaint Paint() => new(Palette.Shown(), Objects.Draws);
}
