namespace SuperMetroid.Core.Game;

/// <summary>Signed claw displacements used by Ridley's grab collision and carried-Samus placement.</summary>
public static class RidleyClawOffsets
{
    /// <summary>
    /// The six bounded adjacent-instruction observations at $A6:B9E1..B9EC,
    /// MoveSamusWithRidleyFeet, retained as the precise nonsense boundary under #1165.
    /// </summary>
    /// <remarks>
    /// Bytes AF 28 78 7E F0 1F 85 12 10 04 49 FF are LDA.l $7E7828,
    /// BEQ +$1F, STA $12, BPL +$04, and EOR #$FFFF through its low operand.
    /// They form words $28AF,$7E78,$1FF0,$1285,$0410,$FF49; they are not six
    /// additional claw positions. Physical claw geometry cannot derive unrelated CPU
    /// opcodes, RAM operands and branch encodings without re-encoding that machine code.
    /// ReadY exposes only this pre-existing managed window via min(index &gt;&gt; 1,8).
    /// This is neither a native bounds rule nor unbounded memory/CPU emulation.
    /// The three ordinary geometry words remain calculated independently.
    /// </remarks>
    private static ReadOnlySpan<short> ExistingOutOfRangeWindow =>
        [0x28af, 0x7e78, 0x1ff0, 0x1285, 0x0410, -183];

    /// <summary>$A6:B9D5 HoldingSamusXDispacement: left/turning/right displacements decrease twelve pixels per facing; preserves the host clamp.</summary>
    public static short ReadX(ushort facing) => (short)(12 - 12 * Math.Min(facing, (ushort)2));

    /// <summary>$A6:B9DB HoldingSamusYDispacement: three foot heights advance 10.5 pixels rounded upward; preserves word indexing and the adjacent host window.</summary>
    public static short ReadY(ushort feetDistanceIndex)
    {
        int index = Math.Min(feetDistanceIndex >> 1, 8);
        return index < 3 ? (short)(35 + (21 * index + 1) / 2) : ExistingOutOfRangeWindow[index - 3];
    }
}
