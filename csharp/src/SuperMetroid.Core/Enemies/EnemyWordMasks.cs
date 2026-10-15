namespace SuperMetroid.Core.Enemies;

/// <summary>Bit positions of a 16-bit cartridge word that enemy code tests or sets as a unit.</summary>
internal static class EnemyWordMasks
{
    /// <summary>Bit 15: the word's sign, or a flag stored in its sign position.</summary>
    internal const ushort SignBit = 0x8000;

    /// <summary>Bits 0-14 beneath <see cref="SignBit"/>.</summary>
    internal const ushort BelowSignBits = 0x7fff;

    /// <summary>Bit 7: the sign of the word's low byte.</summary>
    internal const ushort LowByteSignBit = 0x0080;

    /// <summary>The high byte that Sign_Extend_A ($A0:AFEA) ORs on when the low byte's sign is set; it never clears a borrowed one.</summary>
    internal const ushort SignExtendedHighByte = 0xff00;
}
