namespace SuperMetroid.Core.Hardware;

/// <summary>Channel masks of a 15-bit SNES BGR555 CGRAM color word (bits 0-4 red, 5-9 green, 10-14 blue).</summary>
public static class SnesColorMasks
{
    /// <summary>Bits 5-9: the green channel.</summary>
    public const ushort Green = 0x03e0;

    /// <summary>Bits 5-14: the green and blue channels together, with red cleared.</summary>
    public const ushort GreenBlue = 0x7fe0;
}
