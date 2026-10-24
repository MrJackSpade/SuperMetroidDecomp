using SuperMetroid.Core.Rendering;

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
    internal static AffineTransform Transform(SnesAngle angle, int zoom, Offset16<Mode7Pixels> center,
        Offset16<Mode7Pixels> scroll)
    {
        short cosine = ReadSine(angle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex);
        short sine = ReadSine(angle.TableIndex);
        short a = Scale(cosine, zoom);
        short b = Scale(sine, zoom);
        return new(a, b, unchecked((short)-b), a, center, scroll);
    }

    /// <summary>Reads the compiled signed sine sample for a floor angle index.</summary>
    internal static short ReadSine(byte index) => EnemyTrigonometryTables.SignedSine(new SnesByteAngle(index));

    /// <summary>Multiplies a signed trigonometric component by the 8.8 scale, which stays below 32768, keeping signed 16 bits.</summary>
    private static short Scale(short component, int scalar) =>
        unchecked((short)(SnesEightEight.Multiply(component, checked((short)scalar))));
}
