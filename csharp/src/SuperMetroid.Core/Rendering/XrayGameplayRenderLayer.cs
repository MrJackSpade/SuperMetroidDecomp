using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Combinable CGADSUB ($2131) source-enable and arithmetic bits.</summary>
[Flags]
public enum SnesColorMathControl : byte
{
    None = 0,
    Bg1 = 1,
    Bg2 = 2,
    Bg3 = 4,
    Bg4 = 8,
    Obj = 16,
    Backdrop = 32,
    Half = 64,
    Subtract = 128,
}

/// <summary>
/// X-ray's Mode-1 window composition, retaining source identity until color math.
/// Gameplay contains either the original BG2 registers or the reveal-map registers;
/// memory belongs to the containing scene. This operation also owns the unchanged HUD.
/// </summary>
public sealed record XrayGameplayRenderLayer : RenderLayer
{
    private readonly XrayWindowLine[] lines;
    public OrdinaryGameplayRenderLayer Gameplay { get; }
    public ReadOnlySpan<XrayWindowLine> Lines => lines;
    public bool RevealBlocks { get; }
    public SnesColorMathControl ColorMath { get; }
    public bool AddSubscreen { get; }
    public byte FixedRed { get; }
    public byte FixedGreen { get; }
    public byte FixedBlue { get; }
    public Bg2BppColorMathRenderLayer? Subscreen { get; }
    /// <summary>Select the gameplay BG2 plane as the color-math operand instead of a BG3 plane.</summary>
    public bool SubscreenUsesBg2 { get; }

    public XrayGameplayRenderLayer(OrdinaryGameplayRenderLayer gameplay, ReadOnlySpan<XrayWindowLine> lines,
        bool revealBlocks, SnesColorMathControl colorMath, bool addSubscreen,
        byte fixedRed, byte fixedGreen, byte fixedBlue, Bg2BppColorMathRenderLayer? subscreen = null,
        bool subscreenUsesBg2 = false)
    {
        ArgumentNullException.ThrowIfNull(gameplay);
        if (subscreenUsesBg2 && (subscreen is not null || !addSubscreen))
            throw new ArgumentException("BG2 subscreen requires subscreen arithmetic and cannot also select BG3.", nameof(subscreenUsesBg2));
        if (lines.Length != SnesPpuLayout.ScreenHeightPixels)
            throw new ArgumentException("X-ray requires one interval for each physical scanline.", nameof(lines));
        if (fixedRed > 31 || fixedGreen > 31 || fixedBlue > 31)
            throw new ArgumentOutOfRangeException(nameof(fixedRed), "COLDATA components must be five-bit values.");
        Gameplay = gameplay;
        this.lines = lines.ToArray();
        RevealBlocks = revealBlocks;
        ColorMath = colorMath;
        AddSubscreen = addSubscreen;
        FixedRed = fixedRed;
        FixedGreen = fixedGreen;
        FixedBlue = fixedBlue;
        Subscreen = subscreen;
        SubscreenUsesBg2 = subscreenUsesBg2;
    }
}
