namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Immutable full-screen Mode 7/backdrop followed by OBJ and master brightness.
/// This composition is one supported packet shape, not a substitute for mixed-mode
/// gameplay, windows or main/subscreen color math.
/// </summary>
public sealed record Mode7ObjRenderSnapshot
{
    /// <summary>Immutable VRAM, CGRAM, and finalized OAM image retained by reference without another memory copy.</summary>
    public PpuMemorySnapshot Memory { get; }
    /// <summary>Null disables BG1 without sampling its matrix, retaining the backdrop.</summary>
    public Mode7RenderRegisters? Background { get; }

    /// <summary>What each tile of the background's map shows, which it draws from (#1392); null while it draws from VRAM.</summary>
    public Mode7TilePictures? BackgroundTiles { get; init; }
    /// <summary>Master brightness 0..15 applied after BG1, OBJ, and optional gradient color math.</summary>
    public int Brightness { get; }
    /// <summary>Owned per-scanline fixed-color controls copied from the caller at snapshot construction.</summary>
    private readonly SuperMetroid.Core.Rendering.TitleGradientLine[] gradient;
    /// <summary>
    /// Owned fixed-color controls indexed by visible scanline 0..223, or empty to disable title color math.
    /// $A1 subtracts from BG1/backdrop; $31 adds to BG1/backdrop and winning OBJ palettes 4..7.
    /// </summary>
    public ReadOnlySpan<SuperMetroid.Core.Rendering.TitleGradientLine> Gradient => gradient;

    /// <summary>Captures a 256-by-224-pixel BG1/backdrop-then-OBJ composition, copying only the optional gradient.</summary>
    /// <param name="memory">Complete immutable PPU image; retained rather than recaptured from live hardware.</param>
    /// <param name="background">Mode 7 register values, or null to leave the backdrop below OBJ. Sampling uses screen X starting at zero and physical scanline one for the first visible row.</param>
    /// <param name="brightness">Final master brightness from 0 through 15 inclusive; zero produces black.</param>
    /// <param name="gradient">Empty, or exactly 224 scanline controls whose CGADSUB control subtracts from BG1 and the backdrop or adds to them and the OBJs; copied into owned storage.</param>
    /// <exception cref="ArgumentNullException"><paramref name="memory"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="brightness"/> is out of range.</exception>
    /// <exception cref="ArgumentException"><paramref name="gradient"/> has the wrong nonzero length or an unsupported control.</exception>
    /// <param name="backgroundTiles">What each tile of the background's map shows, which it draws from (#1392); null while it draws from VRAM.</param>
    public Mode7ObjRenderSnapshot(PpuMemorySnapshot memory, Mode7RenderRegisters? background,
        int brightness, ReadOnlySpan<SuperMetroid.Core.Rendering.TitleGradientLine> gradient = default,
        Mode7TilePictures? backgroundTiles = null)
    {
        BackgroundTiles = backgroundTiles;
        Ensure.NotNull(memory);
        Brightness = Ensure.BetweenInclusive(0, Hardware.SnesPpuLayout.MaximumMasterBrightness, brightness);
        Memory = memory;
        Background = background;
        if (!gradient.IsEmpty && gradient.Length != Hardware.SnesPpuLayout.ScreenHeightPixels)
            throw new ArgumentException("Title gradient requires 224 scanlines.", nameof(gradient));
        foreach (var line in gradient)
            if (!IsTitleGradientControl(line.Control))
                throw new ArgumentException("Title gradient has an unsupported CGADSUB control.", nameof(gradient));
        this.gradient = gradient.ToArray();
    }

    /// <summary>The two CGADSUB settings the title gradient uses: subtracting from BG1 and the backdrop, or adding to them and the OBJs.</summary>
    private static bool IsTitleGradientControl(ColorMathControlFlags control) => control is
        (ColorMathControlFlags.Subtract | ColorMathControlFlags.Backdrop | ColorMathControlFlags.Bg1) or
        (ColorMathControlFlags.Backdrop | ColorMathControlFlags.Obj | ColorMathControlFlags.Bg1);
}
