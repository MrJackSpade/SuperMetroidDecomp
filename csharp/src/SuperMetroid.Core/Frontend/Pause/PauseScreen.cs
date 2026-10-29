using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
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
        HudTiles = null;
    }

    /// <summary>What the map, equipment and frame planes show under the pause palette (#1392).</summary>
    [field: NonSerialized]
    [AllowNull]
    internal ScreenLayerPictures PlaneTiles { get => field ??= PauseScreenTiles.Planes(Sheets); private set; }

    /// <summary>What the HUD plane shows under the pause palette (#1392).</summary>
    [field: NonSerialized]
    [AllowNull]
    internal ScreenLayerPictures HudTiles { get => field ??= PauseScreenTiles.Hud(Sheets); private set; }

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
