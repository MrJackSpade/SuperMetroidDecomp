namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The unsigned native 8.8 layout (#223): a word whose high byte is whole units and low byte is 1/256ths, as
/// radii, speeds and accelerations the cartridge advances by a fraction each frame keep it.
/// </summary>
public static class SnesUnsignedEightEight
{
    /// <summary>The whole units of an unsigned 8.8 word.</summary>
    public static byte Whole(ushort value) => unchecked((byte)(value >> 8));
}
