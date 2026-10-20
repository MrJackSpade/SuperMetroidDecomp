namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ceres Ridley's Mode 7 register shadows as the getaway's room main (<c>$A6:AAAF</c>) last wrote them: M7A..M7D,
/// the rotation center and the BG1 offset, before room shake. <c>$80:91EE</c> uploads them with BGMODE at NMI.
/// </summary>
/// <param name="MatrixA">M7A, the signed 8.8 zoom-scaled cosine.</param>
/// <param name="MatrixB">M7B, the signed 8.8 zoom-scaled sine.</param>
/// <param name="MatrixC">M7C, the negated <paramref name="MatrixB"/>.</param>
/// <param name="MatrixD">M7D, equal to <paramref name="MatrixA"/>.</param>
/// <param name="Center">M7X/M7Y.</param>
/// <param name="Offset">BG1HOFS/BG1VOFS.</param>
public readonly record struct CeresRidleyMode7Snapshot(
    short MatrixA, short MatrixB, short MatrixC, short MatrixD,
    Position16<Mode7Pixels> Center, Offset16<Mode7Pixels> Offset)
{
    /// <summary>
    /// The shadows of a getaway drawn in Mode 7, or null while Ceres Ridley is not. Each coefficient is the
    /// two's-complement 8.8 value its shadow holds.
    /// </summary>
    public static CeresRidleyMode7Snapshot? Capture(RidleyEnemyState? ridley) => ridley is { Mode7Active: true } getaway
        ? new(getaway.Mode7MatrixA.SignedRawValue, getaway.Mode7MatrixB.SignedRawValue,
            getaway.Mode7MatrixC.SignedRawValue, getaway.Mode7MatrixD.SignedRawValue,
            getaway.Mode7Center, getaway.Mode7Offset)
        : null;
}
