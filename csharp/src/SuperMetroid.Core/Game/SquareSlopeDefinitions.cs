namespace SuperMetroid.Core.Game;

/// <summary>Native 8-by-8 quadrant geometry for the five square-slope shapes.</summary>
public static class SquareSlopeDefinitions
{

    /// <summary>
    /// Reads the Samus solidity byte at native flattened index 0..19 (4*shape+quadrant).
    /// $94:8E54 encodes bottom half, right half, bottom-right quarter, all but top-left,
    /// and full block respectively. Top-left/right/bottom-left/right are quadrants 0..3.
    /// </summary>
    /// <remarks>
    /// Independently checked for #1165 against all twenty NTSC J/U v1.0 bytes and pinned
    /// bank_94.asm. Return exactly zero or $80. Callers retain BTS/XOR mirroring and
    /// leading-edge selection; reject unsupported indices as the previous span did.
    /// </remarks>
    public static byte ReadSamusQuadrant(int tableIndex)
    {
        if ((uint)tableIndex >= 20) throw new IndexOutOfRangeException();
        bool right = (tableIndex & 1) != 0;
        bool bottom = (tableIndex & 2) != 0;
        bool solid = (tableIndex / 4) switch
        {
            0 => bottom,
            1 => right,
            2 => bottom && right,
            3 => bottom || right,
            _ => true,
        };
        return solid ? (byte)0x80 : (byte)0;
    }

    /// <summary>
    /// Reads the enemy byte at native flattened index 0..19. $A0:C435 and $86:8729
    /// share the Samus geometry but retain quadrant identity in the two low bits.
    /// </summary>
    /// <remarks>
    /// Independently checked for #1165 against all twenty bytes in each NTSC copy
    /// and pinned bank_A0/bank_86.asm. Validation precedes masking, so invalid input
    /// cannot wrap into the valid domain. Solidity remains bit seven.
    /// </remarks>
    public static byte ReadEnemyQuadrant(int tableIndex) =>
        (byte)(ReadSamusQuadrant(tableIndex) | (tableIndex & 3));
}
