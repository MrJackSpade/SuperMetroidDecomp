namespace SuperMetroid.Core.Hardware;

/// <summary>Cartridge values and helpers of <see cref="SnesBusLayout"/> that only verification reads.</summary>
internal static class SnesBusLayoutConstants
{
    /// <summary>$2184: first unused B-bus register after the WRAM port registers.</summary>
    public const int ReservedBBusStart = 0x2184;
    /// <summary>$21FF: final address in the mirrored B-bus register window.</summary>
    public const int ReservedBBusEnd = 0x21ff;
    /// <summary>$2200: first A-bus expansion address; no device responds on Super Metroid's cartridge.</summary>
    public const int UnpopulatedExpansionStart = 0x2200;
    /// <summary>$3FFF: final expansion address before the CPU controller-register window.</summary>
    public const int UnpopulatedExpansionEnd = 0x3fff;
}
