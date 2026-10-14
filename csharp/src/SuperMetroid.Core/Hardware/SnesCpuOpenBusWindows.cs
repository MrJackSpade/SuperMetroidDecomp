namespace SuperMetroid.Core.Hardware;

/// <summary>Proven undriven CPU address ranges, mirrored in banks $00-$3F/$80-$BF.</summary>
public static class SnesCpuOpenBusWindows
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
