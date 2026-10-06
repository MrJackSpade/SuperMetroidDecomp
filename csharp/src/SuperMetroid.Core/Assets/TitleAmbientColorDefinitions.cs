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
