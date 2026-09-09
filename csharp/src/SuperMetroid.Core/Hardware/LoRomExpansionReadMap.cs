namespace SuperMetroid.Core.Hardware;

/// <summary>Unconnected expansion window on this cartridge's ordinary LoROM board.</summary>
internal static class LoRomExpansionReadMap
{
    /// <summary>Banks $80-$BF mirror the system-bank expansion window of $00-$3F.</summary>
    public const int MirrorBankMask = 0x7f;
    /// <summary>First non-system bank after the $00-$3F expansion windows.</summary>
    public const int SystemBankLimit = 0x40;
    /// <summary>Start of the unconnected $6000-$7FFF cartridge expansion window.</summary>
    public const int ExpansionStart = 0x6000;
    /// <summary>ROM drives the data bus beginning at $8000 in system banks.</summary>
    public const int RomStart = 0x8000;
}
