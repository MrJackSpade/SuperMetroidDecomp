namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Gameplay with a blend limited by a window, the frontmost layer at each pixel blending as <see cref="Blend"/> says.
/// X-ray, Phantoon, Fireflea and spores share it; outside the window the blend applies, and a closed window blends
/// everywhere without revealing blocks. The paint belongs to the containing scene, and this layer also draws the
/// unchanged HUD.
/// </summary>
public sealed record GameplayBlendLayer : DisplayLayer
{
    /// <summary>The gameplay layers and sprites, composed before the blend.</summary>
    public GameplaySceneLayer Gameplay { get; }
    /// <summary>The window the X-ray scope's cone lights (#1468): inside it the room shows unblended, revealing blocks when <see cref="RevealBlocks"/> says.</summary>
    public ScreenWindow Window { get; }
    /// <summary>Whether X-ray-reveal block characters replace eligible level tiles.</summary>
    public bool RevealBlocks { get; }
    /// <summary>Which layers blend, how, and with what.</summary>
    public SceneBlend Blend { get; }
    /// <summary>An optional four-colour overlay the frontmost layer blends with.</summary>
    public OverlayBlendLayer? BlendPlane { get; }
    /// <summary>Whether the gameplay background is also blended with, ordered with the optional overlay by their tile depths.</summary>
    public bool BlendsWithBackground { get; }

    /// <summary>Creates gameplay with a blend limited by a window.</summary>
    /// <param name="gameplay">The gameplay layers and sprites.</param>
    /// <param name="window">The window inside which the room shows unblended; <see cref="ScreenWindow.Closed"/> blends everywhere.</param>
    /// <param name="revealBlocks">Whether to substitute eligible level blocks with their X-ray reveal characters.</param>
    /// <param name="blend">Which layers blend, how, and with what.</param>
    /// <param name="subscreen">An optional four-colour overlay blended with.</param>
    /// <param name="subscreenUsesBg2">Whether the gameplay background is also blended with.</param>
    [IndependentFlag("independent facts of the frame: X-ray block reveal, and blending with the background")]
    public GameplayBlendLayer(GameplaySceneLayer gameplay, ScreenWindow window, bool revealBlocks, SceneBlend blend, OverlayBlendLayer? subscreen = null, bool subscreenUsesBg2 = false)
    {
        Ensure.NotNull(gameplay);
        Ensure.NotNull(window);
        if (subscreenUsesBg2 && !blend.BlendsWithLayersBehind)
            throw new ArgumentException("Blending with the background requires blending with the layers behind.", nameof(subscreenUsesBg2));
        Gameplay = gameplay;
        Window = window;
        RevealBlocks = revealBlocks;
        Blend = blend;
        BlendPlane = subscreen;
        BlendsWithBackground = subscreenUsesBg2;
    }
}
