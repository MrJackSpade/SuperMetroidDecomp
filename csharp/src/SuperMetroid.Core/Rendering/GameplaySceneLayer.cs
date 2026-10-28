using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Gameplay composed whole: the backdrop, the HUD, the room's layers and the sprites. It must be a scene's first layer;
/// later layers may blend and draw over it. The background's per-line scrolls cover
/// screen lines 32..223 and are scrolls, not rows already offset.
/// </summary>
public sealed record GameplaySceneLayer : DisplayLayer
{
    /// <summary>The layers each gameplay line shows, where a room effect changes them line by line.</summary>
    private readonly ScreenLayers[] mainScreenLayersByLine;
    /// <summary>How the frame views the gameplay scene: scrolls, the layers shown and pixelation.</summary>
    public GameplaySceneView View { get; }
    /// <summary>The room's foreground and background, resolved from level data through the frame's scroll positions (#1346).</summary>
    public RoomBackgrounds Rooms { get; }
    /// <summary>The background's per-line scrolls for screen lines 32..223; an axis no effect drives uses <see cref="GameplaySceneView.BackgroundScroll"/> on every line.</summary>
    public BackgroundLineScrolls BackgroundLineScrolls { get; }
    /// <summary>The layers screen lines 32..223 show; when given they replace the frame's layers, leaving the HUD as it is.</summary>
    public ReadOnlySpan<ScreenLayers> LayersShownByLine => mainScreenLayersByLine;

    /// <summary>Captures the frame's view and copies its optional per-line tables; this layer is drawn before any blend or overlay.</summary>
    /// <param name="view">How the frame views the gameplay scene, with a background map of 32 or 64 cells on each axis.</param>
    /// <param name="rooms">The room's foreground and background; the background shows its loaded map when the room gives it none.</param>
    /// <param name="bg2LineScrolls">Optional per-line background scrolls for screen lines 32..223.</param>
    /// <param name="mainScreenLayersByLine">Empty or exactly 192 per-line layer sets.</param>
    /// <exception cref="ArgumentException">The background's map size is unsupported, or per-line layers are given for other than exactly 192 lines.</exception>
    public GameplaySceneLayer(GameplaySceneView view, RoomBackgrounds rooms,
        BackgroundLineScrolls bg2LineScrolls = default, ReadOnlySpan<ScreenLayers> mainScreenLayersByLine = default)
    {
        if (view.BackgroundMapSize.X is not (32 or 64) || view.BackgroundMapSize.Y is not (32 or 64)
            || view.BackgroundMapSize.X * view.BackgroundMapSize.Y is not (1024 or 2048 or 4096))
            throw new ArgumentException("The gameplay background's map is 32 or 64 tiles on each axis.", nameof(view));
        if (!mainScreenLayersByLine.IsEmpty && mainScreenLayersByLine.Length != BackgroundLineScrolls.GameplayLines)
            throw new ArgumentException("Per-line layers cover exactly 192 gameplay lines.");
        View = view;
        Rooms = Ensure.NotNull(rooms);
        BackgroundLineScrolls = bg2LineScrolls;
        this.mainScreenLayersByLine = mainScreenLayersByLine.ToArray();
    }

    /// <summary>The background's scroll on gameplay line <paramref name="line"/> (0..191).</summary>
    public Position16<BackgroundPixels> BackgroundScrollAt(int line) => BackgroundLineScrolls.At(line, View.BackgroundScroll);
}
