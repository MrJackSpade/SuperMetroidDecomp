using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ceres Ridley's Mode 7 register shadows as the getaway's room main (<c>$A6:AAAF</c>) last wrote them: M7A..M7D,
/// the rotation center and the BG1 offset, before room shake. <c>$80:91EE</c> uploads them with BGMODE at NMI.
/// </summary>
/// <param name="MatrixA">M7A.</param>
/// <param name="MatrixB">M7B.</param>
/// <param name="MatrixC">M7C.</param>
/// <param name="MatrixD">M7D.</param>
/// <param name="Center">M7X/M7Y.</param>
/// <param name="Offset">BG1HOFS/BG1VOFS.</param>
public readonly record struct CeresRidleyMode7Snapshot(
    ushort MatrixA, ushort MatrixB, ushort MatrixC, ushort MatrixD,
    Position16<Mode7Pixels> Center, Offset16<Mode7Pixels> Offset)
{
    /// <summary>The shadows of a getaway drawn in Mode 7, or null while Ceres Ridley is not.</summary>
    public static CeresRidleyMode7Snapshot? Capture(RidleyEnemyState? ridley) => ridley is { Mode7Active: true } getaway
        ? new(getaway.Mode7MatrixA, getaway.Mode7MatrixB, getaway.Mode7MatrixC, getaway.Mode7MatrixD,
            getaway.Mode7Center, getaway.Mode7Offset)
        : null;
}
