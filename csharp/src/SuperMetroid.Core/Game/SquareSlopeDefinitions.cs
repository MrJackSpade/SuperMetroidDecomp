namespace SuperMetroid.Core.Game;

/// <summary>Native 8-by-8 quadrant definitions for the five square-slope shapes.</summary>
public static class SquareSlopeDefinitions
{
    /// <summary>
    /// $94:8E54, SquareSlopeDefinitions_Bank94 (kTab948E54). Four quadrants per
    /// shape in top-left, top-right, bottom-left, bottom-right order: zero is air,
    /// $80 is solid. BTS mirroring and leading-edge selection belong to the caller.
    /// </summary>
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
    public static ReadOnlySpan<byte> EnemyQuadrants =>
    [
        0x00, 0x01, 0x82, 0x83,
        0x00, 0x81, 0x02, 0x83,
        0x00, 0x01, 0x02, 0x83,
        0x00, 0x81, 0x82, 0x83,
        0x80, 0x81, 0x82, 0x83,
    ];
}
