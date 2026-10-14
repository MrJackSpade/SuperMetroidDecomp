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
    /// <summary>Five-bit fixed-color red component.</summary>
    public byte FixedRed { get; }
    /// <summary>Five-bit fixed-color green component.</summary>
    public byte FixedGreen { get; }
    /// <summary>Five-bit fixed-color blue component.</summary>
    public byte FixedBlue { get; }
    /// <summary>Optional captured 2-bpp BG3-style subscreen plane.</summary>
    public Bg2BppColorMathRenderLayer? Subscreen { get; }
    /// <summary>Admit gameplay BG2 to the subscreen, competing with any captured BG3 by Mode-1 priority.</summary>
    public bool SubscreenUsesBg2 { get; }

    /// <summary>Creates a gameplay composition with scanline-windowed SNES color arithmetic.</summary>
    /// <param name="gameplay">The ordinary gameplay source planes and sprites.</param>
    /// <param name="lines">Exactly one color-window interval per physical scanline.</param>
    /// <param name="revealBlocks">Whether to substitute eligible level blocks with their X-ray reveal characters.</param>
    /// <param name="math">The colour-math registers one composition uses: CGADSUB's sources and operation, CGWSEL's subscreen-or-fixed operand select, and COLDATA's five-bit fixed colour (#633).</param>
    /// <param name="subscreen">Optional captured 2-bpp plane competing on the subscreen.</param>
    /// <param name="subscreenUsesBg2">Whether gameplay BG2 also competes on the subscreen.</param>
    public GameplayColorMathRenderLayer(OrdinaryGameplayRenderLayer gameplay, ReadOnlySpan<XrayWindowLine> lines, bool revealBlocks, ColorMathRegisters math, Bg2BppColorMathRenderLayer? subscreen = null, bool subscreenUsesBg2 = false)
    {
        Ensure.NotNull(gameplay);
        if (subscreenUsesBg2 && !math.AddSubscreen)
            throw new ArgumentException("BG2 subscreen requires subscreen arithmetic.", nameof(subscreenUsesBg2));
        Ensure.LengthEqual(lines, SnesPpuLayout.ScreenHeightPixels);
        if (math.FixedRed > 31 || math.FixedGreen > 31 || math.FixedBlue > 31)
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
