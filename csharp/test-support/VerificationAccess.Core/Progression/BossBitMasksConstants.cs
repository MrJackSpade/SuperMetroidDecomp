namespace SuperMetroid.Core.Progression;

/// <summary>Cartridge values and helpers of <see cref="BossBitMasks"/> that only verification reads.</summary>
internal static class BossBitMasksConstants
{
    /// <summary>
    /// Converts a cartridge operand to a named mask and rejects bits without established
    /// semantics. Context is included so a malformed selector or PLM identifies its owner.
    /// </summary>
    public static BossBits FromCartridge(byte value, string context)
    {
        BossBits bits = (BossBits)value;
        BossBitMasks.Validate(bits, context);
        return bits;
    }
}
