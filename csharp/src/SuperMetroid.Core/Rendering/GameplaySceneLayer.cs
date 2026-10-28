using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Fused backdrop/HUD/Mode-1/OBJ composition. This must be the first operation;
/// subsequent operations may apply color math and overlays. Per-line BG2 scrolls cover
/// physical lines 32..223 and are scrolls, not pre-offset source Y.
/// </summary>
public sealed record GameplaySceneLayer : DisplayLayer
{
    /// <summary>The layers each gameplay line shows, where a room effect changes them line by line.</summary>
    private readonly ScreenLayers[] mainScreenLayersByLine;
    /// <summary>How the frame views the gameplay scene: scrolls, layers, windows and mosaic.</summary>
    public GameplaySceneView View { get; }
    /// <summary>The room backgrounds BG1 and BG2 show, resolved from level data through the frame's scroll positions (#1346).</summary>
    public RoomBackgrounds Rooms { get; }
    /// <summary>BG2's per-line scrolls for physical lines 32..223; an axis no effect drives uses <see cref="GameplaySceneView.BackgroundScroll"/> on every line.</summary>
    public BackgroundLineScrolls BackgroundLineScrolls { get; }
    /// <summary>The layers physical lines 32..223 show; nonempty entries replace the gameplay layers, not the separate window mask or fixed HUD composition.</summary>
    public ReadOnlySpan<ScreenLayers> MainScreenLayersByLine => mainScreenLayersByLine;

    /// <summary>Captures the frame's view and copies optional per-line tables for the first backdrop/HUD/background/sprite operation; its layer composition precedes later color-math and overlay operations.</summary>
    /// <param name="view">How the frame views the gameplay scene, with a 32- or 64-cell BG2 tilemap on each axis.</param>
    /// <param name="rooms">The room backgrounds BG1 and BG2 show; BG2 shows its loaded tilemap when it has no room plane.</param>
    /// <param name="bg2LineScrolls">Optional per-line BG2 scrolls for physical lines 32..223.</param>
    /// <param name="mainScreenLayersByLine">Empty or exactly 192 per-line layer sets.</param>
    /// <exception cref="ArgumentException">BG2 dimensions are unsupported or a nonempty TM table does not contain exactly 192 values.</exception>
    public GameplaySceneLayer(GameplaySceneView view, RoomBackgrounds rooms,
        BackgroundLineScrolls bg2LineScrolls = default, ReadOnlySpan<ScreenLayers> mainScreenLayersByLine = default)
    {
        if (view.BackgroundMapSize.X is not (32 or 64) || view.BackgroundMapSize.Y is not (32 or 64)
            || view.BackgroundMapSize.X * view.BackgroundMapSize.Y is not (1024 or 2048 or 4096))
            throw new ArgumentException("Gameplay BG2 requires 32 or 64 tiles on each axis.", nameof(view));
        if (!mainScreenLayersByLine.IsEmpty && mainScreenLayersByLine.Length != BackgroundLineScrolls.GameplayLines)
            throw new ArgumentException("Per-line layers cover exactly 192 gameplay lines.");
        View = view;
        Rooms = Ensure.NotNull(rooms);
        BackgroundLineScrolls = bg2LineScrolls;
        this.mainScreenLayersByLine = mainScreenLayersByLine.ToArray();
    }

    /// <summary>BG2's scroll on gameplay line <paramref name="line"/> (0..191).</summary>
    public Position16<BackgroundPixels> BackgroundScrollAt(int line) => BackgroundLineScrolls.At(line, View.BackgroundScroll);
}
