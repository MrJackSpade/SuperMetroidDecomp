using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Title tube shading and display paint bindings at $8D:C800-C873.</summary>
/// <remarks>The initial palette remains the independent paint owner. The selected three-unit
/// shade scale, rounded channel interpolation and dim-paint roles describe this animation;
/// no isolated green-channel exception or duplicate stock endpoint is stored.</remarks>
internal static class TitleAmbientColorDefinitions
{
    /// <summary>$8D:C800-C85B: nominal three-unit RGB5 dimming step, with shorter channels distributed to black.</summary>
    private const int ShadeStep = 3;
    /// <summary>$8D:C870: dim warm display paint reuses title palette slot3 ($8C:E1EF).</summary>
    private const int DimWarmPaint = 3;
    /// <summary>$8D:C872: dim green display paint reuses title palette slot19 ($8C:E20F).</summary>
    private const int DimGreenPaint = 19;

    /// <summary>Calculates a title tube-light or display color for a recognized animated palette operand.</summary>
    /// <param name="pointer">The native palette-word pointer being evaluated.</param>
    /// <param name="initial">The title palette's initial RGB5 colors.</param>
    /// <param name="supplied">Caller-provided edits to initial animated colors, keyed by their native pointers.</param>
    /// <param name="color">Receives the calculated RGB5 color, or zero when the pointer is not handled here.</param>
    /// <returns><see langword="true"/> when the pointer belongs to a supported tube-light or display animation.</returns>
    internal static bool TryCalculate(ushort pointer, ReadOnlySpan<ushort> initial,
        IReadOnlyDictionary<ushort, ushort> supplied, out ushort color)
    {
        var tube = TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.TubeLight;
        if (TryCoordinates(tube, pointer, out int frame, out int index))
        {
            ushort first = initial[tube.ColorByteIndex / sizeof(ushort) + index];
            if (frame == 0) { color = first; return true; }
            ushort firstPointer = (ushort)(tube.FirstFramePointer + sizeof(ushort) * (index + 1));
            if (supplied.TryGetValue(firstPointer, out ushort edited)) first = edited;
            int phase = Math.Min(frame, tube.FrameCount - frame);
            color = (ushort)(Channel(first & 31, phase) | Channel(first >> 5 & 31, phase) << 5 |
                Channel(first >> 10 & 31, phase) << 10);
            return true;
        }
        var displays = TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.Displays;
        if (TryCoordinates(displays, pointer, out frame, out index))
        {
            int paletteIndex = frame == 0 ? displays.ColorByteIndex / sizeof(ushort) + index
                : index == 0 ? DimWarmPaint : DimGreenPaint;
            color = initial[paletteIndex];
            return true;
        }
        color = 0;
        return false;
    }

    /// <summary>Interpolates one RGB5 channel toward its three-unit dim endpoint using nearest-even rounding.</summary>
    /// <param name="first">The channel intensity at the start of the dimming cycle.</param>
    /// <param name="phase">The cycle step used to calculate the current intensity.</param>
    /// <returns>The interpolated five-bit channel value.</returns>
    private static int Channel(int first, int phase)
    {
        int steps = Math.Min(TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.TubeDimmingSteps,
            (first + ShadeStep - 1) / ShadeStep);
        int endpoint = Math.Max(0, first - ShadeStep * steps);
        if (phase >= steps) return endpoint;
        int numerator = first * steps - (first - endpoint) * phase;
        int quotient = Math.DivRem(numerator, steps, out int remainder);
        // Exact rational nearest/even quantization: no floating-point contraction or rounding drift.
        return remainder * 2 > steps || (remainder * 2 == steps && (quotient & 1) != 0)
            ? quotient + 1 : quotient;
    }

    /// <summary>Maps an animated program's native word pointer to its frame and color indexes.</summary>
    /// <param name="program">The compiled animation layout whose color operands are being searched.</param>
    /// <param name="pointer">The native palette-word pointer to classify.</param>
    /// <param name="frame">Receives the zero-based frame index calculated from the pointer.</param>
    /// <param name="index">Receives the zero-based color index within that frame.</param>
    /// <returns><see langword="true"/> only when the pointer is word-aligned and addresses a color operand rather than a frame duration.</returns>
    private static bool TryCoordinates(TitleScreenAmbientPaletteFxProgramDefinition program,
        ushort pointer, out int frame, out int index)
    {
        int offset = pointer - program.FirstFramePointer - sizeof(ushort);
        frame = offset / program.FrameByteCount;
        index = offset % program.FrameByteCount / sizeof(ushort);
        return (uint)offset < program.FrameCount * program.FrameByteCount &&
            (offset & 1) == 0 && offset % program.FrameByteCount < program.ColorsPerFrame * sizeof(ushort);
    }
}
