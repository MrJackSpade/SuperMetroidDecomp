namespace SuperMetroid.Core.Rendering;

/// <summary>
/// The codec between <see cref="ColorMathControlFlags"/> and the CGADSUB register byte that authored title-gradient
/// documents and render packets store. This type is the one place the typed control becomes that byte; the import
/// side decodes it by casting the byte to the flags.
/// </summary>
[SnesEncodingOwner]
public static class ColorMathControlFlagsCodec
{
    /// <summary>The control as the CGADSUB register byte that stores it.</summary>
    /// <param name="control">The layers colour math applies to and how it combines.</param>
    public static byte ToNativeByte(this ColorMathControlFlags control) => (byte)control;
}
