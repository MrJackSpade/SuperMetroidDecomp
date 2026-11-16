using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Cartridge;

/// <summary>Cartridge values and helpers of <see cref="SnesBusLayout"/> that only verification reads.</summary>
internal static class SnesBusLayoutConstants
{
    /// <summary>The bytes of one bank.</summary>
    public const int BankBytes = 0x10000;
    /// <summary>Banks $7E and $7F hold the 128 KiB of WRAM.</summary>
    public const int FirstWorkRamBank = 0x7e;
    /// <summary>Whether <paramref name="bank"/> is one of the two WRAM banks.</summary>
    public static bool IsWorkRamBank(int bank) => bank is 0x7e or 0x7f;
    /// <summary>Whether <paramref name="bank"/> is a system bank ($00-$3F or $80-$BF), whose low window mirrors WRAM and the I/O.</summary>
    public static bool IsSystemBank(int bank) => bank is <= 0x3f or >= 0x80 and <= 0xbf;
}
