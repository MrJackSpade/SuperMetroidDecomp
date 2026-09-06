namespace SuperMetroid.Core.Rendering;

/// <summary>Signed Mode 7 registers, retained without floating-point conversion.</summary>
public readonly record struct Mode7RenderRegisters(
    short MatrixA, short MatrixB, short MatrixC, short MatrixD,
    short CenterX, short CenterY, short HorizontalOffset, short VerticalOffset,
    bool FillOutsideWithCharacterZero = false);

/// <summary>
/// Immutable full-screen Mode 7/backdrop followed by OBJ and master brightness.
/// This composition is one supported packet shape, not a substitute for mixed-mode
/// gameplay, windows or main/subscreen color math.
/// </summary>
public sealed record Mode7ObjRenderSnapshot
{
    public PpuMemorySnapshot Memory { get; }
    /// <summary>Null disables BG1 without sampling its matrix, retaining the backdrop.</summary>
    public Mode7RenderRegisters? Background { get; }
    public byte ObjectSelection { get; }
    public byte Brightness { get; }

    public Mode7ObjRenderSnapshot(PpuMemorySnapshot memory, Mode7RenderRegisters? background,
        byte objectSelection, byte brightness)
    {
        ArgumentNullException.ThrowIfNull(memory);
        if (brightness > 15) throw new ArgumentOutOfRangeException(nameof(brightness));
        Memory = memory;
        Background = background;
        ObjectSelection = objectSelection;
        Brightness = brightness;
    }
}
