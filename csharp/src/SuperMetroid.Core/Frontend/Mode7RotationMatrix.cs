namespace SuperMetroid.Core.Frontend;

/// <summary>The Mode-7 rotation and scale matrix the Ceres flight and destruction scenes write each frame.</summary>
internal static class Mode7RotationMatrix
{
    /// <summary>
    /// The matrix for <paramref name="angle"/> and 8.8 <paramref name="zoom"/>: $8532 reads signed 8-bit sine words from
    /// $A0:B443 and keeps product bits 8..23, so reading the retail table makes the rotation agree at every one of 256
    /// angles.
    /// </summary>
    internal static Mode7MatrixParameters Calculate(SnesAngle angle, int zoom)
    {
        short cosine = ReadSine(angle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex);
        short sine = ReadSine(angle.TableIndex);
        short a = Scale(cosine, zoom);
        short b = Scale(sine, zoom);
        return new(a, b, unchecked((short)-b), a);
    }

    /// <summary>Reads the compiled signed sine sample for a Mode-7 angle index.</summary>
    internal static short ReadSine(byte index) => EnemyTrigonometryTables.SignedSine(new SnesByteAngle(index));

    /// <summary>Multiplies a signed trigonometric component by the 8.8 scale, which stays below 32768, keeping signed 16 bits.</summary>
    private static short Scale(short component, int scalar) =>
        unchecked((short)(SnesEightEight.Multiply(component, checked((short)scalar))));
}
