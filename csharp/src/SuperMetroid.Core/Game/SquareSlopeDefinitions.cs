namespace SuperMetroid.Core.Game;

/// <summary>Native 8-by-8 quadrant definitions for the five square-slope shapes.</summary>
public static class SquareSlopeDefinitions
{
    /// <summary>
    /// $94:8E54, SquareSlopeDefinitions_Bank94 (kTab948E54). Four quadrants per
    /// shape in top-left, top-right, bottom-left, bottom-right order: zero is air,
    /// $80 is solid. BTS mirroring and leading-edge selection belong to the caller.
    /// </summary>
    /// <remarks>Issue #625 exact Boolean geometry: validate shape 0..4 and
    /// quadrant 0..3; let right=(q&amp;1)!=0, bottom=(q&amp;2)!=0. Solidity is,
    /// by shape, bottom, right, bottom AND right, bottom OR right, or true.
    /// Encode solid as $80 and air as zero. LookupTableResearch exhaustively
    /// checks all 20 bytes against the NTSC ROM and pinned bank_94.asm, including
    /// input bounds. Shape policy remains explicit; the four repeated quadrant
    /// samples need not be stored. Runtime behavior has not been changed.</remarks>
    public static ReadOnlySpan<byte> SamusQuadrants =>
    [
        0x00, 0x00, 0x80, 0x80,
        0x00, 0x80, 0x00, 0x80,
        0x00, 0x00, 0x00, 0x80,
        0x00, 0x80, 0x80, 0x80,
        0x80, 0x80, 0x80, 0x80,
    ];

    /// <summary>
    /// $A0:C435 / $86:8729, SquareSlopeDefinitions_BankA0 / Bank86. Enemy and
    /// enemy-projectile copies retain quadrant identity in bits zero and one;
    /// only bit seven denotes solidity. Keep the complete native bytes.
    /// </summary>
    /// <remarks>Issue #625 exact formula: use the bounded Boolean geometry
    /// documented on SamusQuadrants, then OR the result with quadrant q (0..3).
    /// LookupTableResearch proves all 20 bytes in EACH of the $A0 and $86 copies
    /// against the NTSC ROM, pinned assembly, and this span. Keeping q is essential
    /// even though current collision tests primarily consume the solidity bit.</remarks>
    public static ReadOnlySpan<byte> EnemyQuadrants =>
    [
        0x00, 0x01, 0x82, 0x83,
        0x00, 0x81, 0x02, 0x83,
        0x00, 0x01, 0x02, 0x83,
        0x00, 0x81, 0x82, 0x83,
        0x80, 0x81, 0x82, 0x83,
    ];
}
