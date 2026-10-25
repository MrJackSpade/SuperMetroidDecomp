namespace SuperMetroid.Core.Hardware;

/// <summary>The three five-bit channels of a <see cref="RgbColor"/>, in packed order.</summary>
public enum ColorChannel : byte
{
    /// <summary>Bits 0-4.</summary>
    Red = 0,
    /// <summary>Bits 5-9.</summary>
    Green = 1,
    /// <summary>Bits 10-14.</summary>
    Blue = 2,
}
