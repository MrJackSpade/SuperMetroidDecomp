namespace SuperMetroid.Core.Game;

/// <summary>The two instruction words past the Gravity list that Speed Booster reads as bank-$9B palette pointers, valued by the word.</summary>
internal enum SpeedBoosterPaletteOverrun : ushort
{
    /// <summary>$91:DAC7, the AD 68 prefix of LDA $0A68 immediately after the Gravity list; interpreted as a bank-$9B pointer.</summary>
    Expansion = 0x68ad,
    /// <summary>$91:DAC9, the 0A C9 instruction-boundary word following that prefix; interpreted as a bank-$9B pointer.</summary>
    GrappleCode = 0xc90a,
}

/// <summary>Bounded instruction-data reads when Gravity Speed Booster inherits Screw Attack phases eight or ten.</summary>
internal static class SpeedBoosterPaletteOverrunDefinitions
{

    /// <summary>$9B:C90A..C929, the exact bounded grapple-handler instruction slice read as sixteen colors by $91:DD5B.</summary>
    /// <remarks>This is executable-code identity, not an authored palette or a substitute normal shade.
    /// Phase ten selects it once before $91:DA97 limits subsequent phases. The native CPU fixture verifies every word.</remarks>
    internal static ushort GrappleInstructionWord(int index)
    {
        ReadOnlySpan<byte> code =
        [
            0xff, 0x00,             // End of AND #$00FF
            0xaa,                   // TAX
            0x89, 0xf0, 0x00,       // BIT #$00F0
            0xd0, 0xc9,             // BNE $C8DB
            0xbd, 0xba, 0xc9,       // LDA $C9BA,X
            0x29, 0xff, 0x00,       // AND #$00FF
            0x8d, 0x2c, 0x0a,       // STA $0A2C
            0x80, 0x38,             // BRA $C955
            0xad, 0x1c, 0x0a,       // LDA $0A1C
            0x0a, 0x0a, 0x0a,       // ASL three times
            0xaa,                   // TAX
            0xbf, 0x2c, 0xb6, 0x91, // LDA $91B62C,X
            0x29, 0xff,             // Start of AND #$00FF
        ];
        if ((uint)index >= code.Length / 2) throw new ArgumentOutOfRangeException(nameof(index));
        return (ushort)(code[index * 2] | code[index * 2 + 1] << 8);
    }
}
