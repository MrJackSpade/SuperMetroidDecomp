namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The two instruction words past the Gravity list that Speed Booster reads as bank-$9B palette pointers, valued by the word.</summary>
internal enum SpeedBoosterPaletteOverrun : ushort
{
    /// <summary>$91:DAC7, the AD 68 prefix of LDA $0A68 immediately after the Gravity list; interpreted as a bank-$9B pointer.</summary>
    Expansion = 0x68ad,
    /// <summary>$91:DAC9, the 0A C9 instruction-boundary word following that prefix; interpreted as a bank-$9B pointer.</summary>
    GrappleCode = 0xc90a,
}
