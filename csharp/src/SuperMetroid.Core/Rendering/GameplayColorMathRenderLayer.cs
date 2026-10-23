namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Mode-1 composition retaining the winning BG/OBJ source until CGADSUB color math.
/// X-ray, Phantoon, Fireflea and spores share this native operation; an all-empty
/// window applies math everywhere without revealing blocks. Memory belongs to the
/// containing scene, and the fused operation also owns the unchanged HUD.
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
    /// <summary>Emulated CGADSUB source-enable and arithmetic bits.</summary>
    public SnesColorMathControl ColorMath { get; }
    /// <summary>Whether the subscreen supplies the arithmetic operand instead of fixed color.</summary>
    public bool AddSubscreen { get; }
    /// <summary>Fixed-color red intensity, 0 through <see cref="Bgr555.MaxChannel"/>.</summary>
    public int FixedRed { get; }
    /// <summary>Fixed-color green intensity, 0 through <see cref="Bgr555.MaxChannel"/>.</summary>
    public int FixedGreen { get; }
    /// <summary>Fixed-color blue intensity, 0 through <see cref="Bgr555.MaxChannel"/>.</summary>
    public int FixedBlue { get; }
    /// <summary>Optional captured 2-bpp BG3-style subscreen plane.</summary>
    public FxPlaneBlendRenderLayer? Subscreen { get; }
    /// <summary>Admit gameplay BG2 to the subscreen, competing with any captured BG3 by Mode-1 priority.</summary>
    public bool SubscreenUsesBg2 { get; }

    /// <summary>Creates a gameplay composition with scanline-windowed SNES color arithmetic.</summary>
    /// <param name="gameplay">The ordinary gameplay source planes and sprites.</param>
    /// <param name="lines">Exactly one color-window interval per physical scanline.</param>
    /// <param name="revealBlocks">Whether to substitute eligible level blocks with their X-ray reveal characters.</param>
    /// <param name="math">The colour-math registers one composition uses: CGADSUB's sources and operation, CGWSEL's subscreen-or-fixed operand select, and COLDATA's five-bit fixed colour (#633).</param>
    /// <param name="subscreen">Optional captured 2-bpp plane competing on the subscreen.</param>
    /// <param name="subscreenUsesBg2">Whether gameplay BG2 also competes on the subscreen.</param>
    [IndependentFlag("independent PPU facts of the captured frame: x-ray block reveal, and BG2 in the subscreen")]
    public GameplayColorMathRenderLayer(OrdinaryGameplayRenderLayer gameplay, ReadOnlySpan<XrayWindowLine> lines, bool revealBlocks, ColorMathRegisters math, FxPlaneBlendRenderLayer? subscreen = null, bool subscreenUsesBg2 = false)
    {
        Ensure.NotNull(gameplay);
        if (subscreenUsesBg2 && !math.AddSubscreen)
            throw new ArgumentException("BG2 subscreen requires subscreen arithmetic.", nameof(subscreenUsesBg2));
        Ensure.LengthEqual(lines, SnesPpuLayout.ScreenHeightPixels);
        if (math.FixedRed is < 0 or > Bgr555.MaxChannel || math.FixedGreen is < 0 or > Bgr555.MaxChannel ||
            math.FixedBlue is < 0 or > Bgr555.MaxChannel)
            throw new ArgumentOutOfRangeException(nameof(math.FixedRed), "COLDATA components must be five-bit values.");
        Gameplay = gameplay;
        this.lines = lines.ToArray();
        RevealBlocks = revealBlocks;
        ColorMath = math.ColorMath;
        AddSubscreen = math.AddSubscreen;
        FixedRed = math.FixedRed;
        FixedGreen = math.FixedGreen;
        FixedBlue = math.FixedBlue;
        Subscreen = subscreen;
        SubscreenUsesBg2 = subscreenUsesBg2;
    }
}
