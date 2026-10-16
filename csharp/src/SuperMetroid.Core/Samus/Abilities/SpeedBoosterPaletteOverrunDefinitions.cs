namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Bounded instruction-data reads when Gravity Speed Booster inherits Screw Attack phases eight or ten.</summary>
internal static class SpeedBoosterPaletteOverrunDefinitions
{
    /// <summary>
    /// Every colour of the expansion overrun: <c>$91:DD5B</c>'s unrolled <c>LDA $0000..$001E,X</c> runs under DB=$9B with
    /// X=$68AD, so it reads <c>$9B:68AD-$68CC</c> in the unconnected expansion window. Open bus returns the memory data
    /// register, which holds the operand's high byte, $00; all sixteen words are $0000.
    /// </summary>
    internal const ushort ExpansionOpenBusWord = 0x0000;

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
        return (ushort)(SnesLittleEndian.Word(code[index * 2], code[index * 2 + 1]));
    }
}
