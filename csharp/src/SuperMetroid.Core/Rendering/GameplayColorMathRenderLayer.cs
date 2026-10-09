using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Combinable CGADSUB ($2131) source-enable and arithmetic bits.</summary>
[Flags]
public enum SnesColorMathControl : byte
{
    /// <summary>Disables color arithmetic for every main-screen source.</summary>
    None = 0,
    /// <summary>Applies color arithmetic to BG1 pixels.</summary>
    Bg1 = 1,
    /// <summary>Applies color arithmetic to BG2 pixels.</summary>
    Bg2 = 2,
    /// <summary>Applies color arithmetic to BG3 pixels.</summary>
    Bg3 = 4,
    /// <summary>Applies color arithmetic to eligible OBJ pixels.</summary>
    Obj = 16,
    /// <summary>Applies color arithmetic when the backdrop wins the main screen.</summary>
    Backdrop = 32,
    /// <summary>Halves the color arithmetic result where SNES conditions permit.</summary>
    Half = 64,
    /// <summary>Subtracts the operand instead of adding it.</summary>
    Subtract = 128,
}

/// <summary>
/// Mode-1 composition retaining the winning BG/OBJ source until CGADSUB color math.
/// X-ray, Phantoon, Fireflea and spores share this native operation; an all-empty
/// window applies math everywhere without revealing blocks. Memory belongs to the
/// containing scene, and the fused operation also owns the unchanged HUD.
/// </summary>
public sealed record GameplayColorMathRenderLayer : RenderLayer
{
    /// <summary>Owned per-scanline reveal/color-math windows for the complete visible frame.</summary>
    private readonly XrayWindowLine[] lines;
    /// <summary>Gets the ordinary gameplay planes and OAM composed before color math.</summary>
    public OrdinaryGameplayRenderLayer Gameplay { get; }
    /// <summary>Gets one color-window interval for each of the 224 physical scanlines.</summary>
    public ReadOnlySpan<XrayWindowLine> Lines => lines;
    /// <summary>Gets whether X-ray-reveal block characters replace eligible level tiles.</summary>
    public bool RevealBlocks { get; }
    /// <summary>Gets the emulated CGADSUB source-enable and arithmetic bits.</summary>
    public SnesColorMathControl ColorMath { get; }
    /// <summary>Gets whether the subscreen supplies the arithmetic operand instead of fixed color.</summary>
    public bool AddSubscreen { get; }
    /// <summary>Gets the five-bit fixed-color red component.</summary>
    public byte FixedRed { get; }
    /// <summary>Gets the five-bit fixed-color green component.</summary>
    public byte FixedGreen { get; }
    /// <summary>Gets the five-bit fixed-color blue component.</summary>
    public byte FixedBlue { get; }
    /// <summary>Gets the optional captured 2-bpp BG3-style subscreen plane.</summary>
    public Bg2BppColorMathRenderLayer? Subscreen { get; }
    /// <summary>Admit gameplay BG2 to the subscreen, competing with any captured BG3 by Mode-1 priority.</summary>
    public bool SubscreenUsesBg2 { get; }

    /// <summary>Creates a gameplay composition with scanline-windowed SNES color arithmetic.</summary>
    /// <param name="gameplay">The ordinary gameplay source planes and sprites.</param>
    /// <param name="lines">Exactly one color-window interval per physical scanline.</param>
    /// <param name="revealBlocks">Whether to substitute eligible level blocks with their X-ray reveal characters.</param>
    /// <param name="colorMath">CGADSUB source and operation bits.</param>
    /// <param name="addSubscreen">Whether arithmetic uses the winning subscreen pixel instead of fixed color.</param>
    /// <param name="fixedRed">Five-bit fixed-color red component.</param>
    /// <param name="fixedGreen">Five-bit fixed-color green component.</param>
    /// <param name="fixedBlue">Five-bit fixed-color blue component.</param>
    /// <param name="subscreen">Optional captured 2-bpp plane competing on the subscreen.</param>
    /// <param name="subscreenUsesBg2">Whether gameplay BG2 also competes on the subscreen.</param>
    public GameplayColorMathRenderLayer(OrdinaryGameplayRenderLayer gameplay, ReadOnlySpan<XrayWindowLine> lines,
        bool revealBlocks, SnesColorMathControl colorMath, bool addSubscreen,
        byte fixedRed, byte fixedGreen, byte fixedBlue, Bg2BppColorMathRenderLayer? subscreen = null,
        bool subscreenUsesBg2 = false)
    {
        ArgumentNullException.ThrowIfNull(gameplay);
        if (subscreenUsesBg2 && !addSubscreen)
            throw new ArgumentException("BG2 subscreen requires subscreen arithmetic.", nameof(subscreenUsesBg2));
        if (lines.Length != SnesPpuLayout.ScreenHeightPixels)
            throw new ArgumentException("Gameplay color math requires one interval for each physical scanline.", nameof(lines));
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
