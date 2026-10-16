namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Raw COLDATA bytes, including each component's select bit, as one producer published them
/// for an accepted NMI. NMI <c>$80:91EE</c> uploads the WRAM shadows to <c>$2132</c>, so the
/// main loop's writes become visible only with the following displayed frame.
/// </summary>
/// <param name="Red">Raw COLDATA red byte; the low five bits are its intensity.</param>
/// <param name="Green">Raw COLDATA green byte; the low five bits are its intensity.</param>
/// <param name="Blue">Raw COLDATA blue byte; the low five bits are its intensity.</param>
public readonly record struct FixedColorRegisterSnapshot(byte Red, byte Green, byte Blue)
{
    /// <summary>The five-bit intensity of <see cref="Red"/>.</summary>
    public byte RedIntensity => (byte)(Red & 31);

    /// <summary>The five-bit intensity of <see cref="Green"/>.</summary>
    public byte GreenIntensity => (byte)(Green & 31);

    /// <summary>The five-bit intensity of <see cref="Blue"/>.</summary>
    public byte BlueIntensity => (byte)(Blue & 31);
}
