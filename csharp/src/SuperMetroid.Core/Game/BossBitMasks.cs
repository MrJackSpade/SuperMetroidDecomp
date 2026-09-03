namespace SuperMetroid.Core.Game;

/// <summary>
/// Validation and conversion boundary for the three boss-state bits whose retail meanings
/// are proven. The remaining five bits in each saved area byte remain losslessly preserved
/// by raw SRAM APIs, but they cannot silently enter named gameplay logic.
/// </summary>
public static class BossBitMasks
{
    /// <summary>Every currently proven flag in the cartridge's per-area boss byte.</summary>
    public const BossBits AllKnown =
        BossBits.AreaBoss | BossBits.AreaMiniBoss | BossBits.AreaTorizo;

    /// <summary>
    /// Converts a cartridge operand to a named mask and rejects bits without established
    /// semantics. Context is included so a malformed selector or PLM identifies its owner.
    /// </summary>
    public static BossBits FromCartridge(byte value, string context)
    {
        BossBits bits = (BossBits)value;
        Validate(bits, context);
        return bits;
    }

    /// <summary>Rejects unnamed flags passed through a typed gameplay API.</summary>
    public static void Validate(BossBits bits, string context)
    {
        BossBits unknown = bits & ~AllKnown;
        if (unknown != BossBits.None)
        {
            throw new ArgumentOutOfRangeException(
                nameof(bits),
                bits,
                $"{context} contains unproven boss bits ${(byte)unknown:X2}.");
        }
    }
}
