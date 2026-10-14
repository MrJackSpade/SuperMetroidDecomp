using SuperMetroid.Core.Hardware;

/// <summary>Reference arithmetic for the cartridge's random-number generator.</summary>
internal static class AssertionsVerification
{
    /// <summary>The cartridge's next random word for <paramref name="seed"/>, computed independently.</summary>
    internal static ushort ReferenceNextRandom(ushort seed)
    {
    int hardwareProductLow = (SnesLittleEndian.LowByte(seed)) * 5;
    byte stackLow = (byte)hardwareProductLow;
    byte stackHigh = SnesLittleEndian.HighByte(unchecked((ushort)hardwareProductLow));

    int hardwareProductHigh = SnesLittleEndian.HighByte(seed) * 5;
    int eightBitAdc = stackHigh + unchecked((byte)hardwareProductHigh) + 1;
    stackHigh = (byte)eightBitAdc;
    int carry = eightBitAdc > 0xff ? 1 : 0;

    int restoredAccumulator = SnesLittleEndian.Word(stackLow, stackHigh);
    return unchecked((ushort)(restoredAccumulator + 0x0011 + carry));
    }
}
