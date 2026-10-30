namespace SuperMetroid.Core.Frontend;

/// <summary>The rotating, zooming perspective floor the Ceres flight and destruction scenes show each frame.</summary>
internal static class FloorRotationMatrix
{
    /// <summary>
    /// The floor's transform for <paramref name="angle"/> and 8.8 <paramref name="zoom"/>, turning about
    /// <paramref name="center"/> and scrolled by <paramref name="scroll"/>. Each coefficient is the angle's signed sine
    /// sample scaled by the zoom and rounded down to whole 8.8 steps: the floor turns through the same 256 angles, at the
    /// same coarseness, as the screen shows, so every frame's floor lands on the same pixels.
    /// </summary>
    internal static AffineTransform Transform(RotationAngle angle, int zoom, Offset16<FloorPixels> center,
        Offset16<FloorPixels> scroll)
    {
        int a = Scale(ReadSine(angle.AddRaw(RotationAngle.QuarterTurn.RawValue).ToByteAngle()), zoom);
        int b = Scale(ReadSine(angle.ToByteAngle()), zoom);
        return new((short)a, (short)b, unchecked((short)-b), (short)a, center, scroll);
    }

    /// <summary>The signed sine sample of a floor angle.</summary>
    internal static short ReadSine(ByteAngle angle) => Trigonometry.SignedSine(angle);

    /// <summary>
    /// A signed sine sample scaled by the 8.8 zoom, which stays below 32768, rounded down to a whole 8.8 coefficient that
    /// fits a signed sixteen-bit coefficient.
    /// </summary>
    private static short Scale(int component, int scalar) =>
        unchecked((short)(EightEight.Multiply(component, checked((short)scalar))));
}
