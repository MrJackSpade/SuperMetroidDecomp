using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Gameplay composed whole: the backdrop, then the room's layers and the actors in <see cref="Order"/> (#1514). It must be
/// a scene's first layer; the HUD, the effect overlay and the glows are the scene's later layers. The background's scroll
/// may change line by line on either axis, as a described shape (#1468); its scrolls are scrolls, not rows already offset.
/// </summary>
public sealed record GameplaySceneLayer : DisplayLayer
{
    /// <summary>
    /// The gameplay scene's layers from back to front (#1514): the actors behind the room, the background's and then the
    /// foreground's back tiles, the actors behind the front tiles, the background's and then the foreground's front tiles,
    /// and the actors in front.
    /// </summary>
    public static ImmutableArray<GameplayLayer> Order { get; } =
    [
        GameplayLayer.ActorsBehindRoom,
        GameplayLayer.BackgroundBackPart,
        GameplayLayer.ForegroundBackPart,
        GameplayLayer.ActorsBehindFrontParts,
        GameplayLayer.BackgroundFrontPart,
        GameplayLayer.ForegroundFrontPart,
        GameplayLayer.ActorsInFront,
    ];

    /// <summary>How the frame views the gameplay scene: scrolls, the layers shown and pixelation.</summary>
    public GameplaySceneView View { get; }
    /// <summary>The room's foreground and background, resolved from level data through the frame's scroll positions (#1346).</summary>
    public RoomBackgrounds Rooms { get; }
    /// <summary>How the background's scroll changes line by line; an axis no effect drives uses <see cref="GameplaySceneView.BackgroundScroll"/> on every line.</summary>
    public BackgroundLineScrolls BackgroundLineScrolls { get; }
    /// <summary>
    /// The bands of the gameplay screen that show their own choice of layers (#1514), top to bottom; the lines no band
    /// covers show <see cref="GameplaySceneView.ShownLayers"/>.
    /// </summary>
    public ImmutableArray<ShownBand> Bands { get; }

    /// <summary>Captures the frame's view, its background's line scrolls and its bands; this layer is drawn before any blend or overlay.</summary>
    /// <param name="view">How the frame views the gameplay scene.</param>
    /// <param name="rooms">The room's foreground and background; the background shows its loaded map when the room gives it none.</param>
    /// <param name="backgroundLineScrolls">How the background's scroll changes line by line, if a room effect moves it.</param>
    /// <param name="bands">The bands that show their own layers, top to bottom, or none.</param>
    /// <exception cref="ArgumentException">A band reaches into the HUD, is empty, or overlaps or lies above the band before it.</exception>
    public GameplaySceneLayer(GameplaySceneView view, RoomBackgrounds rooms,
        BackgroundLineScrolls backgroundLineScrolls = default, ImmutableArray<ShownBand> bands = default)
    {
        ImmutableArray<ShownBand> shown = bands.IsDefault ? [] : bands;
        int below = DisplayScreen.HudHeight;
        foreach (ShownBand band in shown)
        {
            if (band.Lines.FirstLine < below || band.Lines.EndLine <= band.Lines.FirstLine)
                throw new ArgumentException("A gameplay scene's bands lie below the HUD, top to bottom, each holding a line of its own.", nameof(bands));
            below = band.Lines.EndLine;
        }
        View = view;
        Rooms = Ensure.NotNull(rooms);
        BackgroundLineScrolls = backgroundLineScrolls;
        Bands = shown;
    }

    /// <summary>The layers screen line <paramref name="line"/> shows: its band's, or the view's on a line no band covers.</summary>
    /// <param name="line">A screen line.</param>
    /// <returns>The room's layers and the actors the line shows.</returns>
    public ScreenLayers ShownLayersAt(int line)
    {
        foreach (ShownBand band in Bands)
        {
            if (line >= band.Lines.FirstLine && line < band.Lines.EndLine)
                return band.Shows;
        }
        return View.ShownLayers;
    }
}
