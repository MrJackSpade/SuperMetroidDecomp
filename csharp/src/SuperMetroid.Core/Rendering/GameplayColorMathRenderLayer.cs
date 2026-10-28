using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Mode-1 composition retaining the winning layer until it blends. X-ray, Phantoon, Fireflea and spores share this
/// operation; an all-empty window blends everywhere without revealing blocks. Memory belongs to the containing scene,
/// and the fused operation also owns the unchanged HUD.
/// </summary>
public sealed record GameplayColorMathRenderLayer : RenderLayer
{
    /// <summary>Owned scanline windows, copied from the caller for stable rendering.</summary>
    private readonly XrayWindowLine[] lines;
    /// <summary>Ordinary gameplay planes and OAM composed before color math.</summary>
    public OrdinaryGameplayRenderLayer Gameplay { get; }
    /// <summary>One color-window interval for each of the 224 physical scanlines.</summary>
    public ReadOnlySpan<XrayWindowLine> Lines => lines;
    /// <summary>Whether X-ray-reveal block characters replace eligible level tiles.</summary>
    public bool RevealBlocks { get; }
    /// <summary>Which layers blend, how, and with what.</summary>
    public SceneBlend Blend { get; }
    /// <summary>Optional captured 2-bpp BG3-style subscreen plane.</summary>
    public FxPlaneBlendRenderLayer? Subscreen { get; }
    /// <summary>Admit gameplay BG2 to the subscreen, competing with any captured BG3 by Mode-1 priority.</summary>
    public bool SubscreenUsesBg2 { get; }

    /// <summary>Creates a gameplay composition with scanline-windowed colour blending.</summary>
    /// <param name="gameplay">The ordinary gameplay source planes and sprites.</param>
    /// <param name="lines">Exactly one color-window interval per physical scanline.</param>
    /// <param name="revealBlocks">Whether to substitute eligible level blocks with their X-ray reveal characters.</param>
    /// <param name="blend">Which layers blend, how, and with what.</param>
    /// <param name="subscreen">Optional captured 2-bpp plane competing on the subscreen.</param>
    /// <param name="subscreenUsesBg2">Whether gameplay BG2 also competes on the subscreen.</param>
    [IndependentFlag("independent PPU facts of the captured frame: x-ray block reveal, and BG2 in the subscreen")]
    public GameplayColorMathRenderLayer(OrdinaryGameplayRenderLayer gameplay, ReadOnlySpan<XrayWindowLine> lines, bool revealBlocks, SceneBlend blend, FxPlaneBlendRenderLayer? subscreen = null, bool subscreenUsesBg2 = false)
    {
        Ensure.NotNull(gameplay);
        if (subscreenUsesBg2 && !blend.BlendsWithSubscreen)
            throw new ArgumentException("BG2 subscreen requires subscreen arithmetic.", nameof(subscreenUsesBg2));
        Ensure.LengthEqual(lines, ScreenGeometry.ScreenHeightPixels);
        Gameplay = gameplay;
        this.lines = lines.ToArray();
        RevealBlocks = revealBlocks;
        Blend = blend;
        Subscreen = subscreen;
        SubscreenUsesBg2 = subscreenUsesBg2;
    }
}
