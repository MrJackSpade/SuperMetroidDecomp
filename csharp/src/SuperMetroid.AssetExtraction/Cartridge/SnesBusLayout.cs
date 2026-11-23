using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Cartridge;

/// <summary>
/// The SNES CPU-bus layout this LoROM cartridge sees (#1297): the WRAM and save-RAM banks, the system banks' low
/// windows, the ROM window and the reserved windows no device drives.
/// </summary>
public static partial class SnesBusLayout
{
    /// <summary>The highest 24-bit bus address.</summary>
    public const int MaximumAddress = 0x00ff_ffff;
    /// <summary>ROM drives the data bus from $8000 in every cartridge bank.</summary>
    public const int RomWindowStart = 0x8000;
    /// <summary>System banks $00-$3F/$80-$BF mirror the first 8 KiB of bank-$7E WRAM through $1FFF.</summary>
    public const int WorkRamMirrorEnd = 0x2000;
    /// <summary>Banks $80-$BF mirror the system-bank expansion window of $00-$3F.</summary>
    public const int MirrorBankMask = 0x7f;
    /// <summary>First non-system bank after the $00-$3F expansion windows.</summary>
    public const int SystemBankLimit = 0x40;

}
