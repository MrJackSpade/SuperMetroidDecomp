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
    /// <summary>Raw $2101 OBSEL byte selecting OBJ character bases, name-table offset, and small/large size pair.</summary>
    public byte ObjectSelection { get; }
    /// <summary>Master brightness 0..15 applied after BG1, OBJ, and optional gradient color math.</summary>
    public byte Brightness { get; }
    /// <summary>Owned per-scanline fixed-color controls copied from the caller at snapshot construction.</summary>
    private readonly SuperMetroid.Core.Frontend.Title.TitleGradientLine[] gradient;
    /// <summary>
    /// Owned fixed-color controls indexed by visible scanline 0..223, or empty to disable title color math.
    /// $A1 subtracts from BG1/backdrop; $31 adds to BG1/backdrop and winning OBJ palettes 4..7.
    /// </summary>
    public ReadOnlySpan<SuperMetroid.Core.Frontend.Title.TitleGradientLine> Gradient => gradient;

    /// <summary>Captures a 256-by-224-pixel BG1/backdrop-then-OBJ composition, copying only the optional gradient.</summary>
    /// <param name="memory">Complete immutable PPU image; retained rather than recaptured from live hardware.</param>
    /// <param name="background">Mode 7 register values, or null to leave the backdrop below OBJ. Sampling uses screen X starting at zero and physical scanline one for the first visible row.</param>
    /// <param name="objectSelection">Unmodified OBSEL register byte; all byte values are retained.</param>
    /// <param name="brightness">Final master brightness from 0 through 15 inclusive; zero produces black.</param>
    /// <param name="gradient">Empty, or exactly 224 scanline controls with five-bit RGB components and CGADSUB control $A1 or $31; copied into owned storage.</param>
    /// <exception cref="ArgumentNullException"><paramref name="memory"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="brightness"/> exceeds 15.</exception>
    /// <exception cref="ArgumentException"><paramref name="gradient"/> has the wrong nonzero length, a color component above 31, or an unsupported control byte.</exception>
    public Mode7ObjRenderSnapshot(PpuMemorySnapshot memory, Mode7RenderRegisters? background,
        byte objectSelection, byte brightness, ReadOnlySpan<SuperMetroid.Core.Frontend.Title.TitleGradientLine> gradient = default)
    {
        Ensure.NotNull(memory);
        if (brightness > Hardware.SnesPpuLayout.MaximumMasterBrightness) throw new ArgumentOutOfRangeException(nameof(brightness));
        Memory = memory;
        Background = background;
        ObjectSelection = objectSelection;
        Brightness = brightness;
        if (!gradient.IsEmpty && gradient.Length != Hardware.SnesPpuLayout.ScreenHeightPixels)
            throw new ArgumentException("Title gradient requires 224 scanlines.", nameof(gradient));
        foreach (var line in gradient)
            if (line.Red > 31 || line.Green > 31 || line.Blue > 31 || line.Control is not (0xa1 or 0x31))
                throw new ArgumentException("Title gradient has invalid fixed color or unsupported CGADSUB control.", nameof(gradient));
        this.gradient = gradient.ToArray();
    }
}
