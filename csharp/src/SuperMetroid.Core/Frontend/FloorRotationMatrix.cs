namespace SuperMetroid.Core.Frontend;

/// <summary>The rotating, zooming perspective floor the Ceres flight and destruction scenes show each frame.</summary>
internal static class FloorRotationMatrix
{
    /// <summary>
    /// The floor's transform for <paramref name="angle"/> and 8.8 <paramref name="zoom"/>, turning about
    /// <paramref name="center"/> and scrolled by <paramref name="scroll"/>: $8532 reads signed 8-bit sine words from
    /// $A0:B443 and keeps product bits 8..23, so reading the retail table makes the rotation agree at every one of 256
    /// angles.
    /// </summary>
    internal static AffineTransform Transform(RotationAngle angle, int zoom, Offset16<FloorPixels> center,
        Offset16<FloorPixels> scroll)
    {
        int a = Scale(ReadSine(angle.AddRaw(RotationAngle.QuarterTurn.RawValue).ToByteAngle()), zoom);
        int b = Scale(ReadSine(angle.ToByteAngle()), zoom);
        return new((short)a, (short)b, unchecked((short)-b), (short)a, center, scroll);
    }

    /// <summary>Reads the compiled signed sine sample for a floor angle.</summary>
    internal static short ReadSine(ByteAngle angle) => Trigonometry.SignedSine(angle);

    /// <summary>Multiplies a signed trigonometric component by the 8.8 scale, which stays below 32768, keeping signed 16 bits.</summary>
    private static short Scale(int component, int scalar) =>
        unchecked((short)(EightEight.Multiply(component, checked((short)scalar))));
}
